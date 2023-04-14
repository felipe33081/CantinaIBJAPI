using CantinaIBJ.WebApi.Models.Update.Core;

namespace CantinaIBJ.WebApi.Models.Update.Customer;

public class CustomerPersonUpdateModel : BaseUpdateModel
{
    /// <summary>
    /// Nome do cliente
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// E-mail do cliente
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Celular do cliente
    /// </summary>
    public string Phone { get; set; }

    /// <summary>
    /// Valor do saldo existente na conta do cliente
    /// </summary>
    public decimal? Balance { get; set; }
}