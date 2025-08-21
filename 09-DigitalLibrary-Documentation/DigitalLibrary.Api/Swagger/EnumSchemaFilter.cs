using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;

namespace DigitalLibrary.Api.Swagger;

/// <summary>
/// Schema filter to enhance enum documentation in Swagger
/// </summary>
public class EnumSchemaFilter : ISchemaFilter
{
    /// <summary>
    /// Applies enum enhancements to the schema
    /// </summary>
    /// <param name="schema">The OpenAPI schema</param>
    /// <param name="context">The schema filter context</param>
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Enum.Clear();
            var enumValues = new List<Microsoft.OpenApi.Any.IOpenApiAny>();
            var enumDescriptions = new List<string>();

            foreach (var enumValue in Enum.GetValues(context.Type))
            {
                var enumMember = context.Type.GetMember(enumValue.ToString()!).FirstOrDefault();
                var descriptionAttribute = enumMember?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .Cast<DescriptionAttribute>().FirstOrDefault();

                var description = descriptionAttribute?.Description ?? enumValue.ToString();
                var numericValue = (int)enumValue;

                enumValues.Add(new Microsoft.OpenApi.Any.OpenApiInteger(numericValue));
                enumDescriptions.Add($"{numericValue} = {enumValue} ({description})");
            }

            schema.Enum = enumValues;
            schema.Description = $"Possible values:\n{string.Join("\n", enumDescriptions)}";
        }
    }
}