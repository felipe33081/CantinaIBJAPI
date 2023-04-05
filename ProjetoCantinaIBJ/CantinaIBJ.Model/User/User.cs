using CantinaIBJ.Model.Core;
using CantinaIBJ.Model.Enumerations;
using ServiceStack.DataAnnotations;

namespace CantinaIBJ.Model.User;

public class User : BaseModel
{
    public string Name { get; set; }

    public string Email { get; set; }

    [Unique]
    public string Username { get; set; }

    [Unique]
    public string PasswordHash { get; set; }

    public Groups Group { get; set; }
}