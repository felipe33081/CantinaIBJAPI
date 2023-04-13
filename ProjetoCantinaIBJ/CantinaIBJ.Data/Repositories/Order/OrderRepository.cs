using CantinaIBJ.Data.Context;
using CantinaIBJ.Data.Contracts;
using CantinaIBJ.Data.Repositories.Core;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Orders;
using Microsoft.EntityFrameworkCore;

namespace CantinaIBJ.Data.Repositories;

public class OrderRepository : RepositoryBase<Order>, IOrderRepository
{
    public OrderRepository(PostgreSqlContext context) : base(context)
    {

    }

    public async Task<ListDataPagination<Order>> GetListOrders(UserContext contextUser,
        string customerName,
        int page,
        int size)
    {
        var query = Context.Order
            .Include(x => x.CustomerPerson)
            .Include(x => x.Products).ThenInclude(c => c.Product)
            .Where(x => x.IsDeleted == false);
        
        if (!string.IsNullOrEmpty(customerName))
        {
            customerName = customerName.ToLower().Trim();
            query = query.Where(q => q.CustomerName.ToLower().Contains(customerName) ||
            q.CustomerPerson.Name.ToLower().Contains(customerName));
        }

        var data = new ListDataPagination<Order>
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