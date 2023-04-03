using CantinaIBJ.Data.Context;
using CantinaIBJ.WebApi.Configurations;
using CantinaIBJ.WebApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
IServiceCollection services = builder.Services;

// Add services to the container.
services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"])),
        ClockSkew = TimeSpan.Zero
    };
});

// Configuração das políticas de autorização
services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireClaim("admin"));
    options.AddPolicy("UserOnly", policy => policy.RequireClaim("user"));
});

services.AddControllers();

services.AddEntityFrameworkNpgsql()
    .AddDbContext<PostgreSqlContext>(options => options.UseNpgsql(
        builder.Configuration.GetConnectionString("POSTGRESQLCONNSTR_PostgreSQL")
        ));

services.AddEndpointsApiExplorer();

//Configura Swagger com autenticação Jwt
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

// Configure o middleware de pontos de extremidade
//app.UseIdentityServer();

app.UseCors(policy =>
{
    policy.AllowAnyOrigin();
    policy.AllowAnyHeader();
    policy.AllowAnyMethod();
});

// Adição do middleware de autenticação
app.UseAuthentication();

// Adição do middleware de roteamento do MVC
app.UseRouting();

// Adição do middleware de autorização
app.UseAuthorization();

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