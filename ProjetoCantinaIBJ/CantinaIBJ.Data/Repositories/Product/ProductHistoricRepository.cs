using CantinaIBJ.Data.Context;
using CantinaIBJ.Data.Contracts;
using CantinaIBJ.Data.Repositories.Core;
using CantinaIBJ.Model.Product;
using CantinaIBJ.Model.User;
using Microsoft.EntityFrameworkCore;

namespace CantinaIBJ.Data.Repositories;

public class ProductHistoricRepository : RepositoryBase<ProductHistoric>, IProductHistoricRepository
{
    public ProductHistoricRepository(PostgreSqlContext context) : base(context)
    {

    }

    public async Task<IEnumerable<ProductHistoric>> GetProductHistorics(User user)
    {
        return await Context.ProductHistoric.ToListAsync();
    }

    public async Task<ProductHistoric> GetProductHistoricByIdAsync(User user, int id)
    {
        return await Context.ProductHistoric
            .SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddProductHistoricAsync(User user, ProductHistoric productHistoric)
    {
        productHistoric.CreatedBy = user.GetCurrentUser();
        productHistoric.UpdatedBy = user.GetCurrentUser();

        await Context.AddAsync(productHistoric);
        await Context.SaveChangesAsync();
    }
}