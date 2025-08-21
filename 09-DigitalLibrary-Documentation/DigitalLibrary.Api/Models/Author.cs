using System.ComponentModel.DataAnnotations;

namespace DigitalLibrary.Api.Models;

/// <summary>
/// Represents an author in the digital library system
/// </summary>
public class Author
{
    /// <summary>
    /// Unique identifier for the author
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Author's first name
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Author's last name
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Author's biography
    /// </summary>
    [MaxLength(2000)]
    public string? Biography { get; set; }

    /// <summary>
    /// Author's birth date
    /// </summary>
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// Author's nationality
    /// </summary>
    [MaxLength(100)]
    public string? Nationality { get; set; }

    /// <summary>
    /// Author's email address
    /// </summary>
    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; set; }

    /// <summary>
    /// Date when the author record was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the author record was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Collection of books written by this author
    /// </summary>
    public virtual ICollection<Book> Books { get; set; } = new List<Book>();

    /// <summary>
    /// Full name of the author (computed property)
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";
}