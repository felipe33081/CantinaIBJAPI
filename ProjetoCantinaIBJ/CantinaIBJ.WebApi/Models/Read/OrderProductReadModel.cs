namespace CantinaIBJ.WebApi.Models.Read;

public class OrderProductReadModel
{
    public int ProductId { get; set; }

    public string ProductDisplay { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }
}