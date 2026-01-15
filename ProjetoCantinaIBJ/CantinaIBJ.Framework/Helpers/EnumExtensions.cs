using System.ComponentModel;
using System.Reflection;

namespace CantinaIBJ.Framework.Helpers;

public static class EnumExtensions
{
    public static string ToDescription(this Enum? value)
    {
        if (value == null) return string.Empty;

        FieldInfo? field = value.GetType().GetField(value.ToString());

        if (field == null)
        {
            return value.ToString();
        }

        DescriptionAttribute? attribute =
            Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute))
            as DescriptionAttribute;

        return attribute == null ? value.ToString() : attribute.Description;
    }
}