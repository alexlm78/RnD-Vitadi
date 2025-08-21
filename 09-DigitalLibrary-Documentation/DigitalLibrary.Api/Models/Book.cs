using System.ComponentModel.DataAnnotations;

namespace DigitalLibrary.Api.Models;

/// <summary>
/// Represents a book in the digital library system
/// </summary>
public class Book
{
    /// <summary>
    /// Unique identifier for the book
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Book title
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Book description or summary
    /// </summary>
    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// International Standard Book Number
    /// </summary>
    [MaxLength(20)]
    public string? ISBN { get; set; }

    /// <summary>
    /// Publication date of the book
    /// </summary>
    public DateTime? PublicationDate { get; set; }

    /// <summary>
    /// Publisher name
    /// </summary>
    [MaxLength(200)]
    public string? Publisher { get; set; }

    /// <summary>
    /// Number of pages in the book
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page count must be positive")]
    public int? PageCount { get; set; }

    /// <summary>
    /// Book genre or category
    /// </summary>
    [MaxLength(100)]
    public string? Genre { get; set; }

    /// <summary>
    /// Language of the book
    /// </summary>
    [MaxLength(50)]
    public string? Language { get; set; } = "English";

    /// <summary>
    /// Total number of copies available in the library
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Total copies cannot be negative")]
    public int TotalCopies { get; set; } = 1;

    /// <summary>
    /// Number of copies currently available for loan
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Available copies cannot be negative")]
    public int AvailableCopies { get; set; } = 1;

    /// <summary>
    /// Average rating of the book (1-5 stars)
    /// </summary>
    [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5")]
    public decimal? AverageRating { get; set; }

    /// <summary>
    /// Number of ratings received
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Rating count cannot be negative")]
    public int RatingCount { get; set; } = 0;

    /// <summary>
    /// Indicates if the book is currently active in the system
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Date when the book record was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the book record was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Foreign key to the primary author
    /// </summary>
    public int AuthorId { get; set; }

    /// <summary>
    /// Navigation property to the primary author
    /// </summary>
    public virtual Author Author { get; set; } = null!;

    /// <summary>
    /// Collection of loans for this book
    /// </summary>
    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();

    /// <summary>
    /// Indicates if the book is currently available for loan
    /// </summary>
    public bool IsAvailable => AvailableCopies > 0 && IsActive;

    /// <summary>
    /// Calculates the loan rate (percentage of copies currently on loan)
    /// </summary>
    public decimal LoanRate => TotalCopies > 0 ? 
        ((decimal)(TotalCopies - AvailableCopies) / TotalCopies) * 100 : 0;
}