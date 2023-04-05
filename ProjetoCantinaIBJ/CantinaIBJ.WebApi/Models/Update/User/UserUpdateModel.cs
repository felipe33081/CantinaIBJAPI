using CantinaIBJ.WebApi.Models.Update.Core;

namespace CantinaIBJ.WebApi.Models.Update.User;

public class UserUpdateModel : BaseUpdateModel
{
    /// <summary>
    /// Nome do usuário
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// E-mail do usuário
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Senha do usuário
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Grupo do usuário
    /// </summary>
    public string Group { get; set; }
}