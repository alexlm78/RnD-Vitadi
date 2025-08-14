using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Models;

/// <summary>
/// Representa una tarea en el sistema de gestión de tareas
/// </summary>
public class TaskItem
{
    /// <summary>
    /// Identificador único de la tarea
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Título de la tarea
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Descripción detallada de la tarea
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Indica si la tarea ha sido completada
    /// </summary>
    public bool IsCompleted { get; set; } = false;

    /// <summary>
    /// Fecha y hora de creación de la tarea
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Fecha y hora de última actualización de la tarea
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Prioridad de la tarea (1=Baja, 2=Media, 3=Alta)
    /// </summary>
    [Range(1, 3)]
    public int Priority { get; set; } = 2;
}