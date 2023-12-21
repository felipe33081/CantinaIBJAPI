using CantinaIBJ.Model.Enumerations;
using System.ComponentModel.DataAnnotations;

namespace CantinaIBJ.WebApi.Models
{
    public class FinalizeOrderRequestModel
    {
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public PaymentOfType PaymentOfType { get; set; }

        public decimal? PaymentValue { get; set; }
    }
}
