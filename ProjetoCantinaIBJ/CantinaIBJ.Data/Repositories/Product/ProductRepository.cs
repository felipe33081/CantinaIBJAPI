using CantinaIBJ.Data.Context;
using CantinaIBJ.Data.Contracts;
using CantinaIBJ.Data.Repositories.Core;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Customer;
using CantinaIBJ.Model.Orders;
using Microsoft.EntityFrameworkCore;

namespace CantinaIBJ.Data.Repositories;

public class ProductRepository : RepositoryBase<Product>, IProductRepository
{
    public ProductRepository(PostgreSqlContext context) : base(context)
    {

    }

    public async Task<int> GetCountList()
    {
        int totalCount;
        return totalCount = Context.Product
            .Where(x => x.IsDeleted == false)
            .Count();
    }

    public async Task<ListDataPagination<Product>> GetListProducts(UserContext contextUser, int page, int size, string? name, string? description, string? searchString, bool isDeleted, string? orderBy)
    {
        var query = Context.Product
            .Where(x => x.IsDeleted == false);

        if (!string.IsNullOrEmpty(searchString))
        {
            searchString = searchString.ToLower().Trim();
            query = query.Where(q => q.Name.ToLower().Contains(searchString) ||
            q.Description.ToLower().Contains(searchString));
        }

        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(q => q.Name.ToLower().Contains(name));
        }

        if (!string.IsNullOrEmpty(description))
        {
            query = query.Where(q => q.Description.ToLower().Contains(description));
        }

        if (isDeleted)
            query = query.Where(q => q.IsDeleted == true);

        query = query.OrderByDescending(t => t.CreatedAt);

        if (!string.IsNullOrEmpty(orderBy))
        {
            switch (orderBy)
            {
                case "id_DESC":
                    query = query.OrderByDescending(t => t.Id);
                    break;
                case "id_ASC":
                    query = query.OrderBy(t => t.Id);
                    break;
                case "name_DESC":
                    query = query.OrderByDescending(t => t.Name);
                    break;
                case "name_ASC":
                    query = query.OrderBy(t => t.Name);
                    break;
                case "description_DESC":
                    query = query.OrderByDescending(t => t.Description);
                    break;
                case "description_ASC":
                    query = query.OrderBy(t => t.Description);
                    break;
                case "price_DESC":
                    query = query.OrderByDescending(t => t.Price);
                    break;
                case "price_ASC":
                    query = query.OrderBy(t => t.Price);
                    break;
                case "quantity_DESC":
                    query = query.OrderByDescending(t => t.Quantity);
                    break;
                case "quantity_ASC":
                    query = query.OrderBy(t => t.Quantity);
                    break;
                case "disponibility_DESC":
                    query = query.OrderByDescending(t => t.Disponibility);
                    break;
                case "disponibility_ASC":
                    query = query.OrderBy(t => t.Disponibility);
                    break;
                case "createdAt_DESC":
                    query = query.OrderByDescending(t => t.CreatedAt);
                    break;
                case "createdAt_ASC":
                    query = query.OrderBy(t => t.CreatedAt);
                    break;
                case "updatedAt_DESC":
                    query = query.OrderByDescending(t => t.UpdatedAt);
                    break;
                case "updatedAt_ASC":
                    query = query.OrderBy(t => t.UpdatedAt);
                    break;
                case "updatedBy_DESC":
                    query = query.OrderByDescending(t => t.UpdatedBy);
                    break;
                case "updatedBy_ASC":
                    query = query.OrderBy(t => t.UpdatedBy);
                    break;
            }
        }

        var data = new ListDataPagination<Product>
        {
            Page = page,
            TotalItems = await query.CountAsync()
        };

        data.TotalPages = (int)Math.Ceiling((double)data.TotalItems / size);

        data.Data = await query
            .Skip(size * page)
            .Take(size)
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