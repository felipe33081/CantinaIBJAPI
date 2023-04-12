using CantinaIBJ.Data.Contracts.Core;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Orders;

namespace CantinaIBJ.Data.Contracts;

public interface IOrderRepository : IRepositoryBase<Order>
{
    Task<List<Order>> GetOrders(UserContext contextUser);
    Task<Order> GetOrderByIdAsync(UserContext contextUser, int id);
    Task AddOrderAsync(UserContext contextUser, Order order);
}