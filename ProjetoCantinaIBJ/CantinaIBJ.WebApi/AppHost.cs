using CantinaIBJ.Data.Context;
using CantinaIBJ.WebApi.Configurations;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CantinaIBJ.WebApi;

/// <summary>
/// Construcao centralizada do host ASP.NET Core. Usado tanto pelo Program.cs
/// (execucao normal da API) quanto pelo host desktop (CantinaIBJ.Desktop),
/// que sobe o mesmo servidor in-process e o exibe numa janela WebView2.
/// Banco: SQLite embutido (arquivo local), sem instancia externa, 100% offline.
/// </summary>
public static class AppHost
{
    /// <summary>Porta fixa em localhost para o modo desktop.</summary>
    public const string DefaultUrl = "http://localhost:5228";

    /// <summary>Caminho do arquivo SQLite em %LOCALAPPDATA%\CantinaIBJ\cantinaibj.db.</summary>
    public static string DatabasePath()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CantinaIBJ");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "cantinaibj.db");
    }

    /// <summary>
    /// Adiciona uma coluna a uma tabela existente, ignorando o erro caso ela ja exista.
    /// Serve como "mini-migration" para bancos criados por versoes anteriores.
    /// </summary>
    private static void TryAddColumn(PostgreSqlContext context, string table, string column, string type)
    {
        try
        {
            context.Database.ExecuteSqlRaw($"ALTER TABLE {table} ADD COLUMN {column} {type}");
        }
        catch
        {
            // Coluna ja existe (ou tabela ainda nao criada): melhor-esforco, nao impede o app.
        }
    }

    public static WebApplication Build(string[] args)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            // Garante que wwwroot e appsettings sejam resolvidos ao lado do .exe,
            // mesmo quando iniciado pelo host desktop.
            ContentRootPath = AppContext.BaseDirectory
        });

        // Localhost apenas; sem HTTPS/cert (app offline de kiosk unico).
        builder.WebHost.UseUrls(DefaultUrl);

        IServiceCollection services = builder.Services;

        var connectionString = $"Data Source={DatabasePath()}";
        services.AddDbContext<PostgreSqlContext>(options => options.UseSqlite(connectionString));

        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        services.AddHealthChecks();
        services.AddMvcCore().AddAuthorization().AddDataAnnotations();
        services.ConfigureRepositories();
        services.AddMemoryCache();
        services.AddDistributedMemoryCache();
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAnyOrigin",
                policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        });
        // AddApplicationPart garante a descoberta dos controllers mesmo quando o
        // assembly de entrada nao e o da WebApi (caso do host desktop CantinaIBJ.Desktop).
        services.AddControllers().AddApplicationPart(typeof(AppHost).Assembly);
        services.AddEndpointsApiExplorer();
        services.ConfigureServices(config);
        services.ConfigureSwaggerGen();
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        var app = builder.Build();

        // Cria o banco SQLite e o schema a partir do modelo na primeira execucao,
        // e garante o PIN/seed inicial. Nao usa migrations (sao especificas do Postgres).
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<PostgreSqlContext>();

            // Tolera "ja existe" (ex.: estado parcial de execucao anterior) desde
            // que a conexao com o banco funcione.
            try
            {
                context.Database.EnsureCreated();
            }
            catch when (context.Database.CanConnect())
            {
                // Banco ja existe; segue normalmente.
            }

            // Evolucao de esquema leve para bancos criados por versoes anteriores:
            // garante colunas novas sem depender de migrations.
            TryAddColumn(context, "AppSetting", "PrinterName", "TEXT");

            DbSeeder.Seed(context);
        }

        // Serve o SPA (build do React em wwwroot) na mesma origem da API.
        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseHealthChecks("/health");

        app.UseSwagger(x => x.RouteTemplate = "docs/{documentName}/docs.json");
        app.UseSwaggerUI(o =>
        {
            o.RoutePrefix = "docs";
            o.SwaggerEndpoint("/docs/v1/docs.json", "Sistema de gestao");
            o.DefaultModelsExpandDepth(-1);
            o.DocumentTitle = "Sistema de Gestao API";
            o.DisplayRequestDuration();
            o.EnableValidator(null);
        });

        if (app.Environment.IsDevelopment())
            app.UseDeveloperExceptionPage();

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        var cultureInfo = new CultureInfo("pt-BR");
        cultureInfo.NumberFormat.CurrencySymbol = "R$";
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

        app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

        app.Use(async (context, next) =>
        {
            context.Response.Headers.Append("Access-Control-Expose-Headers", "X-Total-Count");
            await next();
        });

        app.MapControllers();

        // Fallback do SPA: qualquer rota nao-API cai no index.html (React Router).
        app.MapFallbackToFile("index.html");

        return app;
    }
}
