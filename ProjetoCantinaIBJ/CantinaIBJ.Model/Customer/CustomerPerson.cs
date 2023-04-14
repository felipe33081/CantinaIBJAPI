using CantinaIBJ.Model.Core;
using System.ComponentModel.DataAnnotations;

#nullable enable

namespace CantinaIBJ.Model.Customer;

//Cliente - Comprador
public class CustomerPerson : BaseModel
{
    [Required]
    public string Name { get; set; }

    public string? Email { get; set; }

    [Required]
    public string Phone { get; set; }

    public decimal? Balance { get; set; }
}