using System.ComponentModel.DataAnnotations;

namespace CantinaIBJ.Model.Core;

public class BaseModel : IModel
{
    [Key]
    public virtual int Id { get; set; }

    public virtual bool IsDeleted { get; set; } = false;
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;
}