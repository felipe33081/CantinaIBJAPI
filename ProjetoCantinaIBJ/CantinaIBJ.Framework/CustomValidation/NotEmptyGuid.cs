using System;
using System.ComponentModel.DataAnnotations;

namespace CantinaIBJ.Framework.CustomValidation
{
    /// <summary>
    /// Validation para Guid Vazios.
    /// </summary>
    public class NotEmptyGuid : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is null)
            {
                return true;
            }
            switch (value)
            {
                case Guid guid:
                    return guid != Guid.Empty;
                default:
                    return true;
            }
        }
    }
}
