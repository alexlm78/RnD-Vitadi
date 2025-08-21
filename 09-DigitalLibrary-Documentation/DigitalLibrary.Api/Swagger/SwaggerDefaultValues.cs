using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;

namespace DigitalLibrary.Api.Swagger;

/// <summary>
/// Operation filter to add default values and improve Swagger documentation
/// </summary>
public class SwaggerDefaultValues : IOperationFilter
{
    /// <summary>
    /// Applies default values and enhancements to Swagger operations
    /// </summary>
    /// <param name="operation">The OpenAPI operation</param>
    /// <param name="context">The operation filter context</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var apiDescription = context.ApiDescription;

        // Set deprecated flag if action is marked as obsolete
        operation.Deprecated |= apiDescription.CustomAttributes().OfType<ObsoleteAttribute>().Any();

        // Add response types if not already present
        if (operation.Responses.Any())
        {
            return;
        }

        // Add common response codes
        foreach (var responseType in context.ApiDescription.SupportedResponseTypes)
        {
            var responseKey = responseType.IsDefaultResponse ? "default" : responseType.StatusCode.ToString();
            var response = operation.Responses[responseKey];

            foreach (var contentType in response.Content.Keys)
            {
                if (!responseType.ApiResponseFormats.Any(x => x.MediaType == contentType))
                {
                    response.Content.Remove(contentType);
                }
            }
        }

        // Enhance parameter descriptions
        foreach (var parameter in operation.Parameters)
        {
            var description = apiDescription.ParameterDescriptions
                .FirstOrDefault(p => p.Name == parameter.Name);

            if (description != null)
            {
                parameter.Description ??= description.ModelMetadata?.Description;

                if (description.RouteInfo != null)
                {
                    parameter.Required |= !description.RouteInfo.IsOptional;
                }
            }
        }

        // Add examples for request body
        if (operation.RequestBody?.Content != null)
        {
            foreach (var content in operation.RequestBody.Content)
            {
                if (content.Value.Schema?.Reference != null)
                {
                    var schemaName = content.Value.Schema.Reference.Id;
                    content.Value.Example = GetExampleForSchema(schemaName);
                }
            }
        }
    }

    /// <summary>
    /// Gets example data for a given schema
    /// </summary>
    /// <param name="schemaName">The schema name</param>
    /// <returns>Example data as OpenApiObject</returns>
    private static Microsoft.OpenApi.Any.IOpenApiAny? GetExampleForSchema(string schemaName)
    {
        return schemaName switch
        {
            "CreateBookDto" => new Microsoft.OpenApi.Any.OpenApiObject
            {
                ["title"] = new Microsoft.OpenApi.Any.OpenApiString("The Great Gatsby"),
                ["description"] = new Microsoft.OpenApi.Any.OpenApiString("A classic American novel set in the Jazz Age"),
                ["isbn"] = new Microsoft.OpenApi.Any.OpenApiString("978-0-7432-7356-5"),
                ["publicationDate"] = new Microsoft.OpenApi.Any.OpenApiString("1925-04-10"),
                ["publisher"] = new Microsoft.OpenApi.Any.OpenApiString("Charles Scribner's Sons"),
                ["pageCount"] = new Microsoft.OpenApi.Any.OpenApiInteger(180),
                ["genre"] = new Microsoft.OpenApi.Any.OpenApiString("Fiction"),
                ["language"] = new Microsoft.OpenApi.Any.OpenApiString("English"),
                ["totalCopies"] = new Microsoft.OpenApi.Any.OpenApiInteger(5),
                ["authorId"] = new Microsoft.OpenApi.Any.OpenApiInteger(1)
            },
            "CreateAuthorDto" => new Microsoft.OpenApi.Any.OpenApiObject
            {
                ["firstName"] = new Microsoft.OpenApi.Any.OpenApiString("F. Scott"),
                ["lastName"] = new Microsoft.OpenApi.Any.OpenApiString("Fitzgerald"),
                ["biography"] = new Microsoft.OpenApi.Any.OpenApiString("American novelist and short story writer"),
                ["birthDate"] = new Microsoft.OpenApi.Any.OpenApiString("1896-09-24"),
                ["nationality"] = new Microsoft.OpenApi.Any.OpenApiString("American"),
                ["email"] = new Microsoft.OpenApi.Any.OpenApiString("f.fitzgerald@example.com")
            },
            "CreateLoanDto" => new Microsoft.OpenApi.Any.OpenApiObject
            {
                ["borrowerName"] = new Microsoft.OpenApi.Any.OpenApiString("Alice Johnson"),
                ["borrowerEmail"] = new Microsoft.OpenApi.Any.OpenApiString("alice.johnson@example.com"),
                ["borrowerPhone"] = new Microsoft.OpenApi.Any.OpenApiString("+1-555-0789"),
                ["dueDate"] = new Microsoft.OpenApi.Any.OpenApiString(DateTime.UtcNow.AddDays(14).ToString("yyyy-MM-dd")),
                ["notes"] = new Microsoft.OpenApi.Any.OpenApiString("Regular borrower"),
                ["bookId"] = new Microsoft.OpenApi.Any.OpenApiInteger(1)
            },
            _ => null
        };
    }
}