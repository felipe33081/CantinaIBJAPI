using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using CantinaIBJ.Data.Contracts;
using CantinaIBJ.Data.Contracts.Customer;
using CantinaIBJ.Data.Contracts.Dashboard;
using CantinaIBJ.Data.Repositories;
using CantinaIBJ.Data.Repositories.Customer;
using CantinaIBJ.Data.Repositories.Dashboard;
using CantinaIBJ.Integration.Cognito;
using CantinaIBJ.Integration.WhatsGW;
using CantinaIBJ.Model.AppSettings;
using CantinaIBJ.WebApi.Common;
using CantinaIBJ.WebApi.Helpers;
using CantinaIBJ.WebApi.Mapper;
using CantinaIBJ.WebApi.Models;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Xml.Linq;
using static CantinaIBJ.WebApi.Common.Constants;

namespace CantinaIBJ.WebApi.Configurations;

public static class ServicesConfiguration
{
    public static IServiceCollection ConfigureRepositories(this IServiceCollection services)
    {
        services.AddScoped<ICustomerPersonRepository, CustomerPersonRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductHistoricRepository, ProductHistoricRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();

        return services;
    }

    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfigurationRoot config)
    {
        services.Configure<WhatsGWSettings>(config.GetSection("WhatsGW"));
        services.Configure<SendEmailSettings>(config.GetSection("SendEmail"));
        services.Configure<CognitoSettings>(config.GetSection("Cognito"));

        services.AddScoped<Mappers>();
        services.AddScoped<MapperProfile>();

        services.AddScoped<HttpUserContext>();
        services.AddHttpContextAccessor();

        services.AddScoped<OrderHelper>();
        services.AddScoped<SmtpHelper>();

        services.AddScoped<ValidateModelAttribute>();

        services.AddScoped<CognitoSettings>();

        services.AddScoped<ICognitoCommunication, CognitoCommunication>();

        services.AddScoped<IWhatsGWService, WhatsGWCommunication>();

        return services;
    }

    public static void ConfigureSwaggerGen(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Cantina IBJ",
                Version = $"v1",
                Description = "API que permite um controle sobre as vendas de uma cantina",
                Contact = new OpenApiContact
                {
                    Email = "felipenogueirap7@gmail.com",
                    Name = "Felipe Nogueira"
                }
            });
            c.AddSecurityDefinition("ApiKey",
                new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Token JWT obtido a partir da camada de autenticação",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
                        },
                        new string[] { }
                    }
                });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "oauth2"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });
            c.UseOneOfForPolymorphism();

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
            c.SchemaFilter<DescribeEnumMembers>(XDocument.Load(xmlPath));
            c.SchemaFilter<IgnoreEnumSchemaFilter>(XDocument.Load(xmlPath));
        });
    }

    public static void AddAuthenticationCIBJ(this IServiceCollection services, WebApplicationBuilder builder)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            // 1. A MÁGICA: O Authority baixa as chaves (jwks) sozinho e valida a assinatura
            // Garanta que a variável Jwt:Issuer no Railway NÃO tenha barra no final
            options.Authority = builder.Configuration["Jwt:Issuer"];

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],

                ValidateAudience = false, // Mantive desligado como no seu, mas ideal é ligar no futuro
                ValidateLifetime = true,

                // Removemos aquele bloco gigante do IssuerSigningKeyResolver
                // O options.Authority já faz aquilo automaticamente
            };

            options.Events = new JwtBearerEvents
            {
                // DICA DE DEBUG: Se der erro, esse evento mostra o porquê no log
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine($"Token Falhou: {context.Exception.Message}");
                    return Task.CompletedTask;
                },

                OnTokenValidated = async context =>
                {
                    // ATENÇÃO: Se as variáveis Cognito__AccessKey ou Cognito__SecretKey
                    // estiverem erradas no Railway, ESTE BLOCO VAI QUEBRAR O LOGIN.
                    try
                    {
                        var claimsIdentity = (ClaimsIdentity?)context?.Principal?.Identity;
                        var username = claimsIdentity?.Claims?.FirstOrDefault(x => x.Type == "username" || x.Type == "cognito:username")?.Value;

                        // Ajuste para pegar o username correto do token se vier nulo acima
                        if (string.IsNullOrEmpty(username))
                        {
                            username = claimsIdentity?.Claims?.FirstOrDefault(x => x.Type.EndsWith("username"))?.Value;
                        }

                        if (!string.IsNullOrEmpty(username))
                        {
                            var cognitoClient = new AmazonCognitoIdentityProviderClient(
                                builder.Configuration["Cognito:AccessKey"],
                                builder.Configuration["Cognito:SecretKey"],
                                RegionEndpoint.USEast2);

                            var user = await cognitoClient.AdminGetUserAsync(new AdminGetUserRequest
                            {
                                UserPoolId = builder.Configuration["Cognito:UserPoolId"],
                                Username = username
                            });

                            if (user != null && user.UserAttributes != null)
                            {
                                // Adicionando claims extras
                                var email = user.UserAttributes.FirstOrDefault(a => a.Name == "email")?.Value;
                                if (!string.IsNullOrEmpty(email))
                                    claimsIdentity?.AddClaim(new Claim("cognito:email", email));

                                var name = user.UserAttributes.FirstOrDefault(a => a.Name == "name")?.Value;
                                if (!string.IsNullOrEmpty(name))
                                    claimsIdentity?.AddClaim(new Claim("cognito:name", name));

                                var phone_number = user.UserAttributes.FirstOrDefault(a => a.Name == "phone_number")?.Value;
                                if (!string.IsNullOrEmpty(phone_number))
                                    claimsIdentity?.AddClaim(new Claim("cognito:phone_number", phone_number));

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Se der erro na AWS, logamos mas NÃO derrubamos o token. 
                        // Assim o usuário loga, mesmo sem os dados extras.
                        Console.WriteLine($"Erro ao buscar dados no Cognito: {ex.Message}");
                    }
                }
            };
        });

        // Configuração das políticas de autorização
        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policy.MASTERADMIN, new AuthorizationPolicyBuilder()
                .RequireAssertion(context =>
                {
                    return context.User.Claims.Any(c => c.Type == Cognito.GROUPS &&
                                                        c.Value == Policy.MASTERADMIN) ? true : false;
                })
                .AddAuthenticationSchemes()
                .RequireAuthenticatedUser()
                .Build());

            options.AddPolicy(Policy.ADMIN, new AuthorizationPolicyBuilder()
                .RequireAssertion(context =>
                {
                    return context.User.Claims.Any(c => c.Type == Cognito.GROUPS &&
                                                        new List<string> { Group.MASTERADMIN, Group.ADMIN }.Contains(c.Value)) ? true : false;
                })
                .AddAuthenticationSchemes()
                .RequireAuthenticatedUser()
                .Build());

            options.AddPolicy(Policy.USER, new AuthorizationPolicyBuilder()
                .RequireAssertion(context =>
                {
                    return context.User.Claims.Any(c => c.Type == Cognito.GROUPS &&
                                                        new List<string> { Group.USER, Group.ADMIN, Group.MASTERADMIN }.Contains(c.Value)) ? true : false;
                })
                .AddAuthenticationSchemes()
                .RequireAuthenticatedUser()
                .Build());
        });

    }
}