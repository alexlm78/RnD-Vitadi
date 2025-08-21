using DigitalLibrary.Api.Data;
using DigitalLibrary.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DigitalLibrary.Api.Controllers;

/// <summary>
/// Controller for managing books in the digital library
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[SwaggerTag("Book management operations")]
public class BooksController : ControllerBase
{
    private readonly DigitalLibraryDbContext _context;

    /// <summary>
    /// Initializes a new instance of the BooksController
    /// </summary>
    /// <param name="context">The database context</param>
    public BooksController(DigitalLibraryDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all books with optional filtering and pagination
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10, max: 100)</param>
    /// <param name="genre">Filter by genre</param>
    /// <param name="authorId">Filter by author ID</param>
    /// <param name="isAvailable">Filter by availability status</param>
    /// <param name="search">Search in title, description, or ISBN</param>
    /// <returns>A paginated list of books</returns>
    /// <response code="200">Returns the list of books</response>
    /// <response code="400">Invalid parameters provided</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all books",
        Description = "Retrieves a paginated list of books with optional filtering by genre, author, availability, and search terms"
    )]
    [SwaggerResponse(200, "Books retrieved successfully", typeof(PagedResult<BookDto>))]
    [SwaggerResponse(400, "Invalid parameters provided")]
    public async Task<ActionResult<PagedResult<BookDto>>> GetBooks(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 10,
        [FromQuery] string? genre = null,
        [FromQuery] int? authorId = null,
        [FromQuery] bool? isAvailable = null,
        [FromQuery] string? search = null)
    {
        var query = _context.Books.Include(b => b.Author).AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(genre))
        {
            query = query.Where(b => b.Genre != null && b.Genre.Contains(genre));
        }

        if (authorId.HasValue)
        {
            query = query.Where(b => b.AuthorId == authorId.Value);
        }

        if (isAvailable.HasValue)
        {
            query = query.Where(b => b.IsAvailable == isAvailable.Value);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(b => 
                b.Title.Contains(search) || 
                (b.Description != null && b.Description.Contains(search)) ||
                (b.ISBN != null && b.ISBN.Contains(search)));
        }

        var totalCount = await query.CountAsync();
        var books = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BookDto
            {
                Id = b.Id,
                Title = b.Title,
                Description = b.Description,
                ISBN = b.ISBN,
                PublicationDate = b.PublicationDate,
                Publisher = b.Publisher,
                PageCount = b.PageCount,
                Genre = b.Genre,
                Language = b.Language,
                TotalCopies = b.TotalCopies,
                AvailableCopies = b.AvailableCopies,
                AverageRating = b.AverageRating,
                RatingCount = b.RatingCount,
                IsActive = b.IsActive,
                AuthorId = b.AuthorId,
                AuthorName = b.Author.FullName,
                IsAvailable = b.IsAvailable,
                LoanRate = b.LoanRate
            })
            .ToListAsync();

        var result = new PagedResult<BookDto>
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
    /// Retrieves a specific book by ID
    /// </summary>
    /// <param name="id">The book ID</param>
    /// <returns>The book details</returns>
    /// <response code="200">Returns the book</response>
    /// <response code="404">Book not found</response>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Get book by ID",
        Description = "Retrieves detailed information about a specific book including author details and loan history"
    )]
    [SwaggerResponse(200, "Book retrieved successfully", typeof(BookDetailDto))]
    [SwaggerResponse(404, "Book not found")]
    public async Task<ActionResult<BookDetailDto>> GetBook(int id)
    {
        var book = await _context.Books
            .Include(b => b.Author)
            .Include(b => b.Loans.Where(l => l.Status == LoanStatus.Active))
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
        {
            return NotFound($"Book with ID {id} not found.");
        }

        var bookDetail = new BookDetailDto
        {
            Id = book.Id,
            Title = book.Title,
            Description = book.Description,
            ISBN = book.ISBN,
            PublicationDate = book.PublicationDate,
            Publisher = book.Publisher,
            PageCount = book.PageCount,
            Genre = book.Genre,
            Language = book.Language,
            TotalCopies = book.TotalCopies,
            AvailableCopies = book.AvailableCopies,
            AverageRating = book.AverageRating,
            RatingCount = book.RatingCount,
            IsActive = book.IsActive,
            CreatedAt = book.CreatedAt,
            UpdatedAt = book.UpdatedAt,
            Author = new AuthorSummaryDto
            {
                Id = book.Author.Id,
                FullName = book.Author.FullName,
                Nationality = book.Author.Nationality
            },
            ActiveLoansCount = book.Loans.Count,
            IsAvailable = book.IsAvailable,
            LoanRate = book.LoanRate
        };

        return Ok(bookDetail);
    }

    /// <summary>
    /// Creates a new book
    /// </summary>
    /// <param name="createBookDto">The book creation data</param>
    /// <returns>The created book</returns>
    /// <response code="201">Book created successfully</response>
    /// <response code="400">Invalid book data provided</response>
    /// <response code="404">Author not found</response>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create a new book",
        Description = "Creates a new book in the library system with the provided information"
    )]
    [SwaggerResponse(201, "Book created successfully", typeof(BookDto))]
    [SwaggerResponse(400, "Invalid book data provided")]
    [SwaggerResponse(404, "Author not found")]
    public async Task<ActionResult<BookDto>> CreateBook([FromBody] CreateBookDto createBookDto)
    {
        // Verify author exists
        var authorExists = await _context.Authors.AnyAsync(a => a.Id == createBookDto.AuthorId);
        if (!authorExists)
        {
            return NotFound($"Author with ID {createBookDto.AuthorId} not found.");
        }

        var book = new Book
        {
            Title = createBookDto.Title,
            Description = createBookDto.Description,
            ISBN = createBookDto.ISBN,
            PublicationDate = createBookDto.PublicationDate,
            Publisher = createBookDto.Publisher,
            PageCount = createBookDto.PageCount,
            Genre = createBookDto.Genre,
            Language = createBookDto.Language ?? "English",
            TotalCopies = createBookDto.TotalCopies,
            AvailableCopies = createBookDto.TotalCopies, // Initially all copies are available
            AuthorId = createBookDto.AuthorId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        // Load the author for the response
        await _context.Entry(book).Reference(b => b.Author).LoadAsync();

        var bookDto = new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            Description = book.Description,
            ISBN = book.ISBN,
            PublicationDate = book.PublicationDate,
            Publisher = book.Publisher,
            PageCount = book.PageCount,
            Genre = book.Genre,
            Language = book.Language,
            TotalCopies = book.TotalCopies,
            AvailableCopies = book.AvailableCopies,
            AverageRating = book.AverageRating,
            RatingCount = book.RatingCount,
            IsActive = book.IsActive,
            AuthorId = book.AuthorId,
            AuthorName = book.Author.FullName,
            IsAvailable = book.IsAvailable,
            LoanRate = book.LoanRate
        };

        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, bookDto);
    }

    /// <summary>
    /// Updates an existing book
    /// </summary>
    /// <param name="id">The book ID</param>
    /// <param name="updateBookDto">The book update data</param>
    /// <returns>The updated book</returns>
    /// <response code="200">Book updated successfully</response>
    /// <response code="400">Invalid book data provided</response>
    /// <response code="404">Book or author not found</response>
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Update an existing book",
        Description = "Updates the information of an existing book in the library system"
    )]
    [SwaggerResponse(200, "Book updated successfully", typeof(BookDto))]
    [SwaggerResponse(400, "Invalid book data provided")]
    [SwaggerResponse(404, "Book or author not found")]
    public async Task<ActionResult<BookDto>> UpdateBook(int id, [FromBody] UpdateBookDto updateBookDto)
    {
        var book = await _context.Books.Include(b => b.Author).FirstOrDefaultAsync(b => b.Id == id);
        if (book == null)
        {
            return NotFound($"Book with ID {id} not found.");
        }

        // Verify author exists if changing author
        if (updateBookDto.AuthorId != book.AuthorId)
        {
            var authorExists = await _context.Authors.AnyAsync(a => a.Id == updateBookDto.AuthorId);
            if (!authorExists)
            {
                return NotFound($"Author with ID {updateBookDto.AuthorId} not found.");
            }
        }

        // Update book properties
        book.Title = updateBookDto.Title;
        book.Description = updateBookDto.Description;
        book.ISBN = updateBookDto.ISBN;
        book.PublicationDate = updateBookDto.PublicationDate;
        book.Publisher = updateBookDto.Publisher;
        book.PageCount = updateBookDto.PageCount;
        book.Genre = updateBookDto.Genre;
        book.Language = updateBookDto.Language;
        book.TotalCopies = updateBookDto.TotalCopies;
        book.AuthorId = updateBookDto.AuthorId;
        book.IsActive = updateBookDto.IsActive;
        book.UpdatedAt = DateTime.UtcNow;

        // Adjust available copies if total copies changed
        var currentLoans = book.TotalCopies - book.AvailableCopies;
        book.AvailableCopies = Math.Max(0, updateBookDto.TotalCopies - currentLoans);

        await _context.SaveChangesAsync();

        // Reload author if changed
        if (updateBookDto.AuthorId != book.AuthorId)
        {
            await _context.Entry(book).Reference(b => b.Author).LoadAsync();
        }

        var bookDto = new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            Description = book.Description,
            ISBN = book.ISBN,
            PublicationDate = book.PublicationDate,
            Publisher = book.Publisher,
            PageCount = book.PageCount,
            Genre = book.Genre,
            Language = book.Language,
            TotalCopies = book.TotalCopies,
            AvailableCopies = book.AvailableCopies,
            AverageRating = book.AverageRating,
            RatingCount = book.RatingCount,
            IsActive = book.IsActive,
            AuthorId = book.AuthorId,
            AuthorName = book.Author.FullName,
            IsAvailable = book.IsAvailable,
            LoanRate = book.LoanRate
        };

        return Ok(bookDto);
    }

    /// <summary>
    /// Deletes a book (soft delete by setting IsActive to false)
    /// </summary>
    /// <param name="id">The book ID</param>
    /// <returns>No content</returns>
    /// <response code="204">Book deleted successfully</response>
    /// <response code="404">Book not found</response>
    /// <response code="400">Cannot delete book with active loans</response>
    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Delete a book",
        Description = "Soft deletes a book by setting its status to inactive. Books with active loans cannot be deleted."
    )]
    [SwaggerResponse(204, "Book deleted successfully")]
    [SwaggerResponse(400, "Cannot delete book with active loans")]
    [SwaggerResponse(404, "Book not found")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var book = await _context.Books
            .Include(b => b.Loans.Where(l => l.Status == LoanStatus.Active))
            .FirstOrDefaultAsync(b => b.Id == id);

        if (book == null)
        {
            return NotFound($"Book with ID {id} not found.");
        }

        if (book.Loans.Any())
        {
            return BadRequest("Cannot delete a book that has active loans. Please return all copies first.");
        }

        book.IsActive = false;
        book.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Updates the rating of a book
    /// </summary>
    /// <param name="id">The book ID</param>
    /// <param name="ratingDto">The rating data</param>
    /// <returns>The updated book rating information</returns>
    /// <response code="200">Rating updated successfully</response>
    /// <response code="400">Invalid rating value</response>
    /// <response code="404">Book not found</response>
    [HttpPost("{id}/rating")]
    [SwaggerOperation(
        Summary = "Rate a book",
        Description = "Adds a rating to a book and updates the average rating"
    )]
    [SwaggerResponse(200, "Rating added successfully", typeof(BookRatingDto))]
    [SwaggerResponse(400, "Invalid rating value")]
    [SwaggerResponse(404, "Book not found")]
    public async Task<ActionResult<BookRatingDto>> RateBook(int id, [FromBody] AddRatingDto ratingDto)
    {
        var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
        if (book == null)
        {
            return NotFound($"Book with ID {id} not found.");
        }

        // Calculate new average rating
        var totalRating = (book.AverageRating ?? 0) * book.RatingCount + ratingDto.Rating;
        book.RatingCount++;
        book.AverageRating = totalRating / book.RatingCount;
        book.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var result = new BookRatingDto
        {
            BookId = book.Id,
            AverageRating = book.AverageRating.Value,
            RatingCount = book.RatingCount
        };

        return Ok(result);
    }
}