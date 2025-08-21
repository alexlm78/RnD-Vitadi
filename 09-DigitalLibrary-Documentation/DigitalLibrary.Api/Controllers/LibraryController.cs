using DigitalLibrary.Api.Data;
using DigitalLibrary.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace DigitalLibrary.Api.Controllers;

/// <summary>
/// Controller for basic library information and statistics
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[SwaggerTag("Library information and statistics")]
public class LibraryController : ControllerBase
{
    private readonly DigitalLibraryDbContext _context;

    /// <summary>
    /// Initializes a new instance of the LibraryController
    /// </summary>
    /// <param name="context">The database context</param>
    public LibraryController(DigitalLibraryDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets basic information about the digital library
    /// </summary>
    /// <returns>Library information including statistics</returns>
    /// <response code="200">Returns the library information</response>
    [HttpGet("info")]
    [SwaggerOperation(
        Summary = "Get library information",
        Description = "Retrieves basic information about the digital library including total counts of books, authors, and active loans"
    )]
    [SwaggerResponse(200, "Library information retrieved successfully", typeof(LibraryInfo))]
    public async Task<ActionResult<LibraryInfo>> GetLibraryInfo()
    {
        var totalBooks = await _context.Books.CountAsync();
        var totalAuthors = await _context.Authors.CountAsync();
        var activeLoans = await _context.Loans.CountAsync(l => l.Status == LoanStatus.Active);
        var availableBooks = await _context.Books.SumAsync(b => b.AvailableCopies);

        var libraryInfo = new LibraryInfo
        {
            Name = "Digital Library System",
            Description = "A comprehensive digital library management system",
            TotalBooks = totalBooks,
            TotalAuthors = totalAuthors,
            ActiveLoans = activeLoans,
            AvailableBooks = availableBooks,
            LastUpdated = DateTime.UtcNow
        };

        return Ok(libraryInfo);
    }

    /// <summary>
    /// Gets library statistics
    /// </summary>
    /// <returns>Detailed library statistics</returns>
    /// <response code="200">Returns the library statistics</response>
    [HttpGet("statistics")]
    [SwaggerOperation(
        Summary = "Get library statistics",
        Description = "Retrieves detailed statistics about the library including genre distribution, loan rates, and popular books"
    )]
    [SwaggerResponse(200, "Library statistics retrieved successfully", typeof(LibraryStatistics))]
    public async Task<ActionResult<LibraryStatistics>> GetLibraryStatistics()
    {
        var genreDistribution = await _context.Books
            .Where(b => !string.IsNullOrEmpty(b.Genre))
            .GroupBy(b => b.Genre)
            .Select(g => new GenreCount { Genre = g.Key!, Count = g.Count() })
            .ToListAsync();

        var topRatedBooks = await _context.Books
            .Where(b => b.AverageRating.HasValue)
            .OrderByDescending(b => b.AverageRating)
            .Take(5)
            .Select(b => new BookSummary 
            { 
                Id = b.Id, 
                Title = b.Title, 
                Rating = b.AverageRating ?? 0 
            })
            .ToListAsync();

        var overdueLoans = await _context.Loans
            .CountAsync(l => l.Status == LoanStatus.Active && l.DueDate < DateTime.UtcNow);

        var statistics = new LibraryStatistics
        {
            GenreDistribution = genreDistribution,
            TopRatedBooks = topRatedBooks,
            OverdueLoans = overdueLoans,
            TotalFinesCollected = await _context.Loans.SumAsync(l => l.FineAmount),
            AverageBookRating = await _context.Books
                .Where(b => b.AverageRating.HasValue)
                .AverageAsync(b => b.AverageRating!.Value)
        };

        return Ok(statistics);
    }
}

/// <summary>
/// Basic library information
/// </summary>
public class LibraryInfo
{
    /// <summary>
    /// Name of the library
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the library
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Total number of books in the library
    /// </summary>
    public int TotalBooks { get; set; }

    /// <summary>
    /// Total number of authors in the library
    /// </summary>
    public int TotalAuthors { get; set; }

    /// <summary>
    /// Number of currently active loans
    /// </summary>
    public int ActiveLoans { get; set; }

    /// <summary>
    /// Total number of available book copies
    /// </summary>
    public int AvailableBooks { get; set; }

    /// <summary>
    /// When the information was last updated
    /// </summary>
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Detailed library statistics
/// </summary>
public class LibraryStatistics
{
    /// <summary>
    /// Distribution of books by genre
    /// </summary>
    public List<GenreCount> GenreDistribution { get; set; } = new();

    /// <summary>
    /// Top rated books in the library
    /// </summary>
    public List<BookSummary> TopRatedBooks { get; set; } = new();

    /// <summary>
    /// Number of overdue loans
    /// </summary>
    public int OverdueLoans { get; set; }

    /// <summary>
    /// Total amount of fines collected
    /// </summary>
    public decimal TotalFinesCollected { get; set; }

    /// <summary>
    /// Average rating across all books
    /// </summary>
    public decimal AverageBookRating { get; set; }
}

/// <summary>
/// Genre count information
/// </summary>
public class GenreCount
{
    /// <summary>
    /// Genre name
    /// </summary>
    public string Genre { get; set; } = string.Empty;

    /// <summary>
    /// Number of books in this genre
    /// </summary>
    public int Count { get; set; }
}

/// <summary>
/// Book summary information
/// </summary>
public class BookSummary
{
    /// <summary>
    /// Book identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Book title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Book rating
    /// </summary>
    public decimal Rating { get; set; }
}