using DigitalLibrary.Api.Data;
using DigitalLibrary.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DigitalLibrary.Api.Controllers;

/// <summary>
/// Controller for managing authors in the digital library
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[SwaggerTag("Author management operations")]
public class AuthorsController : ControllerBase
{
    private readonly DigitalLibraryDbContext _context;

    /// <summary>
    /// Initializes a new instance of the AuthorsController
    /// </summary>
    /// <param name="context">The database context</param>
    public AuthorsController(DigitalLibraryDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all authors with optional filtering and pagination
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10, max: 100)</param>
    /// <param name="nationality">Filter by nationality</param>
    /// <param name="search">Search in first name, last name, or biography</param>
    /// <returns>A paginated list of authors</returns>
    /// <response code="200">Returns the list of authors</response>
    /// <response code="400">Invalid parameters provided</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all authors",
        Description = "Retrieves a paginated list of authors with optional filtering by nationality and search terms"
    )]
    [SwaggerResponse(200, "Authors retrieved successfully", typeof(PagedResult<AuthorDto>))]
    [SwaggerResponse(400, "Invalid parameters provided")]
    public async Task<ActionResult<PagedResult<AuthorDto>>> GetAuthors(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 10,
        [FromQuery] string? nationality = null,
        [FromQuery] string? search = null)
    {
        var query = _context.Authors.Include(a => a.Books).AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(nationality))
        {
            query = query.Where(a => a.Nationality != null && a.Nationality.Contains(nationality));
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(a => 
                a.FirstName.Contains(search) || 
                a.LastName.Contains(search) ||
                (a.Biography != null && a.Biography.Contains(search)));
        }

        var totalCount = await query.CountAsync();
        var authors = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuthorDto
            {
                Id = a.Id,
                FirstName = a.FirstName,
                LastName = a.LastName,
                FullName = a.FullName,
                Biography = a.Biography,
                BirthDate = a.BirthDate,
                Nationality = a.Nationality,
                Email = a.Email,
                BookCount = a.Books.Count,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            })
            .ToListAsync();

        var result = new PagedResult<AuthorDto>
        {
            Items = authors,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return Ok(result);
    }

    /// <summary>
    /// Retrieves a specific author by ID
    /// </summary>
    /// <param name="id">The author ID</param>
    /// <returns>The author details including their books</returns>
    /// <response code="200">Returns the author</response>
    /// <response code="404">Author not found</response>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Get author by ID",
        Description = "Retrieves detailed information about a specific author including their published books"
    )]
    [SwaggerResponse(200, "Author retrieved successfully", typeof(AuthorDetailDto))]
    [SwaggerResponse(404, "Author not found")]
    public async Task<ActionResult<AuthorDetailDto>> GetAuthor(int id)
    {
        var author = await _context.Authors
            .Include(a => a.Books.Where(b => b.IsActive))
            .FirstOrDefaultAsync(a => a.Id == id);

        if (author == null)
        {
            return NotFound($"Author with ID {id} not found.");
        }

        var authorDetail = new AuthorDetailDto
        {
            Id = author.Id,
            FirstName = author.FirstName,
            LastName = author.LastName,
            FullName = author.FullName,
            Biography = author.Biography,
            BirthDate = author.BirthDate,
            Nationality = author.Nationality,
            Email = author.Email,
            CreatedAt = author.CreatedAt,
            UpdatedAt = author.UpdatedAt,
            Books = author.Books.Select(b => new BookSummaryDto
            {
                Id = b.Id,
                Title = b.Title,
                ISBN = b.ISBN,
                PublicationDate = b.PublicationDate,
                Genre = b.Genre,
                AverageRating = b.AverageRating,
                IsAvailable = b.IsAvailable
            }).ToList(),
            BookCount = author.Books.Count,
            TotalCopies = author.Books.Sum(b => b.TotalCopies),
            AverageBookRating = author.Books.Where(b => b.AverageRating.HasValue).Any() 
                ? author.Books.Where(b => b.AverageRating.HasValue).Average(b => b.AverageRating!.Value) 
                : null
        };

        return Ok(authorDetail);
    }

    /// <summary>
    /// Creates a new author
    /// </summary>
    /// <param name="createAuthorDto">The author creation data</param>
    /// <returns>The created author</returns>
    /// <response code="201">Author created successfully</response>
    /// <response code="400">Invalid author data provided</response>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create a new author",
        Description = "Creates a new author in the library system with the provided information"
    )]
    [SwaggerResponse(201, "Author created successfully", typeof(AuthorDto))]
    [SwaggerResponse(400, "Invalid author data provided")]
    public async Task<ActionResult<AuthorDto>> CreateAuthor([FromBody] CreateAuthorDto createAuthorDto)
    {
        var author = new Author
        {
            FirstName = createAuthorDto.FirstName,
            LastName = createAuthorDto.LastName,
            Biography = createAuthorDto.Biography,
            BirthDate = createAuthorDto.BirthDate,
            Nationality = createAuthorDto.Nationality,
            Email = createAuthorDto.Email,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Authors.Add(author);
        await _context.SaveChangesAsync();

        var authorDto = new AuthorDto
        {
            Id = author.Id,
            FirstName = author.FirstName,
            LastName = author.LastName,
            FullName = author.FullName,
            Biography = author.Biography,
            BirthDate = author.BirthDate,
            Nationality = author.Nationality,
            Email = author.Email,
            BookCount = 0,
            CreatedAt = author.CreatedAt,
            UpdatedAt = author.UpdatedAt
        };

        return CreatedAtAction(nameof(GetAuthor), new { id = author.Id }, authorDto);
    }

    /// <summary>
    /// Updates an existing author
    /// </summary>
    /// <param name="id">The author ID</param>
    /// <param name="updateAuthorDto">The author update data</param>
    /// <returns>The updated author</returns>
    /// <response code="200">Author updated successfully</response>
    /// <response code="400">Invalid author data provided</response>
    /// <response code="404">Author not found</response>
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Update an existing author",
        Description = "Updates the information of an existing author in the library system"
    )]
    [SwaggerResponse(200, "Author updated successfully", typeof(AuthorDto))]
    [SwaggerResponse(400, "Invalid author data provided")]
    [SwaggerResponse(404, "Author not found")]
    public async Task<ActionResult<AuthorDto>> UpdateAuthor(int id, [FromBody] UpdateAuthorDto updateAuthorDto)
    {
        var author = await _context.Authors.Include(a => a.Books).FirstOrDefaultAsync(a => a.Id == id);
        if (author == null)
        {
            return NotFound($"Author with ID {id} not found.");
        }

        // Update author properties
        author.FirstName = updateAuthorDto.FirstName;
        author.LastName = updateAuthorDto.LastName;
        author.Biography = updateAuthorDto.Biography;
        author.BirthDate = updateAuthorDto.BirthDate;
        author.Nationality = updateAuthorDto.Nationality;
        author.Email = updateAuthorDto.Email;
        author.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var authorDto = new AuthorDto
        {
            Id = author.Id,
            FirstName = author.FirstName,
            LastName = author.LastName,
            FullName = author.FullName,
            Biography = author.Biography,
            BirthDate = author.BirthDate,
            Nationality = author.Nationality,
            Email = author.Email,
            BookCount = author.Books.Count,
            CreatedAt = author.CreatedAt,
            UpdatedAt = author.UpdatedAt
        };

        return Ok(authorDto);
    }

    /// <summary>
    /// Deletes an author
    /// </summary>
    /// <param name="id">The author ID</param>
    /// <returns>No content</returns>
    /// <response code="204">Author deleted successfully</response>
    /// <response code="404">Author not found</response>
    /// <response code="400">Cannot delete author with existing books</response>
    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Delete an author",
        Description = "Deletes an author from the system. Authors with existing books cannot be deleted."
    )]
    [SwaggerResponse(204, "Author deleted successfully")]
    [SwaggerResponse(400, "Cannot delete author with existing books")]
    [SwaggerResponse(404, "Author not found")]
    public async Task<IActionResult> DeleteAuthor(int id)
    {
        var author = await _context.Authors
            .Include(a => a.Books)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (author == null)
        {
            return NotFound($"Author with ID {id} not found.");
        }

        if (author.Books.Any())
        {
            return BadRequest("Cannot delete an author who has books in the system. Please remove or reassign all books first.");
        }

        _context.Authors.Remove(author);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Gets books by a specific author
    /// </summary>
    /// <param name="id">The author ID</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10, max: 100)</param>
    /// <param name="isAvailable">Filter by availability status</param>
    /// <returns>A paginated list of books by the author</returns>
    /// <response code="200">Returns the list of books</response>
    /// <response code="404">Author not found</response>
    [HttpGet("{id}/books")]
    [SwaggerOperation(
        Summary = "Get books by author",
        Description = "Retrieves a paginated list of books written by a specific author"
    )]
    [SwaggerResponse(200, "Books retrieved successfully", typeof(PagedResult<BookSummaryDto>))]
    [SwaggerResponse(404, "Author not found")]
    public async Task<ActionResult<PagedResult<BookSummaryDto>>> GetBooksByAuthor(
        int id,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 10,
        [FromQuery] bool? isAvailable = null)
    {
        var authorExists = await _context.Authors.AnyAsync(a => a.Id == id);
        if (!authorExists)
        {
            return NotFound($"Author with ID {id} not found.");
        }

        var query = _context.Books.Where(b => b.AuthorId == id && b.IsActive);

        if (isAvailable.HasValue)
        {
            query = query.Where(b => b.IsAvailable == isAvailable.Value);
        }

        var totalCount = await query.CountAsync();
        var books = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BookSummaryDto
            {
                Id = b.Id,
                Title = b.Title,
                ISBN = b.ISBN,
                PublicationDate = b.PublicationDate,
                Genre = b.Genre,
                AverageRating = b.AverageRating,
                IsAvailable = b.IsAvailable
            })
            .ToListAsync();

        var result = new PagedResult<BookSummaryDto>
        {
            Items = books,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return Ok(result);
    }

    /// <summary>
    /// Gets statistics for a specific author
    /// </summary>
    /// <param name="id">The author ID</param>
    /// <returns>Author statistics</returns>
    /// <response code="200">Returns the author statistics</response>
    /// <response code="404">Author not found</response>
    [HttpGet("{id}/statistics")]
    [SwaggerOperation(
        Summary = "Get author statistics",
        Description = "Retrieves detailed statistics about an author including book counts, ratings, and loan information"
    )]
    [SwaggerResponse(200, "Author statistics retrieved successfully", typeof(AuthorStatisticsDto))]
    [SwaggerResponse(404, "Author not found")]
    public async Task<ActionResult<AuthorStatisticsDto>> GetAuthorStatistics(int id)
    {
        var author = await _context.Authors
            .Include(a => a.Books)
            .ThenInclude(b => b.Loans)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (author == null)
        {
            return NotFound($"Author with ID {id} not found.");
        }

        var statistics = new AuthorStatisticsDto
        {
            AuthorId = author.Id,
            AuthorName = author.FullName,
            TotalBooks = author.Books.Count,
            ActiveBooks = author.Books.Count(b => b.IsActive),
            TotalCopies = author.Books.Sum(b => b.TotalCopies),
            AvailableCopies = author.Books.Sum(b => b.AvailableCopies),
            TotalLoans = author.Books.SelectMany(b => b.Loans).Count(),
            ActiveLoans = author.Books.SelectMany(b => b.Loans).Count(l => l.Status == LoanStatus.Active),
            AverageBookRating = author.Books.Where(b => b.AverageRating.HasValue).Any()
                ? author.Books.Where(b => b.AverageRating.HasValue).Average(b => b.AverageRating!.Value)
                : null,
            MostPopularBook = author.Books
                .Where(b => b.Loans.Any())
                .OrderByDescending(b => b.Loans.Count)
                .Select(b => new BookSummaryDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    ISBN = b.ISBN,
                    PublicationDate = b.PublicationDate,
                    Genre = b.Genre,
                    AverageRating = b.AverageRating,
                    IsAvailable = b.IsAvailable
                })
                .FirstOrDefault()
        };

        return Ok(statistics);
    }
}