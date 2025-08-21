using DigitalLibrary.Api.Models;
using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace DigitalLibrary.Api.Controllers;

/// <summary>
/// Data transfer object for loan information
/// </summary>
public class LoanDto
{
    /// <summary>
    /// Unique identifier for the loan
    /// </summary>
    [SwaggerSchema("Unique identifier for the loan")]
    public int Id { get; set; }

    /// <summary>
    /// Name of the borrower
    /// </summary>
    [SwaggerSchema("Full name of the person borrowing the book")]
    public string BorrowerName { get; set; } = string.Empty;

    /// <summary>
    /// Email address of the borrower
    /// </summary>
    [SwaggerSchema("Email address of the borrower")]
    public string BorrowerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Phone number of the borrower
    /// </summary>
    [SwaggerSchema("Phone number of the borrower")]
    public string? BorrowerPhone { get; set; }

    /// <summary>
    /// Date when the book was loaned
    /// </summary>
    [SwaggerSchema("Date when the book was borrowed")]
    public DateTime LoanDate { get; set; }

    /// <summary>
    /// Expected return date for the book
    /// </summary>
    [SwaggerSchema("Date when the book is expected to be returned")]
    public DateTime DueDate { get; set; }

    /// <summary>
    /// Actual return date (null if not yet returned)
    /// </summary>
    [SwaggerSchema("Date when the book was actually returned (null if still on loan)")]
    public DateTime? ReturnDate { get; set; }

    /// <summary>
    /// Current status of the loan
    /// </summary>
    [SwaggerSchema("Current status of the loan")]
    public LoanStatus Status { get; set; }

    /// <summary>
    /// Notes about the loan
    /// </summary>
    [SwaggerSchema("Additional notes about the loan")]
    public string? Notes { get; set; }

    /// <summary>
    /// Fine amount for overdue books
    /// </summary>
    [SwaggerSchema("Fine amount charged for overdue return")]
    public decimal FineAmount { get; set; }

    /// <summary>
    /// Indicates if the fine has been paid
    /// </summary>
    [SwaggerSchema("Whether the fine has been paid")]
    public bool FinePaid { get; set; }

    /// <summary>
    /// Number of renewal attempts for this loan
    /// </summary>
    [SwaggerSchema("Number of times this loan has been renewed")]
    public int RenewalCount { get; set; }

    /// <summary>
    /// Maximum number of renewals allowed
    /// </summary>
    [SwaggerSchema("Maximum number of renewals allowed for this loan")]
    public int MaxRenewals { get; set; }

    /// <summary>
    /// ID of the borrowed book
    /// </summary>
    [SwaggerSchema("Unique identifier of the borrowed book")]
    public int BookId { get; set; }

    /// <summary>
    /// Title of the borrowed book
    /// </summary>
    [SwaggerSchema("Title of the borrowed book")]
    public string BookTitle { get; set; } = string.Empty;

    /// <summary>
    /// ISBN of the borrowed book
    /// </summary>
    [SwaggerSchema("ISBN of the borrowed book")]
    public string? BookISBN { get; set; }

    /// <summary>
    /// Author name of the borrowed book
    /// </summary>
    [SwaggerSchema("Author of the borrowed book")]
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the loan is currently overdue
    /// </summary>
    [SwaggerSchema("Whether the loan is currently overdue")]
    public bool IsOverdue { get; set; }

    /// <summary>
    /// Number of days the loan is overdue
    /// </summary>
    [SwaggerSchema("Number of days the loan is overdue (0 if not overdue)")]
    public int DaysOverdue { get; set; }

    /// <summary>
    /// Indicates if the loan can be renewed
    /// </summary>
    [SwaggerSchema("Whether the loan can be renewed")]
    public bool CanRenew { get; set; }

    /// <summary>
    /// Duration of the loan in days
    /// </summary>
    [SwaggerSchema("Total duration of the loan in days")]
    public int LoanDurationDays { get; set; }
}

/// <summary>
/// Detailed loan information including additional metadata
/// </summary>
public class LoanDetailDto : LoanDto
{
    /// <summary>
    /// Date when the loan record was created
    /// </summary>
    [SwaggerSchema("Date when the loan was created in the system")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date when the loan record was last updated
    /// </summary>
    [SwaggerSchema("Date when the loan information was last modified")]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Detailed book information
    /// </summary>
    [SwaggerSchema("Detailed information about the borrowed book")]
    public BookLoanInfoDto Book { get; set; } = null!;
}

/// <summary>
/// Data transfer object for creating a new loan
/// </summary>
public class CreateLoanDto
{
    /// <summary>
    /// Name of the borrower
    /// </summary>
    [Required(ErrorMessage = "Borrower name is required")]
    [MaxLength(200, ErrorMessage = "Borrower name cannot exceed 200 characters")]
    [SwaggerSchema("Full name of the person borrowing the book")]
    public string BorrowerName { get; set; } = string.Empty;

    /// <summary>
    /// Email address of the borrower
    /// </summary>
    [Required(ErrorMessage = "Borrower email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    [SwaggerSchema("Email address of the borrower")]
    public string BorrowerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Phone number of the borrower
    /// </summary>
    [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    [SwaggerSchema("Phone number of the borrower")]
    public string? BorrowerPhone { get; set; }

    /// <summary>
    /// Expected return date for the book (optional, defaults to 14 days from now)
    /// </summary>
    [SwaggerSchema("Expected return date (defaults to 14 days from now if not specified)")]
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Notes about the loan
    /// </summary>
    [MaxLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    [SwaggerSchema("Additional notes about the loan")]
    public string? Notes { get; set; }

    /// <summary>
    /// ID of the book to borrow
    /// </summary>
    [Required(ErrorMessage = "Book ID is required")]
    [SwaggerSchema("Unique identifier of the book to borrow")]
    public int BookId { get; set; }
}

/// <summary>
/// Data transfer object for returning a loan
/// </summary>
public class ReturnLoanDto
{
    /// <summary>
    /// Actual return date (optional, defaults to current date/time)
    /// </summary>
    [SwaggerSchema("Actual return date (defaults to current date/time if not specified)")]
    public DateTime? ReturnDate { get; set; }

    /// <summary>
    /// Notes about the return
    /// </summary>
    [MaxLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    [SwaggerSchema("Additional notes about the book return")]
    public string? Notes { get; set; }
}

/// <summary>
/// Data transfer object for renewing a loan
/// </summary>
public class RenewLoanDto
{
    /// <summary>
    /// Number of days to extend the loan (optional, defaults to 14 days)
    /// </summary>
    [Range(1, 90, ErrorMessage = "Extension days must be between 1 and 90")]
    [SwaggerSchema("Number of days to extend the loan (defaults to 14 if not specified)")]
    public int? ExtensionDays { get; set; }
}

/// <summary>
/// Book information for loan details
/// </summary>
public class BookLoanInfoDto
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
    [SwaggerSchema("ISBN of the book")]
    public string? ISBN { get; set; }

    /// <summary>
    /// Author's full name
    /// </summary>
    [SwaggerSchema("Full name of the book's author")]
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>
    /// Book genre or category
    /// </summary>
    [SwaggerSchema("Genre or category of the book")]
    public string? Genre { get; set; }

    /// <summary>
    /// Publisher name
    /// </summary>
    [SwaggerSchema("Name of the book publisher")]
    public string? Publisher { get; set; }
}