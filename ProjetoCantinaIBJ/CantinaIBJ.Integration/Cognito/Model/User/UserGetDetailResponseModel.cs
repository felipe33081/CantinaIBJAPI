using System;
using System.Collections.Generic;

namespace CantinaIBJ.Integration.Cognito.Model.User
{
    public class UserGetDetailResponseModel
    {
        public List<AttributeModel> UserAttributes { get; set; }
        public object PreferredMFASetting { get; set; }
        public List<object> MFAOptions { get; set; }
        public List<object> UserMFASettingList { get; set; }
        public bool Enabled { get; set; }
        public string Username { get; set; }
        public string UserStatus { get; set; }
        public DateTime UserCreateDate { get; set; }
        public DateTime UserLastModifiedDate { get; set; }
    }
}