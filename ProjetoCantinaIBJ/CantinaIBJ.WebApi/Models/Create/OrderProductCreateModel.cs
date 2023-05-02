using System.ComponentModel.DataAnnotations;

namespace CantinaIBJ.WebApi.Models.Create;

public class OrderProductCreateModel
{
    /// <summary>
    /// Id do produto
    /// </summary>
    [Required(ErrorMessage = "Id do produto é obrigatório")]
    public int ProductId { get; set; }

    /// <summary>
    /// Quantidade do produto
    /// </summary>
    [Required(ErrorMessage = "Quantidade do produto é obrigatório")]
    public int Quantity { get; set; }
}