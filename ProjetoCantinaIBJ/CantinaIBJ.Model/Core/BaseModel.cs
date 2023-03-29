using System.ComponentModel.DataAnnotations;

namespace CantinaIBJ.Model.Core;

public class BaseModel : IModel
{
    [Key]
    public virtual int Id { get; set; }

    public virtual bool IsDeleted { get; set; } = false;
}