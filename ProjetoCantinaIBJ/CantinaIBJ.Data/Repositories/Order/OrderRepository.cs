using CantinaIBJ.Data.Context;
using CantinaIBJ.Data.Contracts;
using CantinaIBJ.Data.Repositories.Core;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Customer;
using CantinaIBJ.Model.Enumerations;
using CantinaIBJ.Model.Orders;
using Microsoft.EntityFrameworkCore;

namespace CantinaIBJ.Data.Repositories;

public class OrderRepository : RepositoryBase<Order>, IOrderRepository
{
    public OrderRepository(PostgreSqlContext context) : base(context)
    {

    }

    public async Task<int> GetCountList()
    {
        int totalCount;
        return totalCount = Context.Order
            .Where(x => x.IsDeleted == false)
            .Count();
    }

    public List<Order> GetAll()
    {
        return Context.Order.ToList();
    }

    public List<Order> GetAllByCustomerId(int id)
    {
        return Context.Order
            .Include(x => x.CustomerPerson)
            .Where(x => x.CustomerPerson.Id == id)
            .ToList();
    }

    public async Task<ListDataPagination<Order>> GetListOrders(UserContext contextUser, int page, int size, string? searchString, bool isDeleted, string? orderBy, OrderStatus? status)
    {
        var query = Context.Order
            .Include(x => x.CustomerPerson)
            .Include(x => x.Products).ThenInclude(o => o.Product)
            .Where(x => x.IsDeleted == false);

        if (!string.IsNullOrEmpty(searchString))
        {
            searchString = searchString.ToLower().Trim();
            query = query.Where(q => q.CustomerPerson.Name.ToLower().Contains(searchString) ||
            q.CustomerName.ToLower().Contains(searchString));
        }

        if (status != null)
        {
            query = query.Where(x => x.Status == status);
        }

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
                case "createdAt_DESC":
                    query = query.OrderByDescending(t => t.CreatedAt);
                    break;
                case "createdAt_ASC":
                    query = query.OrderBy(t => t.CreatedAt);
                    break;
            }
        }

        var data = new ListDataPagination<Order>
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

    public async Task<Order> GetOrderByIdAsync(UserContext contextUser, int id)
    {
        var query = await Context.Order
            .Include(x => x.CustomerPerson)
            .Include(x => x.Products).ThenInclude(c => c.Product)
            .Where(x => x.IsDeleted == false)
            .SingleOrDefaultAsync(x => x.Id == id);

        return query;
    }

    public async Task AddOrderAsync(UserContext contextUser, Order order)
    {
        order.CreatedBy = contextUser.GetCurrentUser();

        await Context.AddAsync(order);
        await Context.SaveChangesAsync();
    }
}