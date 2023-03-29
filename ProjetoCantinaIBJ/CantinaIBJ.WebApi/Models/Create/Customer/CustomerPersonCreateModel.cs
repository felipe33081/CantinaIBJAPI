using CantinaIBJ.WebApi.Models.Create.Core;
using System.ComponentModel.DataAnnotations;

namespace CantinaIBJ.WebApi.Models.Create.Customer;

public class CustomerPersonCreateModel : BaseCreateModel
{
    [Required(ErrorMessage = "Obrigatório informar o nome")]
    public string Name { get; set; }

    public string? Email { get; set; }

    [Required(ErrorMessage = "Obrigatório informar o Celular")]
    public string Phone { get; set; }

    public decimal? DebitBalance { get; set; }

    public decimal? CreditBalance { get; set; }
}