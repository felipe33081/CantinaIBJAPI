using System.ComponentModel.DataAnnotations;

namespace CantinaIBJ.Model.Core;

public class BaseModel : IModel
{
    [Key]
    public virtual int Id { get; set; }

    public virtual bool IsDeleted { get; set; } = false;
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public string CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}