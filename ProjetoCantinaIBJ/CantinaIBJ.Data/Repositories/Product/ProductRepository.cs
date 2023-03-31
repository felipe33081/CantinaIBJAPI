using CantinaIBJ.Data.Context;
using CantinaIBJ.Data.Contracts;
using CantinaIBJ.Data.Repositories.Core;
using CantinaIBJ.Model.Product;
using Microsoft.EntityFrameworkCore;

namespace CantinaIBJ.Data.Repositories;

public class ProductRepository : RepositoryBase<Product>, IProductRepository
{
    public ProductRepository(PostgreSqlContext context) : base(context)
    {

    }

    public async Task<IEnumerable<Product>> GetProducts()
    {
        return await Context.Product.ToListAsync();
    }

    public async Task<Product> GetProductByIdAsync(int id)
    {
        return await Context.Product
            .SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddProductAsync(Product product)
    {
        await Context.AddAsync(product);
        await Context.SaveChangesAsync();
    }
}