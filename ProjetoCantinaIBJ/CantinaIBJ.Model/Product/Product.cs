using CantinaIBJ.Model.Core;
using Microsoft.Build.Framework;

namespace CantinaIBJ.Model;

public class Product : BaseModel
{
    [Required]
    public string Name { get; set; }

    public string? Description { get; set; }

    [Required]
    public decimal Price { get; set; }
    
    [Required]
    public int Quantity { get; set; }

    [Required]
    public bool Disponibility { get; set; } = false;

    public List<OrderProduct> OrdersProducts { get; set; }
}