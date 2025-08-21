using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using DigitalLibrary.Api.Models;

namespace DigitalLibrary.Api.Swagger;

/// <summary>
/// Schema filter to add examples to model schemas
/// </summary>
public class ExampleSchemaFilter : ISchemaFilter
{
    /// <summary>
    /// Applies examples to schemas for better API documentation
    /// </summary>
    /// <param name="schema">The OpenAPI schema</param>
    /// <param name="context">The schema filter context</param>
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(Author))
        {
            schema.Example = new Microsoft.OpenApi.Any.OpenApiObject
            {
                ["id"] = new Microsoft.OpenApi.Any.OpenApiInteger(1),
                ["firstName"] = new Microsoft.OpenApi.Any.OpenApiString("Gabriel"),
                ["lastName"] = new Microsoft.OpenApi.Any.OpenApiString("García Márquez"),
                ["biography"] = new Microsoft.OpenApi.Any.OpenApiString("Colombian novelist and Nobel Prize winner"),
                ["birthDate"] = new Microsoft.OpenApi.Any.OpenApiString("1927-03-06"),
                ["nationality"] = new Microsoft.OpenApi.Any.OpenApiString("Colombian"),
                ["email"] = new Microsoft.OpenApi.Any.OpenApiString("gabo@example.com"),
                ["fullName"] = new Microsoft.OpenApi.Any.OpenApiString("Gabriel García Márquez"),
                ["createdAt"] = new Microsoft.OpenApi.Any.OpenApiString(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")),
                ["updatedAt"] = new Microsoft.OpenApi.Any.OpenApiString(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"))
            };
        }
        else if (context.Type == typeof(Book))
        {
            schema.Example = new Microsoft.OpenApi.Any.OpenApiObject
            {
                ["id"] = new Microsoft.OpenApi.Any.OpenApiInteger(1),
                ["title"] = new Microsoft.OpenApi.Any.OpenApiString("One Hundred Years of Solitude"),
                ["description"] = new Microsoft.OpenApi.Any.OpenApiString("A landmark novel that tells the multi-generational story of the Buendía family"),
                ["isbn"] = new Microsoft.OpenApi.Any.OpenApiString("978-0060883287"),
                ["publicationDate"] = new Microsoft.OpenApi.Any.OpenApiString("1967-05-30"),
                ["publisher"] = new Microsoft.OpenApi.Any.OpenApiString("Harper & Row"),
                ["pageCount"] = new Microsoft.OpenApi.Any.OpenApiInteger(417),
                ["genre"] = new Microsoft.OpenApi.Any.OpenApiString("Magical Realism"),
                ["language"] = new Microsoft.OpenApi.Any.OpenApiString("Spanish"),
                ["totalCopies"] = new Microsoft.OpenApi.Any.OpenApiInteger(5),
                ["availableCopies"] = new Microsoft.OpenApi.Any.OpenApiInteger(3),
                ["averageRating"] = new Microsoft.OpenApi.Any.OpenApiDouble(4.5),
                ["ratingCount"] = new Microsoft.OpenApi.Any.OpenApiInteger(150),
                ["isActive"] = new Microsoft.OpenApi.Any.OpenApiBoolean(true),
                ["authorId"] = new Microsoft.OpenApi.Any.OpenApiInteger(1),
                ["isAvailable"] = new Microsoft.OpenApi.Any.OpenApiBoolean(true),
                ["loanRate"] = new Microsoft.OpenApi.Any.OpenApiDouble(40.0),
                ["createdAt"] = new Microsoft.OpenApi.Any.OpenApiString(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")),
                ["updatedAt"] = new Microsoft.OpenApi.Any.OpenApiString(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"))
            };
        }
        else if (context.Type == typeof(Loan))
        {
            schema.Example = new Microsoft.OpenApi.Any.OpenApiObject
            {
                ["id"] = new Microsoft.OpenApi.Any.OpenApiInteger(1),
                ["borrowerName"] = new Microsoft.OpenApi.Any.OpenApiString("John Smith"),
                ["borrowerEmail"] = new Microsoft.OpenApi.Any.OpenApiString("john.smith@example.com"),
                ["borrowerPhone"] = new Microsoft.OpenApi.Any.OpenApiString("+1-555-0123"),
                ["loanDate"] = new Microsoft.OpenApi.Any.OpenApiString(DateTime.UtcNow.AddDays(-10).ToString("yyyy-MM-ddTHH:mm:ssZ")),
                ["dueDate"] = new Microsoft.OpenApi.Any.OpenApiString(DateTime.UtcNow.AddDays(4).ToString("yyyy-MM-ddTHH:mm:ssZ")),
                ["returnDate"] = new Microsoft.OpenApi.Any.OpenApiNull(),
                ["status"] = new Microsoft.OpenApi.Any.OpenApiInteger(1),
                ["notes"] = new Microsoft.OpenApi.Any.OpenApiString("First-time borrower"),
                ["fineAmount"] = new Microsoft.OpenApi.Any.OpenApiDouble(0.0),
                ["finePaid"] = new Microsoft.OpenApi.Any.OpenApiBoolean(false),
                ["renewalCount"] = new Microsoft.OpenApi.Any.OpenApiInteger(0),
                ["maxRenewals"] = new Microsoft.OpenApi.Any.OpenApiInteger(2),
                ["bookId"] = new Microsoft.OpenApi.Any.OpenApiInteger(1),
                ["isOverdue"] = new Microsoft.OpenApi.Any.OpenApiBoolean(false),
                ["daysOverdue"] = new Microsoft.OpenApi.Any.OpenApiInteger(0),
                ["canRenew"] = new Microsoft.OpenApi.Any.OpenApiBoolean(true),
                ["loanDurationDays"] = new Microsoft.OpenApi.Any.OpenApiInteger(14),
                ["createdAt"] = new Microsoft.OpenApi.Any.OpenApiString(DateTime.UtcNow.AddDays(-10).ToString("yyyy-MM-ddTHH:mm:ssZ")),
                ["updatedAt"] = new Microsoft.OpenApi.Any.OpenApiString(DateTime.UtcNow.AddDays(-10).ToString("yyyy-MM-ddTHH:mm:ssZ"))
            };
        }
        else if (context.Type == typeof(LoanStatus))
        {
            schema.Example = new Microsoft.OpenApi.Any.OpenApiInteger(1);
        }
    }
}