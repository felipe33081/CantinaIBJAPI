using CantinaIBJ.Model;
using CantinaIBJ.Model.Enumerations;
using CantinaIBJ.WebApi.Models.Read.Core;

namespace CantinaIBJ.WebApi.Models.Read.Order;

public class OrderReadModel : BaseReadModel
{
    /// <summary>
    /// Id do cliente pré-cadastrado relacionado ao pedido
    /// </summary>
    public int? CustomerPersonId { get; set; }
    
    /// <summary>
    /// Nome do cliente pré-cadastrado relacionado ao pedido
    /// </summary>
    public string CustomerPersonDisplay { get; set; }

    /// <summary>
    /// Nome do Cliente relacionado ao pedido (Caso faça um pedido sem ter um cadastro)
    /// </summary>
    public string CustomerName { get; set; }

    /// <summary>
    /// Lista de produtos relacionados ao pedido
    /// </summary>
    public List<OrderProductReadModel> Products { get; set; }

    /// <summary>
    /// Valor total
    /// </summary>
    public decimal TotalValue { get; set; }

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
    public OrderStatus Status { get; set; }

    /// <summary>
    /// Tipo de pagamento
    /// </summary>
    public PaymentOfType? PaymentOfType { get; set; }
}