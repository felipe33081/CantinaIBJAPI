using CantinaIBJ.Model;
using CantinaIBJ.Model.Enumerations;
using CantinaIBJ.WebApi.Models.Update.Core;
using System.ComponentModel.DataAnnotations;

namespace CantinaIBJ.WebApi.Models.Update.Order;

public class OrderUpdateModel : BaseUpdateModel
{
    /// <summary>
    /// Cliente pré-cadastrado relacionado ao pedido
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "O Campo {0} deve ser maior que zero")]
    public int? CustomerPersonId { get; set; }

    /// <summary>
    /// Nome do Cliente relacionado ao pedido (Caso faça um pedido sem ter um cadastro)
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// Lista de produtos relacionados ao pedido
    /// </summary>
    public List<OrderProductUpdateModel> Products { get; set; }

    /// <summary>
    /// Valor de pagamento do cliente
    /// </summary>
    public decimal? PaymentValue { get; set; }

    /// <summary>
    /// Valor de troco a ser repassado ao cliente
    /// </summary>
    public decimal? ChangeValue { get; set; }

    /// <summary>
    /// Status do pedido
    /// </summary>
    public OrderStatus Status { get; set; } = OrderStatus.InProgress;

    /// <summary>
    /// Tipo de pagamento
    /// </summary>
    public PaymentOfType? PaymentOfType { get; set; }
}