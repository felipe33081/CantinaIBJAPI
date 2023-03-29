using CantinaIBJ.WebApi.Models.Read.Core;

namespace CantinaIBJ.WebApi.Models.Read.Customer;

public class CustomerPersonReadModel : BaseReadModel
{
    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public decimal? DebitBalance { get; set; }

    public decimal? CreditBalance { get; set; }
}