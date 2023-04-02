using CantinaIBJ.WebApi.CustomValidation;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace CantinaIBJ.WebApi.Configurations;

public class DescribeEnumMembers : ISchemaFilter
{
    private readonly XDocument mXmlComments;
    public DescribeEnumMembers(XDocument argXmlComments)
    {
        mXmlComments = argXmlComments;
    }

    public void Apply(OpenApiSchema argSchema, SchemaFilterContext argContext)
    {
        var EnumType = argContext.Type;

        if (!EnumType.IsEnum) return;

        var sb = new StringBuilder(argSchema.Description);

        sb.AppendLine("<p>Valores Possíveis:</p>");
        sb.AppendLine("<ul>");

        foreach (var EnumMemberName in Enum.GetNames(EnumType))
        {
            var FullEnumMemberName = $"F:{EnumType.FullName}.{EnumMemberName}";

            var EnumMemberDescription = mXmlComments.XPathEvaluate(
              $"normalize-space(//member[@name = '{FullEnumMemberName}']/summary/text())"
            ) as string;

            if (string.IsNullOrEmpty(EnumMemberDescription))
                return;
            else
                sb.AppendLine($"<li><b>{EnumMemberName}</b>: {EnumMemberDescription}</li>");
        }

        sb.AppendLine("</ul>");

        argSchema.Description = sb.ToString();
    }
}


public class IgnoreEnumSchemaFilter : ISchemaFilter
{
    private readonly XDocument mXmlComments;
    public IgnoreEnumSchemaFilter(XDocument argXmlComments)
    {
        mXmlComments = argXmlComments;
    }
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            var enumOpenApiStrings = new List<IOpenApiAny>();

            foreach (var enumValue in Enum.GetValues(context.Type))
            {
                var member = context.Type.GetMember(enumValue.ToString())[0];
                if (!member.GetCustomAttributes<IgnoreEnumAttribute>().Any())
                {
                    enumOpenApiStrings.Add(new OpenApiString(enumValue.ToString()));
                }
            }

            schema.Enum = enumOpenApiStrings;
        }
    }
}