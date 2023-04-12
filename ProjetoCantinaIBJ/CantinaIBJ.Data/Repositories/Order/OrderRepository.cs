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

    public async Task<List<Order>> GetOrders(UserContext contextUser)
    {
        return await Context.Order
            .Include(x => x.CustomerPerson)
            .Include(x => x.Products).ThenInclude(c => c.Product)
            .ToListAsync();
    }

    public async Task<Order> GetOrderByIdAsync(UserContext contextUser, int id)
    {
        return await Context.Order
            .Include(x => x.CustomerPerson)
            .Include(x => x.Products).ThenInclude(c => c.Product)
            .SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddOrderAsync(UserContext contextUser, Order order)
    {
        order.CreatedBy = contextUser.GetCurrentUser();

        await Context.AddAsync(order);
        await Context.SaveChangesAsync();
    }
}