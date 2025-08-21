using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace DigitalLibrary.Api.Controllers;

/// <summary>
/// Data transfer object for author information
/// </summary>
public class AuthorDto
{
    /// <summary>
    /// Unique identifier for the author
    /// </summary>
    [SwaggerSchema("Unique identifier for the author")]
    public int Id { get; set; }

    /// <summary>
    /// Author's first name
    /// </summary>
    [SwaggerSchema("First name of the author")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Author's last name
    /// </summary>
    [SwaggerSchema("Last name of the author")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Author's full name
    /// </summary>
    [SwaggerSchema("Full name of the author (computed from first and last name)")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Author's biography
    /// </summary>
    [SwaggerSchema("Brief biography or description of the author")]
    public string? Biography { get; set; }

    /// <summary>
    /// Author's birth date
    /// </summary>
    [SwaggerSchema("Date of birth of the author")]
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// Author's nationality
    /// </summary>
    [SwaggerSchema("Nationality or country of origin of the author")]
    public string? Nationality { get; set; }

    /// <summary>
    /// Author's email address
    /// </summary>
    [SwaggerSchema("Email address of the author")]
    public string? Email { get; set; }

    /// <summary>
    /// Number of books by this author
    /// </summary>
    [SwaggerSchema("Total number of books written by this author")]
    public int BookCount { get; set; }

    /// <summary>
    /// Date when the author record was created
    /// </summary>
    [SwaggerSchema("Date when the author was added to the system")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date when the author record was last updated
    /// </summary>
    [SwaggerSchema("Date when the author information was last modified")]
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Detailed author information including books
/// </summary>
public class AuthorDetailDto : AuthorDto
{
    /// <summary>
    /// List of books written by this author
    /// </summary>
    [SwaggerSchema("List of books written by this author")]
    public List<BookSummaryDto> Books { get; set; } = new();

    /// <summary>
    /// Total number of book copies by this author
    /// </summary>
    [SwaggerSchema("Total number of copies of all books by this author")]
    public int TotalCopies { get; set; }

    /// <summary>
    /// Average rating across all books by this author
    /// </summary>
    [SwaggerSchema("Average rating across all books by this author")]
    public decimal? AverageBookRating { get; set; }
}

/// <summary>
/// Data transfer object for creating a new author
/// </summary>
public class CreateAuthorDto
{
    /// <summary>
    /// Author's first name
    /// </summary>
    [Required(ErrorMessage = "First name is required")]
    [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
    [SwaggerSchema("First name of the author")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Author's last name
    /// </summary>
    [Required(ErrorMessage = "Last name is required")]
    [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
    [SwaggerSchema("Last name of the author")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Author's biography
    /// </summary>
    [MaxLength(2000, ErrorMessage = "Biography cannot exceed 2000 characters")]
    [SwaggerSchema("Brief biography or description of the author")]
    public string? Biography { get; set; }

    /// <summary>
    /// Author's birth date
    /// </summary>
    [SwaggerSchema("Date of birth of the author")]
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// Author's nationality
    /// </summary>
    [MaxLength(100, ErrorMessage = "Nationality cannot exceed 100 characters")]
    [SwaggerSchema("Nationality or country of origin")]
    public string? Nationality { get; set; }

    /// <summary>
    /// Author's email address
    /// </summary>
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    [SwaggerSchema("Email address of the author")]
    public string? Email { get; set; }
}

/// <summary>
/// Data transfer object for updating an existing author
/// </summary>
public class UpdateAuthorDto
{
    /// <summary>
    /// Author's first name
    /// </summary>
    [Required(ErrorMessage = "First name is required")]
    [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
    [SwaggerSchema("First name of the author")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Author's last name
    /// </summary>
    [Required(ErrorMessage = "Last name is required")]
    [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
    [SwaggerSchema("Last name of the author")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Author's biography
    /// </summary>
    [MaxLength(2000, ErrorMessage = "Biography cannot exceed 2000 characters")]
    [SwaggerSchema("Brief biography or description of the author")]
    public string? Biography { get; set; }

    /// <summary>
    /// Author's birth date
    /// </summary>
    [SwaggerSchema("Date of birth of the author")]
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// Author's nationality
    /// </summary>
    [MaxLength(100, ErrorMessage = "Nationality cannot exceed 100 characters")]
    [SwaggerSchema("Nationality or country of origin")]
    public string? Nationality { get; set; }

    /// <summary>
    /// Author's email address
    /// </summary>
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    [SwaggerSchema("Email address of the author")]
    public string? Email { get; set; }
}

/// <summary>
/// Book summary information for author details
/// </summary>
public class BookSummaryDto
{
    /// <summary>
    /// Book identifier
    /// </summary>
    [SwaggerSchema("Unique identifier of the book")]
    public int Id { get; set; }

    /// <summary>
    /// Book title
    /// </summary>
    [SwaggerSchema("Title of the book")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// International Standard Book Number
    /// </summary>
    [SwaggerSchema("ISBN number of the book")]
    public string? ISBN { get; set; }

    /// <summary>
    /// Publication date of the book
    /// </summary>
    [SwaggerSchema("Date when the book was published")]
    public DateTime? PublicationDate { get; set; }

    /// <summary>
    /// Book genre or category
    /// </summary>
    [SwaggerSchema("Genre or category of the book")]
    public string? Genre { get; set; }

    /// <summary>
    /// Average rating of the book
    /// </summary>
    [SwaggerSchema("Average rating of the book (1-5 scale)")]
    public decimal? AverageRating { get; set; }

    /// <summary>
    /// Indicates if the book is currently available for loan
    /// </summary>
    [SwaggerSchema("Whether the book is currently available for borrowing")]
    public bool IsAvailable { get; set; }
}

/// <summary>
/// Author statistics information
/// </summary>
public class AuthorStatisticsDto
{
    /// <summary>
    /// Author identifier
    /// </summary>
    [SwaggerSchema("Unique identifier of the author")]
    public int AuthorId { get; set; }

    /// <summary>
    /// Author's full name
    /// </summary>
    [SwaggerSchema("Full name of the author")]
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>
    /// Total number of books by this author
    /// </summary>
    [SwaggerSchema("Total number of books written by this author")]
    public int TotalBooks { get; set; }

    /// <summary>
    /// Number of active books by this author
    /// </summary>
    [SwaggerSchema("Number of active books by this author")]
    public int ActiveBooks { get; set; }

    /// <summary>
    /// Total number of copies of all books by this author
    /// </summary>
    [SwaggerSchema("Total number of copies of all books by this author")]
    public int TotalCopies { get; set; }

    /// <summary>
    /// Number of available copies of all books by this author
    /// </summary>
    [SwaggerSchema("Number of available copies of all books by this author")]
    public int AvailableCopies { get; set; }

    /// <summary>
    /// Total number of loans for books by this author
    /// </summary>
    [SwaggerSchema("Total number of loans for books by this author")]
    public int TotalLoans { get; set; }

    /// <summary>
    /// Number of currently active loans for books by this author
    /// </summary>
    [SwaggerSchema("Number of currently active loans for books by this author")]
    public int ActiveLoans { get; set; }

    /// <summary>
    /// Average rating across all books by this author
    /// </summary>
    [SwaggerSchema("Average rating across all books by this author")]
    public decimal? AverageBookRating { get; set; }

    /// <summary>
    /// Most popular book by this author (based on loan count)
    /// </summary>
    [SwaggerSchema("Most popular book by this author based on loan count")]
    public BookSummaryDto? MostPopularBook { get; set; }
}