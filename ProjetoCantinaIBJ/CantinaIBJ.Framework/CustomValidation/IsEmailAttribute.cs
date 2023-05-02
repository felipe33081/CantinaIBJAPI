using System.ComponentModel.DataAnnotations;

namespace CantinaIBJ.Framework.CustomValidation
{
    /// <summary>
    /// Validation para cpf e cnpj.
    /// </summary>
    public class IsEmailAttribute : ValidationAttribute
    {

        public override bool IsValid(object value)
        {
            try
            {
                string strValue = (string)value;
                var addr = new System.Net.Mail.MailAddress(strValue);
                return addr.Address == strValue;
            }
            catch
            {
                return false;
            }
        }
    }
}
