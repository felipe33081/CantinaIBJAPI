using CantinaIBJ.WebApi.Models.Read.Core;

namespace CantinaIBJ.WebApi.Models.Read.User;

public class UserReadModel : BaseReadModel
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
    /// Login do usuário
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// Senha do usuário
    /// </summary>
    public string PasswordHash { get; set; }

    /// <summary>
    /// Grupo do usuário
    /// </summary>
    public string Group { get; set; }
}