using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using TaskManager.Api.DTOs;

namespace TaskManager.Api.Swagger;

/// <summary>
/// Filtro para agregar ejemplos a los esquemas de Swagger
/// Demuestra personalización avanzada de documentación OpenAPI
/// </summary>
public class ExampleSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(CreateTaskDto))
        {
            schema.Example = new OpenApiObject
            {
                ["title"] = new OpenApiString("Implementar autenticación JWT"),
                ["description"] = new OpenApiString("Configurar JWT authentication en la API para proteger endpoints sensibles"),
                ["priority"] = new OpenApiInteger(3)
            };
        }
        else if (context.Type == typeof(UpdateTaskDto))
        {
            schema.Example = new OpenApiObject
            {
                ["title"] = new OpenApiString("Implementar autenticación JWT - COMPLETADO"),
                ["description"] = new OpenApiString("JWT authentication configurado correctamente con middleware personalizado"),
                ["isCompleted"] = new OpenApiBoolean(true),
                ["priority"] = new OpenApiInteger(3)
            };
        }
        else if (context.Type == typeof(TaskResponseDto))
        {
            schema.Example = new OpenApiObject
            {
                ["id"] = new OpenApiInteger(1),
                ["title"] = new OpenApiString("Configurar logging con Serilog"),
                ["description"] = new OpenApiString("Implementar logging estructurado usando Serilog con múltiples sinks"),
                ["isCompleted"] = new OpenApiBoolean(false),
                ["createdAt"] = new OpenApiString("2024-01-15T10:30:00Z"),
                ["updatedAt"] = new OpenApiString("2024-01-15T10:30:00Z"),
                ["priority"] = new OpenApiInteger(2),
                ["priorityText"] = new OpenApiString("Media")
            };
        }
    }
}