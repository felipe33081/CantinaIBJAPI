using CantinaIBJ.Model.Core;
using System.ComponentModel.DataAnnotations;

#nullable enable

namespace CantinaIBJ.Model.CustomerPerson;

//Cliente - Comprador
public class CustomerPerson : BaseModel
{
    [Required]
    public string Name { get; set; }

    public string? Email { get; set; }

    [Required]
    public string Phone { get; set; }

    public decimal? DebitBalance { get; set; }

    public decimal? CreditBalance { get; set; }
}