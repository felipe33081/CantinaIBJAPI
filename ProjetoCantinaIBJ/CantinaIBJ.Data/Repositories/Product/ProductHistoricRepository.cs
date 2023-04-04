using CantinaIBJ.Data.Context;
using CantinaIBJ.Data.Contracts;
using CantinaIBJ.Data.Repositories.Core;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Product;
using Microsoft.EntityFrameworkCore;

namespace CantinaIBJ.Data.Repositories;

public class ProductHistoricRepository : RepositoryBase<ProductHistoric>, IProductHistoricRepository
{
    public ProductHistoricRepository(PostgreSqlContext context) : base(context)
    {

    }

    public async Task<IEnumerable<ProductHistoric>> GetProductHistorics(UserContext user)
    {
        return await Context.ProductHistoric.ToListAsync();
    }

    public async Task<ProductHistoric> GetProductHistoricByIdAsync(UserContext user, int id)
    {
        return await Context.ProductHistoric
            .SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddProductHistoricAsync(UserContext user, ProductHistoric productHistoric)
    {
        productHistoric.CreatedBy = user.GetCurrentUser();
        productHistoric.UpdatedBy = user.GetCurrentUser();

        await Context.AddAsync(productHistoric);
        await Context.SaveChangesAsync();
    }
}