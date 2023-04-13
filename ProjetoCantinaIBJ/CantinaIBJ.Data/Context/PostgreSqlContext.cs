using CantinaIBJ.Model.Customer;
using CantinaIBJ.Model;
using CantinaIBJ.Model.User;
using Microsoft.EntityFrameworkCore;
using CantinaIBJ.Model.Orders;
using CantinaIBJ.Data.Context.Configuration;

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
    public DbSet<User> User { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OrderProductConfiguration());
        modelBuilder.ApplyConfiguration(new OrderConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}