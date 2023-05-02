using System;
using System.Collections.Generic;

namespace CantinaIBJ.Integration.Cognito.Model.User;

public class UserGetResponseModel
{
    public List<AttributeModel> UserAttributes { get; set; } = new();

    public bool Enabled { get; set; }
    public string Id { get; set; }//
    public string Name { get; set; }//
    public string PhoneNumber { get; set; }//
    public string Email { get; set; }///
    public string UserStatus { get; set; }//
    public DateTime UserCreateDate { get; set; }
    public DateTime UserLastModifiedDate { get; set; }
    public bool EmailVerified { get; set; }//
}