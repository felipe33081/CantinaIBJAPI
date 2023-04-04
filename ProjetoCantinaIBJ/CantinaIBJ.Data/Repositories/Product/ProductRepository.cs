using CantinaIBJ.Data.Context;
using CantinaIBJ.Data.Contracts;
using CantinaIBJ.Data.Repositories.Core;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Customer;
using CantinaIBJ.Model.Product;
using Microsoft.EntityFrameworkCore;

namespace CantinaIBJ.Data.Repositories;

public class ProductRepository : RepositoryBase<Product>, IProductRepository
{
    public ProductRepository(PostgreSqlContext context) : base(context)
    {

    }

    public async Task<IEnumerable<Product>> GetProducts(UserContext user)
    {
        return await Context.Product.ToListAsync();
    }

    public async Task<Product> GetProductByIdAsync(UserContext user, int id)
    {
        return await Context.Product
            .SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddProductAsync(UserContext user, Product product)
    {
        product.CreatedBy = user.GetCurrentUser();
        product.UpdatedBy = user.GetCurrentUser();

        await Context.AddAsync(product);
        await Context.SaveChangesAsync();
    }
}