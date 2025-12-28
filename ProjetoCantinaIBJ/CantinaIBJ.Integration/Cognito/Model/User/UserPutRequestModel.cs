using System.Collections.Generic;

namespace CantinaIBJ.Integration.Cognito.Model.User
{
    public class UserPutRequestModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool EmailVerified { get; set; } = true;
    }
}