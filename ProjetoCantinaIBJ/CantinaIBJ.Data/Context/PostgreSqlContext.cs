using CantinaIBJ.Data.Context.Configuration;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Customer;
using CantinaIBJ.Model.Orders;
using Microsoft.EntityFrameworkCore;

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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OrderProductConfiguration());
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerPersonConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}