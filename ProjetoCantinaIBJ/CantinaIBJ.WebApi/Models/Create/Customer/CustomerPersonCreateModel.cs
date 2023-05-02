using CantinaIBJ.WebApi.Models.Create.Core;
using System.ComponentModel.DataAnnotations;

namespace CantinaIBJ.WebApi.Models.Create.Customer;

public class CustomerPersonCreateModel : BaseCreateModel
{
    /// <summary>
    /// Nome do cliente
    /// </summary>
    [Required(ErrorMessage = "Obrigatório informar o nome")]
    public string Name { get; set; }

    /// <summary>
    /// E-mail do cliente
    /// </summary>
    [EmailAddress(ErrorMessage = "Digite um e-mail válido")]
    public string? Email { get; set; }

    /// <summary>
    /// Celular do cliente
    /// </summary>
    [Required(ErrorMessage = "Obrigatório informar o Celular")]
    public string Phone { get; set; }
}