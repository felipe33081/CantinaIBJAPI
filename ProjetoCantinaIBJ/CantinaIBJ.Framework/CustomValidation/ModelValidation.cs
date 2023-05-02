using System;

namespace CantinaIBJ.Framework.CustomValidation
{
    public class ModelValidation : Exception
    {
        public ModelValidation(string error) : base(error)
        {

        }

        public static void When(bool hasError, string message)
        {
            if (hasError)
                throw new ModelValidation(message);
        }
    }
}
