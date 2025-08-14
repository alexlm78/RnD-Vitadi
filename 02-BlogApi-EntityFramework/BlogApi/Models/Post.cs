using System.ComponentModel.DataAnnotations;

namespace BlogApi.Models;

/// <summary>
/// Entidad que representa un post del blog
/// </summary>
public class Post
{
    /// <summary>
    /// Identificador único del post
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Título del post
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Contenido completo del post
    /// </summary>
    [Required]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Resumen o extracto del post
    /// </summary>
    [MaxLength(500)]
    public string? Summary { get; set; }

    /// <summary>
    /// Nombre del autor del post
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de creación del post
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Fecha de última actualización del post
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indica si el post está publicado
    /// </summary>
    public bool IsPublished { get; set; } = false;

    /// <summary>
    /// Fecha de publicación del post
    /// </summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// Tags asociados al post (separados por comas)
    /// </summary>
    [MaxLength(500)]
    public string? Tags { get; set; }

    /// <summary>
    /// Colección de comentarios asociados a este post
    /// Relación uno a muchos: Un post puede tener muchos comentarios
    /// </summary>
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
}