using CantinaIBJ.Model.Customer;
using CantinaIBJ.Model.Product;
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


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}