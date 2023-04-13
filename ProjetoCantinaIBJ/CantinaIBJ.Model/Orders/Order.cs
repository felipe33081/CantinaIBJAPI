using CantinaIBJ.Model.Core;
using CantinaIBJ.Model.Customer;
using CantinaIBJ.Model.Enumerations;

namespace CantinaIBJ.Model.Orders;

public class Order : BaseModel
{
    public int? CustomerPersonId { get; set; }

    public CustomerPerson? CustomerPerson { get; set; }

    public string? CustomerName { get; set; }

    public List<OrderProduct> Products { get; set; }
    
    public decimal TotalValue { get; set; }
    
    public decimal? PaymentValue { get; set; }
    
    public decimal? ChangeValue { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Created;
    
    public PaymentOfType? PaymentOfType { get; set; }
}