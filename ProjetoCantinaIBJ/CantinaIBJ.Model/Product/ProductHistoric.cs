using CantinaIBJ.Model.Core;

namespace CantinaIBJ.Model;

public class ProductHistoric : BaseModel
{
    public int ProductId { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public bool Diponibility { get; set; }
}