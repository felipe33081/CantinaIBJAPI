using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using CantinaIBJ.Data.Context;
using CantinaIBJ.Model.AppSettings;
using CantinaIBJ.WebApi.Configurations;
using CantinaIBJ.WebApi.Models;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using static CantinaIBJ.WebApi.Common.Constants;

var builder = WebApplication.CreateBuilder(args);
IServiceCollection services = builder.Services;

services.AddEntityFrameworkNpgsql()
    .AddDbContext<PostgreSqlContext>(options => options.UseNpgsql(
        builder.Configuration.GetConnectionString("POSTGRESQLCONNSTR_PostgreSQL")
        ));

//Configura o appsettings
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var host = Host
    .CreateDefaultBuilder()
#if DEBUG
    .UseEnvironment("development")
#endif
    .ConfigureAppConfiguration(app => app.AddConfiguration(config))
    .ConfigureLogging(b =>
    {
        b.AddConsole();
    });

services.Configure<SendEmailSettings>(config.GetSection("SendEmail"));
services.Configure<CognitoSettings>(config.GetSection("Cognito"));
services.Configure<JwtSettings>(config.GetSection("Jwt"));
services.Configure<HashPasswordSettings>(config.GetSection("HashPassword"));

services.AddMvcCore()
   .AddAuthorization()
   .AddDataAnnotations();

services.ConfigureRepositories();

//services.ConfigureAuth();

// configura autenticação jwt.
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
                    var keys = JsonConvert.DeserializeObject<Jwks>(json).Keys;

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
        }).AddAuthenticationSchemes()
        .RequireAuthenticatedUser()
        .Build());

    options.AddPolicy(Policy.ADMIN, new AuthorizationPolicyBuilder()
        .RequireAssertion(context =>
        {
            return context.User.Claims.Any(c => c.Type == Cognito.GROUPS &&
                                                new List<string> { Group.MASTERADMIN, Group.ADMIN }.Contains(c.Value)) ? true : false;
        }).AddAuthenticationSchemes()
        .RequireAuthenticatedUser()
        .Build());

    options.AddPolicy(Policy.USER, new AuthorizationPolicyBuilder()
        .RequireAssertion(context =>
        {
            return context.User.Claims.Any(c => c.Type == Cognito.GROUPS &&
                                                new List<string> { Group.USER, Group.ADMIN, Group.MASTERADMIN }.Contains(c.Value)) ? true : false;
        }).AddAuthenticationSchemes()
        .RequireAuthenticatedUser()
        .Build());
});

services.AddMemoryCache();

services.AddCors(options =>
    {
        options.AddPolicy("AllowAnyOrigin",
            builder => builder.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader());
    });

services.AddDistributedMemoryCache();

services.AddControllers();
services.AddEndpointsApiExplorer();

//

services.AddMvcCore()
    .AddAuthorization()
    .AddDataAnnotations();

//Configura os serviços
services.ConfigureServices();

//Configura Swagger com autenticação Jwt
services.ConfigureSwaggerGen();

services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSwagger(x =>
{
    x.RouteTemplate = "docs/{documentName}/docs.json";
});
app.UseSwaggerUI(o =>
{
    o.RoutePrefix = "docs";
    o.SwaggerEndpoint("/docs/v1/docs.json", "Cantina IBJ");
    o.DefaultModelsExpandDepth(-1);
    o.DocumentTitle = "API";
    o.DisplayRequestDuration();
    o.EnableValidator(null);
});

#if DEBUG
app.UseDeveloperExceptionPage();
#endif

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor |
    ForwardedHeaders.XForwardedProto
});

var cultureInfo = new CultureInfo("pt-BR");
cultureInfo.NumberFormat.CurrencySymbol = "R$";

CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

app.UseRouting();
app.UseCors(policy =>
{
    policy.AllowAnyOrigin();
    policy.AllowAnyHeader();
    policy.AllowAnyMethod();
});

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Access-Control-Expose-Headers", "X-Total-Count");
    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Configuração para ignorar validação de certificado SSL/TLS
ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;