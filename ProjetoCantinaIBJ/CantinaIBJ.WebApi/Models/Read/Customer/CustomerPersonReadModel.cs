using CantinaIBJ.WebApi.Models.Read.Core;

namespace CantinaIBJ.WebApi.Models.Read.Customer;

public class CustomerPersonReadModel : BaseReadModel
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
    /// Valor debitado do cliente
    /// </summary>
    public decimal? DebitBalance { get; set; }

    /// <summary>
    /// Valor creditado no cliente
    /// </summary>
    public decimal? CreditBalance { get; set; }
}