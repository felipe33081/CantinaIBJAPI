using CantinaIBJ.Data.Contracts;
using CantinaIBJ.Data.Contracts.Customer;
using CantinaIBJ.Data.Repositories;
using CantinaIBJ.Data.Repositories.Customer;
using CantinaIBJ.WebApi.Common;
using CantinaIBJ.WebApi.Interfaces;
using CantinaIBJ.WebApi.Mapper;
using CantinaIBJ.WebApi.Services;
using Microsoft.OpenApi.Models;

namespace CantinaIBJ.WebApi.Configurations;

public static class ServicesConfiguration
{
    public static IServiceCollection ConfigureRepositories(this IServiceCollection services)
    {
        services.AddScoped<Mappers>();
        services.AddScoped<MapperProfile>();

        services.AddScoped<HttpUserContext>();
        services.AddHttpContextAccessor();

        //Jwt
        services.AddScoped<IJwtService, JwtService>();

        services.AddScoped<ICustomerPersonRepository, CustomerPersonRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProductHistoricRepository, ProductHistoricRepository>();

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

            //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            //c.IncludeXmlComments(xmlPath);
            //c.SchemaFilter<DescribeEnumMembers>(XDocument.Load(xmlPath));
            //c.SchemaFilter<IgnoreEnumSchemaFilter>(XDocument.Load(xmlPath));

            //xmlPath = Path.Combine(AppContext.BaseDirectory, "Risk.Integration.xml");
            //c.IncludeXmlComments(xmlPath);
            //c.SchemaFilter<DescribeEnumMembers>(XDocument.Load(xmlPath));
            //c.SchemaFilter<IgnoreEnumSchemaFilter>(XDocument.Load(xmlPath));

            //xmlPath = Path.Combine(AppContext.BaseDirectory, "Risk.Model.xml");
            //c.IncludeXmlComments(xmlPath);
            //c.SchemaFilter<DescribeEnumMembers>(XDocument.Load(xmlPath));
            //c.SchemaFilter<IgnoreEnumSchemaFilter>(XDocument.Load(xmlPath));
        });
    }
}