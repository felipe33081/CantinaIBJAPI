namespace CantinaIBJ.WebApi.Common
{
    public static partial class Constants
    {
        public static class Policy
        {
            public const string MASTERADMIN = "MasterAdmin";
            public const string ADMIN = "Admin";
            public const string USER = "User";
        }
        
        public static class Group 
        {
            public const string MASTERADMIN = "MasterAdmin";
            public const string USER = "User";
            public const string ADMIN = "Admin";
        }

        public static class Cognito
        {
            public const string GROUPS = "cognito:groups";
            public const string USERNAME = "cognito:name";
            public const string ISSUER = "iss";
            public const string ID = "nameidentifier";
            public const string EMAIL = "cognito:email";
            public const string PHONE_NUMBER = "cognito:phone_number";
        }

        public static class Regex
        {
            public const string email = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|([-A-Za-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)@[A-Za-z0-9][\w\.-]*[A-Za-z0-9]\.[A-Za-z][A-Za-z\.]*[A-Za-z]$";
            public const string telefone = @"/^(\([1-9]{2}\)\s?9[0-9]{4}-?[0-9]{4})$/";
            public const string nomeUsuario = @"^(?!.*\.\.)(?!.*\.$)[^\W][\w.]{0,29}$";
            public const string nome = @"^[A-zÀ-ÿ']+\s([A-zÀ-ÿ']\s?)*[A-zÀ-ÿ']+$";
        }
    }
}
