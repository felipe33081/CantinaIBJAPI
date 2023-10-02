using System.ComponentModel.DataAnnotations;

namespace CantinaIBJ.Integration.Cognito.Model.User
{
    public class UserPostRequestModel
    {
        [EmailAddress(ErrorMessage = "E-mail com formato inválido")]
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public bool EmailVerified { get; set; } = true;
    }
}