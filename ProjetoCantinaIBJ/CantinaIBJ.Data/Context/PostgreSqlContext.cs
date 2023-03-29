using Microsoft.EntityFrameworkCore;

namespace CantinaIBJ.Data.Context;

public class PostgreSqlContext : DbContext
{
    public PostgreSqlContext(DbContextOptions<PostgreSqlContext> options) : base(options)
    {

    }

}