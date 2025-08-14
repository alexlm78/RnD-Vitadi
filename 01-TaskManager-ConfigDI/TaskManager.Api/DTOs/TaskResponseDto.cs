using Swashbuckle.AspNetCore.Annotations;

namespace TaskManager.Api.DTOs;

/// <summary>
/// DTO para la respuesta de una tarea
/// </summary>
[SwaggerSchema(
    Title = "Respuesta de Tarea",
    Description = "Representación completa de una tarea con todos sus datos y metadatos"
)]
public class TaskResponseDto
{
    /// <summary>
    /// Identificador único de la tarea
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Título de la tarea
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Descripción de la tarea
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Indica si la tarea está completada
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Fecha de creación de la tarea
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Fecha de última actualización
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Prioridad de la tarea (1=Baja, 2=Media, 3=Alta)
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Descripción textual de la prioridad
    /// </summary>
    [SwaggerSchema(
        Description = "Representación textual del nivel de prioridad"
    )]
    public string PriorityText => Priority switch
    {
        1 => "Baja",
        2 => "Media",
        3 => "Alta",
        _ => "Desconocida"
    };
}