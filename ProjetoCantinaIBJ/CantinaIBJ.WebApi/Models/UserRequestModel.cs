using System.ComponentModel.DataAnnotations;

namespace CantinaIBJ.WebApi.Models;

public class UserRequestModel
{
    /// <summary>
    /// Login do usuário
    /// </summary>
    [Required(ErrorMessage = "O campo {0} é obrigatório")]
    [RegularExpression(@"^[a-z0-9]{1,50}$",
        ErrorMessage = "O nome de usuário deve ter no máximo 50 caracteres e só pode conter letras minúsculas e números.")]
    public string Username { get; set; }

    /// <summary>
    /// Senha do usuário
    /// </summary>
    [Required(ErrorMessage = "O campo {0} é obrigatório")]
    public string Password { get; set; }
}