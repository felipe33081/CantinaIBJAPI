using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class PhoneNumberAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value != null)
        {
            string phoneNumber = value.ToString();

            // Defina sua expressão regular para validar números de telefone
            string pattern = @"^(\([1-9]{2}\)\s?9[0-9]{4}-?[0-9]{4})$";

            if (!Regex.IsMatch(phoneNumber, pattern))
            {
                throw new Exception("Por favor, insira um número de telefone válido.");
            }
        }

        return ValidationResult.Success;
    }
}