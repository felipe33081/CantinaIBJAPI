using ServiceStack.DataAnnotations;

namespace CantinaIBJ.Model.Enumerations;

public enum PaymentOfType
{
    [Description("Dinheiro")]
    Money = 0,
    [Description("PIX")]
    PIX = 1,

    [Description("Cartão de Débito")]
    DebitCard = 2,
    [Description("Cartão de Crédito")]
    CreditCard = 3,

    [Description("Fiado")]
    Debitor = 4,
    [Description("Crédito na conta")]
    ExtraMoney =5
}