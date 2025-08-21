using System.ComponentModel.DataAnnotations;

namespace DigitalLibrary.Api.Models;

/// <summary>
/// Represents a book loan in the digital library system
/// </summary>
public class Loan
{
    /// <summary>
    /// Unique identifier for the loan
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the borrower
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string BorrowerName { get; set; } = string.Empty;

    /// <summary>
    /// Email address of the borrower
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string BorrowerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Phone number of the borrower
    /// </summary>
    [MaxLength(20)]
    public string? BorrowerPhone { get; set; }

    /// <summary>
    /// Date when the book was loaned
    /// </summary>
    public DateTime LoanDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Expected return date for the book
    /// </summary>
    public DateTime DueDate { get; set; }

    /// <summary>
    /// Actual return date (null if not yet returned)
    /// </summary>
    public DateTime? ReturnDate { get; set; }

    /// <summary>
    /// Current status of the loan
    /// </summary>
    public LoanStatus Status { get; set; } = LoanStatus.Active;

    /// <summary>
    /// Notes about the loan (condition, special instructions, etc.)
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Fine amount for overdue books
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Fine amount cannot be negative")]
    public decimal FineAmount { get; set; } = 0;

    /// <summary>
    /// Indicates if the fine has been paid
    /// </summary>
    public bool FinePaid { get; set; } = false;

    /// <summary>
    /// Number of renewal attempts for this loan
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Renewal count cannot be negative")]
    public int RenewalCount { get; set; } = 0;

    /// <summary>
    /// Maximum number of renewals allowed
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Max renewals cannot be negative")]
    public int MaxRenewals { get; set; } = 2;

    /// <summary>
    /// Date when the loan record was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the loan record was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Foreign key to the borrowed book
    /// </summary>
    public int BookId { get; set; }

    /// <summary>
    /// Navigation property to the borrowed book
    /// </summary>
    public virtual Book Book { get; set; } = null!;

    /// <summary>
    /// Indicates if the loan is currently overdue
    /// </summary>
    public bool IsOverdue => Status == LoanStatus.Active && DateTime.UtcNow > DueDate;

    /// <summary>
    /// Number of days the loan is overdue (0 if not overdue)
    /// </summary>
    public int DaysOverdue => IsOverdue ? (DateTime.UtcNow - DueDate).Days : 0;

    /// <summary>
    /// Indicates if the loan can be renewed
    /// </summary>
    public bool CanRenew => Status == LoanStatus.Active && RenewalCount < MaxRenewals && !IsOverdue;

    /// <summary>
    /// Duration of the loan in days
    /// </summary>
    public int LoanDurationDays => (DueDate - LoanDate).Days;
}

/// <summary>
/// Enumeration of possible loan statuses
/// </summary>
public enum LoanStatus
{
    /// <summary>
    /// Loan is currently active
    /// </summary>
    Active = 1,

    /// <summary>
    /// Book has been returned on time
    /// </summary>
    Returned = 2,

    /// <summary>
    /// Book was returned late
    /// </summary>
    ReturnedLate = 3,

    /// <summary>
    /// Loan has been cancelled
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// Book is reported as lost
    /// </summary>
    Lost = 5,

    /// <summary>
    /// Loan has been renewed
    /// </summary>
    Renewed = 6
}