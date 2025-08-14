using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace TaskManager.Api.DTOs;

/// <summary>
/// DTO para actualizar una tarea existente
/// </summary>
[SwaggerSchema(
    Title = "Actualizar Tarea",
    Description = "Datos para actualizar una tarea existente, incluyendo su estado de completado"
)]
public class UpdateTaskDto
{
    /// <summary>
    /// Título de la tarea
    /// </summary>
    [Required(ErrorMessage = "El título es obligatorio")]
    [MaxLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Descripción de la tarea
    /// </summary>
    [MaxLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
    public string? Description { get; set; }

    /// <summary>
    /// Indica si la tarea está completada
    /// </summary>
    [SwaggerSchema(
        Description = "Estado de completado de la tarea"
    )]
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Prioridad de la tarea (1=Baja, 2=Media, 3=Alta)
    /// </summary>
    [Range(1, 3, ErrorMessage = "La prioridad debe ser entre 1 y 3")]
    public int Priority { get; set; } = 2;
}