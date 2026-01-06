using CantinaIBJ.Data.Context;
using CantinaIBJ.Integration.WhatsGW;
using CantinaIBJ.Model.AppSettings;
using CantinaIBJ.WebApi.Configurations;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
IServiceCollection services = builder.Services;

services.AddEntityFrameworkNpgsql()
    .AddDbContext<PostgreSqlContext>(options => options.UseNpgsql(
        builder.Configuration.GetConnectionString("POSTGRESQLCONNSTR_PostgreSQL")
        ));

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

services.AddHealthChecks();

services.AddMvcCore()
   .AddAuthorization()
   .AddDataAnnotations();

services.ConfigureRepositories();

services.AddAuthenticationCIBJ(builder);

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

services.ConfigureServices(config);

services.ConfigureSwaggerGen();

services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseHealthChecks("/health");

app.UseSwagger(x =>
{
    x.RouteTemplate = "docs/{documentName}/docs.json";
});
app.UseSwaggerUI(o =>
{
    o.RoutePrefix = "docs";
    o.SwaggerEndpoint("/docs/v1/docs.json", "Cantina IBJ");
    o.DefaultModelsExpandDepth(-1);
    o.DocumentTitle = "C.IBJ API";
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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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