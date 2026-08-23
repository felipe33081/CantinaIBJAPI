using CantinaIBJ.Data.Context.Configuration;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Auth;
using CantinaIBJ.Model.Customer;
using CantinaIBJ.Model.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CantinaIBJ.Data.Context;

public class PostgreSqlContext : DbContext
{
    public PostgreSqlContext(DbContextOptions<PostgreSqlContext> options) : base(options)
    {
    }

    public DbSet<CustomerPerson> CustomerPerson { get; set; }
    public DbSet<Product> Product { get; set; }
    public DbSet<ProductHistoric> ProductHistoric { get; set; }
    public DbSet<Order> Order { get; set; }
    public DbSet<AppSetting> AppSetting { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OrderProductConfiguration());
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerPersonConfiguration());

        // SQLite nao ordena por DateTimeOffset (usado em CreatedAt/UpdatedAt via BaseModel).
        // Convertendo para binario (long ordenavel) o ORDER BY passa a funcionar.
        var dtoConverter = new DateTimeOffsetToBinaryConverter();
        var nullableDtoConverter = new ValueConverter<DateTimeOffset?, long?>(
            v => v.HasValue ? v.Value.ToUnixTimeMilliseconds() : (long?)null,
            v => v.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(v.Value) : (DateTimeOffset?)null);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTimeOffset))
                    property.SetValueConverter(dtoConverter);
                else if (property.ClrType == typeof(DateTimeOffset?))
                    property.SetValueConverter(nullableDtoConverter);
            }
        }

        base.OnModelCreating(modelBuilder);
    }
}