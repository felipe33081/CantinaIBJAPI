namespace CantinaIBJ.WebApi.Models.Read.Core;

public class BaseReadModel
{
    public int Id { get; set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}