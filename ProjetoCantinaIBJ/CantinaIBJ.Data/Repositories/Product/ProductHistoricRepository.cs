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

    public async Task<IEnumerable<ProductHistoric>> GetProductHistorics(UserContext contextUser)
    {
        return await Context.ProductHistoric.ToListAsync();
    }

    public async Task<ProductHistoric> GetProductHistoricByIdAsync(UserContext contextUser, int id)
    {
        return await Context.ProductHistoric
            .SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddProductHistoricAsync(UserContext contextUser, ProductHistoric productHistoric)
    {
        productHistoric.CreatedBy = contextUser.GetCurrentUser();
        productHistoric.UpdatedBy = contextUser.GetCurrentUser();

        await Context.AddAsync(productHistoric);
        await Context.SaveChangesAsync();
    }
}