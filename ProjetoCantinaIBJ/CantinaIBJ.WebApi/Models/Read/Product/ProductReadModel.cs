using CantinaIBJ.WebApi.Models.Read.Core;

namespace CantinaIBJ.WebApi.Models.Read.Product;

public class ProductReadModel : BaseReadModel
{
    /// <summary>
    /// Nome do produto
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Descrição do produto
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Preço do produto
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Quantidade do produto em estoque
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Disponiblidade do produto em estoque
    /// </summary>
    public bool Diponibility { get; set; }
}