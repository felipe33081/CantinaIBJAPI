using CantinaIBJ.Data.Context;
using CantinaIBJ.Data.Contracts;
using CantinaIBJ.Data.Repositories.Core;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Orders;
using Microsoft.EntityFrameworkCore;

namespace CantinaIBJ.Data.Repositories;

public class ProductRepository : RepositoryBase<Product>, IProductRepository
{
    public ProductRepository(PostgreSqlContext context) : base(context)
    {

    }

    public async Task<ListDataPagination<Product>> GetListProducts(UserContext contextUser,
        string searchString,
        int page,
        int size)
    {
        var query = Context.Product
            .Where(x => x.IsDeleted == false);

        if (!string.IsNullOrEmpty(searchString))
        {
            searchString = searchString.ToLower().Trim();
            query = query.Where(q => q.Name.ToLower().Contains(searchString) ||
            q.Description.ToLower().Contains(searchString));
        }

        var data = new ListDataPagination<Product>
        {
            Page = page,
            TotalItems = await query.CountAsync()
        };
        data.TotalPages = (int)Math.Ceiling((double)data.TotalItems / size);

        data.Data = await query.Skip(size * page)
            .Take(size)
            .AsNoTracking()
            .ToListAsync();

        return data;
    }

    public async Task<Product> GetProductByIdAsync(UserContext contextUser, int id)
    {
        var query = await Context.Product
            .Where(x => x.IsDeleted == false)
            .SingleOrDefaultAsync(x => x.Id == id);

        return query;
    }

    public async Task AddProductAsync(UserContext contextUser, Product product)
    {
        product.CreatedBy = contextUser.GetCurrentUser();

        await Context.AddAsync(product);
        await Context.SaveChangesAsync();
    }
}