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
            // configuração do JWT bearer
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKeyResolver = (s, securityToken, identifier, parameters) =>
                {
                    var json = new WebClient().DownloadString($"https://cognito-idp.us-east-2.amazonaws.com/us-east-2_CT2bI5jE6/.well-known/jwks.json");
                    var keys = JsonConvert.DeserializeObject<Jwks>(json)?.Keys;

                    var selectedKey = keys?.FirstOrDefault(k => k.KeyId == identifier);
                    if (selectedKey == null)
                    {
                        throw new SecurityTokenSignatureKeyNotFoundException("Matching key not found.");
                    }

#pragma warning disable CA1416 // Validate platform compatibility
                    var rsa = new RSACng();
                    rsa.ImportParameters(new RSAParameters
                    {
                        Exponent = Base64Url.Decode(selectedKey.Exponent),
                        Modulus = Base64Url.Decode(selectedKey.Modulus)
                    });
#pragma warning restore CA1416 // Validate platform compatibility

                    return new List<SecurityKey> { new RsaSecurityKey(rsa) };
                }
            };
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var claimsIdentity = (ClaimsIdentity?)context?.Principal?.Identity;
                    var username = claimsIdentity?.Claims?.FirstOrDefault(x => x.Type.Contains(Cognito.ID))?.Value;

                    var cognitoClient = new AmazonCognitoIdentityProviderClient(builder.Configuration["Cognito:AccessKey"], builder.Configuration["Cognito:SecretKey"], RegionEndpoint.USEast2);

                    var user = await cognitoClient.AdminGetUserAsync(new AdminGetUserRequest
                    {
                        UserPoolId = builder.Configuration["Cognito:UserPoolId"],
                        Username = username
                    });

                    if (user != null)
                    {
                        if (user.UserAttributes != null)
                        {
                            if (user.UserAttributes.Any(a => a.Name == "email"))
                                claimsIdentity?.AddClaim(new Claim("cognito:email", user.UserAttributes.First(a => a.Name == "email").Value));

                            if (user.UserAttributes.Any(a => a.Name == "name"))
                                claimsIdentity?.AddClaim(new Claim("cognito:name", user.UserAttributes.First(a => a.Name == "name").Value));

                            if (user.UserAttributes.Any(a => a.Name == "phone_number"))
                                claimsIdentity?.AddClaim(new Claim("cognito:phone_number", user.UserAttributes.First(a => a.Name == "phone_number").Value));
                        }
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