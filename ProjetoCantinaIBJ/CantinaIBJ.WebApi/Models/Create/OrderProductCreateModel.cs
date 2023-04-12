namespace CantinaIBJ.WebApi.Models.Create;

public class OrderProductCreateModel
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }
}