using System.ComponentModel.DataAnnotations;

namespace CantinaIBJ.Framework.CustomValidation
{
    /// <summary>
    /// Validation para cpf e cnpj.
    /// </summary>
    public class IsPhoneAttribute : ValidationAttribute
    {

        public override bool IsValid(object value)
        {
            int phoneLength = ((string)value).Length;
            return phoneLength >= 13 && phoneLength <= 14;
        }
    }
}
