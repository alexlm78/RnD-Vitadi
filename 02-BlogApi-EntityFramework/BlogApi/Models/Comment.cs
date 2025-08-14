using System.ComponentModel.DataAnnotations;

namespace BlogApi.Models;

/// <summary>
/// Entidad que representa un comentario en un post del blog
/// </summary>
public class Comment
{
    /// <summary>
    /// Identificador único del comentario
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Contenido del comentario
    /// </summary>
    [Required]
    [MaxLength(1000)]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del autor del comentario
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>
    /// Email del autor del comentario
    /// </summary>
    [Required]
    [MaxLength(200)]
    [EmailAddress]
    public string AuthorEmail { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de creación del comentario
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indica si el comentario está aprobado para mostrar
    /// </summary>
    public bool IsApproved { get; set; } = false;

    /// <summary>
    /// Identificador del post al que pertenece este comentario
    /// Clave foránea hacia Post
    /// </summary>
    [Required]
    public int PostId { get; set; }

    /// <summary>
    /// Navegación hacia el post al que pertenece este comentario
    /// Relación muchos a uno: Muchos comentarios pertenecen a un post
    /// </summary>
    public virtual Post Post { get; set; } = null!;
}