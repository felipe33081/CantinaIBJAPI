using CantinaIBJ.Data.Context;
using CantinaIBJ.Data.Contracts;
using CantinaIBJ.Data.Repositories.Core;
using CantinaIBJ.Model;
using Microsoft.EntityFrameworkCore;

namespace CantinaIBJ.Data.Repositories;

public class ProductHistoricRepository : RepositoryBase<ProductHistoric>, IProductHistoricRepository
{
    public ProductHistoricRepository(PostgreSqlContext context) : base(context)
    {

    }

    public async Task<List<ProductHistoric>> GetProductHistorics()
    {
        return await Context.ProductHistoric.ToListAsync();
    }

    public async Task<ProductHistoric> GetProductHistoricByIdAsync(int id)
    {
        return await Context.ProductHistoric
            .SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddProductHistoricAsync(ProductHistoric productHistoric)
    {
        await Context.AddAsync(productHistoric);
        await Context.SaveChangesAsync();
    }
}