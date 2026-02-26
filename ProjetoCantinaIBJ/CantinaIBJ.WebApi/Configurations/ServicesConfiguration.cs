using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using CantinaIBJ.Data.Contracts;
using CantinaIBJ.Data.Contracts.Customer;
using CantinaIBJ.Data.Contracts.Dashboard;
using CantinaIBJ.Data.Repositories;
using CantinaIBJ.Data.Repositories.Customer;
using CantinaIBJ.Data.Repositories.Dashboard;
using CantinaIBJ.Integration.WhatsGW;
using CantinaIBJ.Model.AppSettings;
using CantinaIBJ.Model.Interfaces;
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

        services.AddScoped<Mappers>();
        services.AddScoped<MapperProfile>();

        services.AddHttpContextAccessor();

        services.AddScoped<OrderHelper>();
        services.AddScoped<SmtpHelper>();

        services.AddScoped<ValidateModelAttribute>();

        services.AddScoped<IWhatsGWService, WhatsGWCommunication>();

        services.AddScoped<IPrinterService, PrinterService>();

        return services;
    }

    public static void ConfigureSwaggerGen(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "F&S Software solutions",
                Version = $"v0.1",
                Description = "API que permite um controle sobre suas vendas",
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

}