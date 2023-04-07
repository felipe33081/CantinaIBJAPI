using CantinaIBJ.Data.Context;
using CantinaIBJ.Data.Contracts;
using CantinaIBJ.Data.Repositories.Core;
using CantinaIBJ.Model;
using Microsoft.EntityFrameworkCore;

namespace CantinaIBJ.Data.Repositories;

public class ProductRepository : RepositoryBase<Product>, IProductRepository
{
    public ProductRepository(PostgreSqlContext context) : base(context)
    {

    }

    public async Task<List<Product>> GetProducts(UserContext contextUser)
    {
        return await Context.Product.ToListAsync();
    }

    public async Task<Product> GetProductByIdAsync(UserContext contextUser, int id)
    {
        return await Context.Product
            .SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddProductAsync(UserContext contextUser, Product product)
    {
        product.CreatedBy = contextUser.GetCurrentUser();
        product.UpdatedBy = contextUser.GetCurrentUser();

        await Context.AddAsync(product);
        await Context.SaveChangesAsync();
    }
}