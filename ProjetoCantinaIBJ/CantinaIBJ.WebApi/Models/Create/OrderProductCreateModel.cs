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
}