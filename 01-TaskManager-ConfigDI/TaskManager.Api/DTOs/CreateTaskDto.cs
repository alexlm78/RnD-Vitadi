using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace TaskManager.Api.DTOs;

/// <summary>
/// DTO para crear una nueva tarea
/// </summary>
[SwaggerSchema(
    Title = "Crear Tarea",
    Description = "Datos necesarios para crear una nueva tarea en el sistema"
)]
public class CreateTaskDto
{
    /// <summary>
    /// Título de la tarea
    /// </summary>
    [Required(ErrorMessage = "El título es obligatorio")]
    [MaxLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
    [SwaggerSchema(
        Description = "Título descriptivo de la tarea (máximo 200 caracteres)"
    )]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Descripción de la tarea
    /// </summary>
    [MaxLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
    [SwaggerSchema(
        Description = "Descripción detallada de la tarea (opcional, máximo 1000 caracteres)"
    )]
    public string? Description { get; set; }

    /// <summary>
    /// Prioridad de la tarea (1=Baja, 2=Media, 3=Alta)
    /// </summary>
    [Range(1, 3, ErrorMessage = "La prioridad debe ser entre 1 y 3")]
    [SwaggerSchema(
        Description = "Nivel de prioridad de la tarea (1=Baja, 2=Media, 3=Alta)"
    )]
    public int Priority { get; set; } = 2;
}