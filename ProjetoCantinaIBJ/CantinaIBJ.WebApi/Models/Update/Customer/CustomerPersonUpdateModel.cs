using CantinaIBJ.WebApi.Models.Update.Core;

namespace CantinaIBJ.WebApi.Models.Update.Customer;

public class CustomerPersonUpdateModel : BaseUpdateModel
{
    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public decimal? DebitBalance { get; set; }

    public decimal? CreditBalance { get; set; }
}