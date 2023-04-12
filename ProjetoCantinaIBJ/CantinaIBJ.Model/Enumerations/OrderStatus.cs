using ServiceStack.DataAnnotations;

namespace CantinaIBJ.Model.Enumerations;

public enum OrderStatus
{
    [Description("Criado")]
    Created = 0,

    [Description("Em andamento")]
    InProgress = 1,

    [Description("Encerrado")]
    Closed = 2,

    [Description("Cancelado")]
    Canceled = 3,

    [Description("Excluído")]
    Excluded = 4
}