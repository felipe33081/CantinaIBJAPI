using CantinaIBJ.Model;
using CantinaIBJ.Model.Enumerations;
using CantinaIBJ.WebApi.Models.Create.Core;
using System.ComponentModel.DataAnnotations;

namespace CantinaIBJ.WebApi.Models.Create.Order;

public class OrderCreateModel : BaseCreateModel
{
    /// <summary>
    /// Cliente pré-cadastrado relacionado ao pedido
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "O Campo {0} deve ser maior que zero")]
    public int? CustomerPersonId { get; set; }

    /// <summary>
    /// Nome do Cliente relacionado ao pedido (Caso faça um pedido sem ter um cadastro)
    /// </summary>
    [StringLength(40, ErrorMessage = "O Campo {0} deve conter o tamanho máximo de 40 caracteres")]
    public string? CustomerName { get; set; }

    /// <summary>
    /// Lista de produtos relacionados ao pedido
    /// </summary>
    [Required(ErrorMessage = "Produto é obrigatório")]
    public List<OrderProductCreateModel> Products { get; set; }

    /// <summary>
    /// Tipo de pagamento
    /// </summary>
    public PaymentOfType? PaymentOfType { get; set; }
}