using CantinaIBJ.WebApi.Models.Create.Core;
using System.ComponentModel.DataAnnotations;

namespace CantinaIBJ.WebApi.Models.Create.Product;

public class ProductCreateModel : BaseCreateModel
{
    /// <summary>
    /// Nome do produto
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// Descrição do produto
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Preço do produto
    /// </summary>
    [Required]
    public decimal Price { get; set; }

    /// <summary>
    /// Quantidade do produto em estoque
    /// </summary>
    [Required]
    public int Quantity { get; set; }

    /// <summary>
    /// Disponiblidade do produto em estoque
    /// </summary>
    [Required]
    public bool Diponibility { get; set; }
}