using CantinaIBJ.WebApi.Models.Create.Core;
using System.ComponentModel.DataAnnotations;

namespace CantinaIBJ.WebApi.Models.Create.Product;

public class ProductCreateModel : BaseCreateModel
{
    /// <summary>
    /// Nome do produto
    /// </summary>
    [Required(ErrorMessage = "O campo {0} é obrigatório")]
    public string Name { get; set; }

    /// <summary>
    /// Descrição do produto
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Preço do produto
    /// </summary>
    [Required(ErrorMessage = "O campo {0} é obrigatório")]
    public decimal Price { get; set; }

    /// <summary>
    /// Quantidade do produto em estoque
    /// </summary>
    [Required(ErrorMessage = "O campo {0} é obrigatório")]
    public int Quantity { get; set; }
}