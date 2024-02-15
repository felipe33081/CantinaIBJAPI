using CantinaIBJ.Data.Context;
using CantinaIBJ.Data.Contracts.Dashboard;
using CantinaIBJ.Data.Repositories.Core;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Enumerations;
using CantinaIBJ.Model.Orders;
using Microsoft.EntityFrameworkCore;

namespace CantinaIBJ.Data.Repositories.Dashboard;

public class DashboardRepository : RepositoryBase<Order>, IDashboardRepository
{
    private PostgreSqlContext _context;
    public DashboardRepository(PostgreSqlContext context) : base(context)
    {
        _context = context;
    }

    public DashboardData GetDashboardDataAsync(DateTime? from = null, DateTime? to = null)
    {
        var orders = _context.Order
            .Include(o => o.CustomerPerson)
            .Include(o => o.Products)
            .AsQueryable();

        if (from.HasValue)
        {
            orders = orders.Where(c =>
                c.UpdatedAt.HasValue ?
                c.UpdatedAt.Value.DateTime >= from.Value :
                c.CreatedAt.DateTime >= from.Value);
        }

        if (to.HasValue)
        {
            var original = to.Value;
            var findDate = new DateTime(original.Year, original.Month, original.Day, 23, 59, 59);
            orders = orders.Where(c =>
                c.UpdatedAt.HasValue ?
                c.UpdatedAt.Value.DateTime <= findDate :
                c.CreatedAt.DateTime <= findDate);
        }

        int orderTotalQuantity = orders.Where(c => c.IsDeleted == false).Count();

        decimal totalValueAmount = orders
            .Where(c => c.Status == OrderStatus.InProgress || c.Status == OrderStatus.Finished)
            .Sum(c => c.TotalValue);

        decimal totalValueFinishedAmount = orders
            .Where(c => c.Status == OrderStatus.Finished)
            .Sum(c => c.TotalValue);

        decimal totalValueInProgressAmount = orders
            .Where(c => c.Status == OrderStatus.InProgress)
            .Sum(c => c.TotalValue);

        decimal averageOrders = 0;
        
        int ordersFinished = orders.Where(c => c.Status == OrderStatus.Finished).Count();

        int ordersInProgress = orders.Where(c => c.Status == OrderStatus.InProgress).Count();

        int ordersExcluded = orders.Where(c => c.Status == OrderStatus.Excluded).Count();

        return new DashboardData
        {
            OrderQuantity = orderTotalQuantity,
            TotalValueAmount = totalValueAmount,
            TotalValueFinishedAmount = totalValueFinishedAmount,
            TotalValueInProgressAmount = totalValueInProgressAmount,
            AverageOrderValueAmount = averageOrders,
            OrdersFinished = ordersFinished,
            OrdersInProgress = ordersInProgress,
            OrdersExcluded = ordersExcluded
        };
    }
}
