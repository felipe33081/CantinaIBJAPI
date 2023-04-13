namespace CantinaIBJ.WebApi.Models.Create;

public class OrderProductCreateModel
{
    /// <summary>
    /// Id do produto
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Quantidade do produto
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Preço unitário do produto
    /// </summary>
    public decimal Price { get; set; }
}