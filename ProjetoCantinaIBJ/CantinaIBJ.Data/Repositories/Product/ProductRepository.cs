using CantinaIBJ.Data.Context;
using CantinaIBJ.Data.Contracts;
using CantinaIBJ.Data.Repositories.Core;
using CantinaIBJ.Model.Customer;
using CantinaIBJ.Model.Product;
using CantinaIBJ.Model.User;
using Microsoft.EntityFrameworkCore;

namespace CantinaIBJ.Data.Repositories;

public class ProductRepository : RepositoryBase<Product>, IProductRepository
{
    public ProductRepository(PostgreSqlContext context) : base(context)
    {

    }

    public async Task<IEnumerable<Product>> GetProducts(User user)
    {
        return await Context.Product.ToListAsync();
    }

    public async Task<Product> GetProductByIdAsync(User user, int id)
    {
        return await Context.Product
            .SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddProductAsync(User user, Product product)
    {
        product.CreatedBy = user.GetCurrentUser();
        product.UpdatedBy = user.GetCurrentUser();

        await Context.AddAsync(product);
        await Context.SaveChangesAsync();
    }
}