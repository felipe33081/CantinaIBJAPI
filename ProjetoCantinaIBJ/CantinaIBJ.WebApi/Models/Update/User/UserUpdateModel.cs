using CantinaIBJ.Model.Enumerations;
using CantinaIBJ.WebApi.Models.Update.Core;
using System.ComponentModel.DataAnnotations;

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
    [EmailAddress(ErrorMessage = "O campo {0} informado é inválido")]
    public string Email { get; set; }

    /// <summary>
    /// Senha do usuário
    /// </summary>
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^\\da-zA-Z]).{8,}$",
        ErrorMessage = "A senha deve ter pelo menos 8 caracteres, uma letra maiúscula, uma letra minúscula, um número e um caractere especial.")]
    public string Password { get; set; }

    /// <summary>
    /// Grupo do usuário
    /// </summary>
    public Groups Group { get; set; }
}