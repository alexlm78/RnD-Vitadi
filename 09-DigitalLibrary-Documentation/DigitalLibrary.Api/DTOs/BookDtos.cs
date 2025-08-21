using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace DigitalLibrary.Api.Controllers;

/// <summary>
/// Data transfer object for book information
/// </summary>
public class BookDto
{
    /// <summary>
    /// Unique identifier for the book
    /// </summary>
    [SwaggerSchema("Unique identifier for the book")]
    public int Id { get; set; }

    /// <summary>
    /// Book title
    /// </summary>
    [SwaggerSchema("The title of the book")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Book description or summary
    /// </summary>
    [SwaggerSchema("Brief description or summary of the book")]
    public string? Description { get; set; }

    /// <summary>
    /// International Standard Book Number
    /// </summary>
    [SwaggerSchema("ISBN number for the book")]
    public string? ISBN { get; set; }

    /// <summary>
    /// Publication date of the book
    /// </summary>
    [SwaggerSchema("Date when the book was published")]
    public DateTime? PublicationDate { get; set; }

    /// <summary>
    /// Publisher name
    /// </summary>
    [SwaggerSchema("Name of the book publisher")]
    public string? Publisher { get; set; }

    /// <summary>
    /// Number of pages in the book
    /// </summary>
    [SwaggerSchema("Total number of pages in the book")]
    public int? PageCount { get; set; }

    /// <summary>
    /// Book genre or category
    /// </summary>
    [SwaggerSchema("Genre or category of the book (e.g., Fiction, Science, History)")]
    public string? Genre { get; set; }

    /// <summary>
    /// Language of the book
    /// </summary>
    [SwaggerSchema("Language in which the book is written")]
    public string? Language { get; set; }

    /// <summary>
    /// Total number of copies available in the library
    /// </summary>
    [SwaggerSchema("Total number of copies owned by the library")]
    public int TotalCopies { get; set; }

    /// <summary>
    /// Number of copies currently available for loan
    /// </summary>
    [SwaggerSchema("Number of copies currently available for borrowing")]
    public int AvailableCopies { get; set; }

    /// <summary>
    /// Average rating of the book (1-5 stars)
    /// </summary>
    [SwaggerSchema("Average rating given by readers (1-5 scale)")]
    public decimal? AverageRating { get; set; }

    /// <summary>
    /// Number of ratings received
    /// </summary>
    [SwaggerSchema("Total number of ratings received")]
    public int RatingCount { get; set; }

    /// <summary>
    /// Indicates if the book is currently active in the system
    /// </summary>
    [SwaggerSchema("Whether the book is active in the library system")]
    public bool IsActive { get; set; }

    /// <summary>
    /// Author ID
    /// </summary>
    [SwaggerSchema("Unique identifier of the book's author")]
    public int AuthorId { get; set; }

    /// <summary>
    /// Author's full name
    /// </summary>
    [SwaggerSchema("Full name of the book's author")]
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the book is currently available for loan
    /// </summary>
    [SwaggerSchema("Whether the book is currently available for borrowing")]
    public bool IsAvailable { get; set; }

    /// <summary>
    /// Loan rate percentage
    /// </summary>
    [SwaggerSchema("Percentage of copies currently on loan")]
    public decimal LoanRate { get; set; }
}

/// <summary>
/// Detailed book information including additional metadata
/// </summary>
public class BookDetailDto : BookDto
{
    /// <summary>
    /// Date when the book record was created
    /// </summary>
    [SwaggerSchema("Date when the book was added to the system")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date when the book record was last updated
    /// </summary>
    [SwaggerSchema("Date when the book information was last modified")]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Author information
    /// </summary>
    [SwaggerSchema("Detailed information about the book's author")]
    public AuthorSummaryDto Author { get; set; } = null!;

    /// <summary>
    /// Number of active loans for this book
    /// </summary>
    [SwaggerSchema("Number of copies currently on loan")]
    public int ActiveLoansCount { get; set; }
}

/// <summary>
/// Data transfer object for creating a new book
/// </summary>
public class CreateBookDto
{
    /// <summary>
    /// Book title
    /// </summary>
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(500, ErrorMessage = "Title cannot exceed 500 characters")]
    [SwaggerSchema("The title of the book")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Book description or summary
    /// </summary>
    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    [SwaggerSchema("Brief description or summary of the book")]
    public string? Description { get; set; }

    /// <summary>
    /// International Standard Book Number
    /// </summary>
    [MaxLength(20, ErrorMessage = "ISBN cannot exceed 20 characters")]
    [SwaggerSchema("ISBN number for the book")]
    public string? ISBN { get; set; }

    /// <summary>
    /// Publication date of the book
    /// </summary>
    [SwaggerSchema("Date when the book was published")]
    public DateTime? PublicationDate { get; set; }

    /// <summary>
    /// Publisher name
    /// </summary>
    [MaxLength(200, ErrorMessage = "Publisher name cannot exceed 200 characters")]
    [SwaggerSchema("Name of the book publisher")]
    public string? Publisher { get; set; }

    /// <summary>
    /// Number of pages in the book
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page count must be positive")]
    [SwaggerSchema("Total number of pages in the book")]
    public int? PageCount { get; set; }

    /// <summary>
    /// Book genre or category
    /// </summary>
    [MaxLength(100, ErrorMessage = "Genre cannot exceed 100 characters")]
    [SwaggerSchema("Genre or category of the book")]
    public string? Genre { get; set; }

    /// <summary>
    /// Language of the book
    /// </summary>
    [MaxLength(50, ErrorMessage = "Language cannot exceed 50 characters")]
    [SwaggerSchema("Language in which the book is written")]
    public string? Language { get; set; }

    /// <summary>
    /// Total number of copies to add to the library
    /// </summary>
    [Required(ErrorMessage = "Total copies is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Total copies must be at least 1")]
    [SwaggerSchema("Total number of copies to add to the library")]
    public int TotalCopies { get; set; } = 1;

    /// <summary>
    /// Author ID
    /// </summary>
    [Required(ErrorMessage = "Author ID is required")]
    [SwaggerSchema("Unique identifier of the book's author")]
    public int AuthorId { get; set; }
}

/// <summary>
/// Data transfer object for updating an existing book
/// </summary>
public class UpdateBookDto
{
    /// <summary>
    /// Book title
    /// </summary>
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(500, ErrorMessage = "Title cannot exceed 500 characters")]
    [SwaggerSchema("The title of the book")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Book description or summary
    /// </summary>
    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    [SwaggerSchema("Brief description or summary of the book")]
    public string? Description { get; set; }

    /// <summary>
    /// International Standard Book Number
    /// </summary>
    [MaxLength(20, ErrorMessage = "ISBN cannot exceed 20 characters")]
    [SwaggerSchema("ISBN number for the book")]
    public string? ISBN { get; set; }

    /// <summary>
    /// Publication date of the book
    /// </summary>
    [SwaggerSchema("Date when the book was published")]
    public DateTime? PublicationDate { get; set; }

    /// <summary>
    /// Publisher name
    /// </summary>
    [MaxLength(200, ErrorMessage = "Publisher name cannot exceed 200 characters")]
    [SwaggerSchema("Name of the book publisher")]
    public string? Publisher { get; set; }

    /// <summary>
    /// Number of pages in the book
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page count must be positive")]
    [SwaggerSchema("Total number of pages in the book")]
    public int? PageCount { get; set; }

    /// <summary>
    /// Book genre or category
    /// </summary>
    [MaxLength(100, ErrorMessage = "Genre cannot exceed 100 characters")]
    [SwaggerSchema("Genre or category of the book")]
    public string? Genre { get; set; }

    /// <summary>
    /// Language of the book
    /// </summary>
    [MaxLength(50, ErrorMessage = "Language cannot exceed 50 characters")]
    [SwaggerSchema("Language in which the book is written")]
    public string? Language { get; set; }

    /// <summary>
    /// Total number of copies in the library
    /// </summary>
    [Required(ErrorMessage = "Total copies is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Total copies must be at least 1")]
    [SwaggerSchema("Total number of copies owned by the library")]
    public int TotalCopies { get; set; }

    /// <summary>
    /// Author ID
    /// </summary>
    [Required(ErrorMessage = "Author ID is required")]
    [SwaggerSchema("Unique identifier of the book's author")]
    public int AuthorId { get; set; }

    /// <summary>
    /// Indicates if the book is currently active in the system
    /// </summary>
    [SwaggerSchema("Whether the book should be active in the library system")]
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Data transfer object for adding a rating to a book
/// </summary>
public class AddRatingDto
{
    /// <summary>
    /// Rating value (1-5 stars)
    /// </summary>
    [Required(ErrorMessage = "Rating is required")]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    [SwaggerSchema("Rating value from 1 to 5 stars")]
    public decimal Rating { get; set; }
}

/// <summary>
/// Data transfer object for book rating information
/// </summary>
public class BookRatingDto
{
    /// <summary>
    /// Book ID
    /// </summary>
    [SwaggerSchema("Unique identifier of the rated book")]
    public int BookId { get; set; }

    /// <summary>
    /// Average rating of the book
    /// </summary>
    [SwaggerSchema("Current average rating of the book")]
    public decimal AverageRating { get; set; }

    /// <summary>
    /// Total number of ratings
    /// </summary>
    [SwaggerSchema("Total number of ratings received")]
    public int RatingCount { get; set; }
}

/// <summary>
/// Author summary information
/// </summary>
public class AuthorSummaryDto
{
    /// <summary>
    /// Author ID
    /// </summary>
    [SwaggerSchema("Unique identifier of the author")]
    public int Id { get; set; }

    /// <summary>
    /// Author's full name
    /// </summary>
    [SwaggerSchema("Full name of the author")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Author's nationality
    /// </summary>
    [SwaggerSchema("Nationality of the author")]
    public string? Nationality { get; set; }
}

/// <summary>
/// Paginated result container
/// </summary>
/// <typeparam name="T">Type of items in the result</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// List of items for the current page
    /// </summary>
    [SwaggerSchema("Items for the current page")]
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    [SwaggerSchema("Total number of items available")]
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number
    /// </summary>
    [SwaggerSchema("Current page number")]
    public int Page { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    [SwaggerSchema("Number of items per page")]
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    [SwaggerSchema("Total number of pages available")]
    public int TotalPages { get; set; }

    /// <summary>
    /// Indicates if there is a previous page
    /// </summary>
    [SwaggerSchema("Whether there is a previous page")]
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Indicates if there is a next page
    /// </summary>
    [SwaggerSchema("Whether there is a next page")]
    public bool HasNextPage => Page < TotalPages;
}