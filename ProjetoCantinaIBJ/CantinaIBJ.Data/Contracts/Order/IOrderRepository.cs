using CantinaIBJ.Data.Contracts.Core;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Enumerations;
using CantinaIBJ.Model.Orders;

namespace CantinaIBJ.Data.Contracts;

public interface IOrderRepository : IRepositoryBase<Order>
{
    Task<int> GetCountList();
    List<Order> GetAll();
    Task<List<Order>> GetAllByCustomerId(int id);
    Task<ListDataPagination<Order>> GetListOrders(UserContext contextUser, int page, int size, string? searchString, int? id, bool isDeleted, string? orderBy, OrderStatus? status);
    Task<Order?> GetOrderByIdEndpointAsync(int id);
    Task<Order?> GetOrderByIdAsync(UserContext contextUser, int id);
    Task AddOrderAsync(UserContext contextUser, Order order);
}