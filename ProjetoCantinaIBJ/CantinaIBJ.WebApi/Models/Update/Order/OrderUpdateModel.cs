using CantinaIBJ.Model;
using CantinaIBJ.Model.Enumerations;
using CantinaIBJ.WebApi.Models.Update.Core;

namespace CantinaIBJ.WebApi.Models.Update.Order;

public class OrderUpdateModel : BaseUpdateModel
{
    /// <summary>
    /// Nome do Cliente relacionado ao pedido (Caso faça um pedido sem ter um cadastro)
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// Lista de produtos relacionados ao pedido
    /// </summary>
    public List<OrderProductUpdateModel> Products { get; set; }

    /// <summary>
    /// Status do pedido
    /// </summary>
    public OrderStatus Status { get; set; }

    /// <summary>
    /// Tipo de pagamento
    /// </summary>
    public PaymentOfType? PaymentOfType { get; set; }
}