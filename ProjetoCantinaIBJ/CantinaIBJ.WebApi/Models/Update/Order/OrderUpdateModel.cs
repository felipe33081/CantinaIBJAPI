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
    public int? CustomerPersonId { get; set; }

    /// <summary>
    /// Nome do Cliente relacionado ao pedido (Caso faça um pedido sem ter um cadastro)
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// Lista de produtos relacionados ao pedido
    /// </summary>
    [Required(ErrorMessage = "o Campo {0} é obrigatório")]
    public List<OrderProductUpdateModel>? Products { get; set; }
}