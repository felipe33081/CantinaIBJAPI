using CantinaIBJ.Model.Enumerations;
using CantinaIBJ.WebApi.Models.Create.Core;
using System.ComponentModel.DataAnnotations;

namespace CantinaIBJ.WebApi.Models.Create.User;

public class UserCreateModel : BaseCreateModel
{
    /// <summary>
    /// Nome do usuário
    /// </summary>
    [Required(ErrorMessage = "O campo {0} é obrigatório")]
    public string Name { get; set; }

    /// <summary>
    /// E-mail do usuário
    /// </summary>
    [Required(ErrorMessage = "O campo {0} é obrigatório")]
    [EmailAddress(ErrorMessage = "O campo {0} informado é inválido")]
    public string Email { get; set; }

    /// <summary>
    /// Login do usuário
    /// </summary>
    [Required(ErrorMessage = "O campo {0} é obrigatório")]
    [RegularExpression(@"^[a-zA-Z0-9]{1,50}$", 
        ErrorMessage = "O nome de usuário deve ter no máximo 50 caracteres e só pode conter letras, números.")]
    public string Username { get; set; }

    /// <summary>
    /// Senha do usuário
    /// </summary>
    [Required(ErrorMessage = "O campo {0} é obrigatório")]
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^\\da-zA-Z]).{8,}$", 
        ErrorMessage = "A senha deve ter pelo menos 8 caracteres, uma letra maiúscula, uma letra minúscula, um número e um caractere especial.")]
    public string Password { get; set; }

    /// <summary>
    /// Grupo do usuário
    /// </summary>
    [Required(ErrorMessage = "O campo {0} é obrigatório")]
    public UserGroups Group { get; set; }
}