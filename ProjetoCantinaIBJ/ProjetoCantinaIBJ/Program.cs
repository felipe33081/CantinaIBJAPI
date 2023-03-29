using CantinaIBJ.Data.Context;
using CantinaIBJ.WebApi.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
IServiceCollection services = builder.Services;

// Add services to the container.
services.AddControllers();

services.AddEntityFrameworkNpgsql()
    .AddDbContext<PostgreSqlContext>(options => options.UseNpgsql(
        builder.Configuration.GetConnectionString("POSTGRESQLCONNSTR_PostgreSQL")
        ));

services.AddEndpointsApiExplorer();
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Cantina IBJ1", Version = "v1" });
});

services.AddMvcCore().AddAuthorization().AddDataAnnotations();
services.AddMemoryCache();
services.AddCors(c =>
{
    c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});
services.AddDistributedMemoryCache();

services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//Configura os Repositórios
ServicesConfiguration.ConfigureRepositories(builder.Services);

var app = builder.Build();

app.UseSwaggerUI();
app.UseSwagger(x => x.SerializeAsV2 = true);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

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