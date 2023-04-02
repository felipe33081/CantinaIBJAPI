using CantinaIBJ.Data.Context;
using CantinaIBJ.WebApi.Configurations;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.Extensions.DependencyInjection;
using CantinaIBJ.WebApi.Helpers;

var builder = WebApplication.CreateBuilder(args);
IServiceCollection services = builder.Services;

// Add services to the container.
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

services.AddControllers();

services.AddEntityFrameworkNpgsql()
    .AddDbContext<PostgreSqlContext>(options => options.UseNpgsql(
        builder.Configuration.GetConnectionString("POSTGRESQLCONNSTR_PostgreSQL")
        ));

//services.AddIdentityServer()
//            .AddInMemoryClients(new List<Client>() {
//                new Client {
//                    ClientId = "meu-cliente",
//                    AllowedGrantTypes = GrantTypes.ClientCredentials,
//                    ClientSecrets = { new Secret(builder.Configuration["IdentityServer:ClientSecret"].Sha256()) },
//                    AllowedScopes = { "minha-api" }
//                }
//            })
//            .AddInMemoryApiScopes(new List<ApiScope>() {
//                new ApiScope("minha-api", "Minha API")
//            })
//            .AddInMemoryApiResources(new List<ApiResource>() {
//                new ApiResource("minha-api", "Minha API")
//            })
//            .AddTestUsers(new List<TestUser>() {
//                new TestUser {
//                    SubjectId = "1",
//                    Username = "alice",
//                    Password = "alice",
//                    Claims = {
//                        new Claim("name", "Alice"),
//                        new Claim("website", "https://alice.com")
//                    }
//                }
//            });

services.AddEndpointsApiExplorer();

services.ConfigureSwaggerGen();

services.AddMvcCore()
    .AddAuthorization()
    .AddDataAnnotations();

//Configura os Repositórios
services.ConfigureRepositories();

services.AddMemoryCache();
services.AddCors(c =>
{
    c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});
services.AddDistributedMemoryCache();

services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

var cultureInfo = new CultureInfo("pt-BR");
cultureInfo.NumberFormat.CurrencySymbol = "R$";

CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

app.UseSwagger(x =>
{
    x.RouteTemplate = "docs/{documentName}/docs.json";
});
app.UseSwaggerUI(o =>
{
    o.RoutePrefix = "docs";
    o.SwaggerEndpoint("/docs/v1/docs.json", "Cantina IBJ");
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

#if DEBUG
app.UseDeveloperExceptionPage();
#endif
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor |
    ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();
app.UseStaticFiles();

// Configure o middleware de roteamento
app.UseRouting();

// Configure o middleware de pontos de extremidade
//app.UseIdentityServer();

app.UseCors(policy =>
{
    policy.AllowAnyOrigin();
    policy.AllowAnyHeader();
    policy.AllowAnyMethod();
});

app.UseAuthorization();
app.UseAuthentication();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

app.UseCors(options =>
{
    options.WithOrigins("http://localhost:3000")
    .AllowAnyMethod()
    .AllowAnyHeader();
});

app.UseDeveloperExceptionPage();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});