using CantinaIBJ.Model.CustomerPerson;
using Microsoft.EntityFrameworkCore;

namespace CantinaIBJ.Data.Context;

public class PostgreSqlContext : DbContext
{
    public PostgreSqlContext(DbContextOptions<PostgreSqlContext> options) : base(options)
    {

    }

    public DbSet<CustomerPerson> CustomerPerson { get; set; }
}