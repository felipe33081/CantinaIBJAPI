namespace CantinaIBJ.Model.Core;

public interface IModel
{
    int Id { get; set; }

    bool IsDeleted { get; set; }

    DateTimeOffset CreatedAt { get; set; }
    DateTimeOffset? UpdatedAt { get; set; }

    string CreatedBy { get; set; }
    string? UpdatedBy { get; set; }
}