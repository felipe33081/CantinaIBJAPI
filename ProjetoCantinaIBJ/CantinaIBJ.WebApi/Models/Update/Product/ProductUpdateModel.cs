using CantinaIBJ.WebApi.Models.Update.Core;
using System.ComponentModel.DataAnnotations;

namespace CantinaIBJ.WebApi.Models.Update.Product;

public class ProductUpdateModel : BaseUpdateModel
{
    /// <summary>
    /// Nome do produto
    /// </summary>
    [Required(ErrorMessage = "Nome é obrigatório")]
    public string Name { get; set; }

    /// <summary>
    /// Descrição do produto
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Preço do produto
    /// </summary>
    [Required(ErrorMessage = "Preço é obrigatório")]
    public decimal Price { get; set; }

    /// <summary>
    /// Quantidade do produto em estoque
    /// </summary>
    [Required(ErrorMessage = "Quantidade é obrigatório")]
    public int Quantity { get; set; }
}