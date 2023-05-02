using System.ComponentModel.DataAnnotations;

namespace CantinaIBJ.Framework.CustomValidation;

public class RequiredIfNotAttribute : ValidationAttribute
{
    string PropName { get; set; }
    object InvalidValue { get; set; }

    readonly RequiredAttribute _innerAttribute;
    public RequiredIfNotAttribute(string propName, object invalidValue)
    {
        PropName = propName;
        InvalidValue = invalidValue;
        _innerAttribute = new RequiredAttribute();
    }

    protected override ValidationResult IsValid(object value, ValidationContext context)
    {
        var dependentValue = context.ObjectInstance.GetType().GetProperty(PropName).GetValue(context.ObjectInstance, null);
        if (dependentValue != null && dependentValue?.ToString() != InvalidValue?.ToString() && !_innerAttribute.IsValid(value))
            return new ValidationResult(ErrorMessage);

        return ValidationResult.Success;
    }
}