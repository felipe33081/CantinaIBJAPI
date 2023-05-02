using System;

namespace CantinaIBJ.Integration.Cognito.Model.Group
{
    public class GroupPostResponseModel
    {
        public DateTime CreationDate { get; set; }
        public string Description { get; set; }
        public string GroupName { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public int Precedence { get; set; }
        public string UserPoolId { get; set; }
    }
}