using DigitalLibrary.Api.Data;
using DigitalLibrary.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DigitalLibrary.Api.Controllers;

/// <summary>
/// Controller for managing book loans in the digital library
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[SwaggerTag("Loan management operations")]
public class LoansController : ControllerBase
{
    private readonly DigitalLibraryDbContext _context;

    /// <summary>
    /// Initializes a new instance of the LoansController
    /// </summary>
    /// <param name="context">The database context</param>
    public LoansController(DigitalLibraryDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all loans with optional filtering and pagination
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10, max: 100)</param>
    /// <param name="status">Filter by loan status</param>
    /// <param name="bookId">Filter by book ID</param>
    /// <param name="borrowerEmail">Filter by borrower email</param>
    /// <param name="isOverdue">Filter by overdue status</param>
    /// <returns>A paginated list of loans</returns>
    /// <response code="200">Returns the list of loans</response>
    /// <response code="400">Invalid parameters provided</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all loans",
        Description = "Retrieves a paginated list of loans with optional filtering by status, book, borrower, and overdue status"
    )]
    [SwaggerResponse(200, "Loans retrieved successfully", typeof(PagedResult<LoanDto>))]
    [SwaggerResponse(400, "Invalid parameters provided")]
    public async Task<ActionResult<PagedResult<LoanDto>>> GetLoans(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 10,
        [FromQuery] LoanStatus? status = null,
        [FromQuery] int? bookId = null,
        [FromQuery] string? borrowerEmail = null,
        [FromQuery] bool? isOverdue = null)
    {
        var query = _context.Loans.Include(l => l.Book).ThenInclude(b => b.Author).AsQueryable();

        // Apply filters
        if (status.HasValue)
        {
            query = query.Where(l => l.Status == status.Value);
        }

        if (bookId.HasValue)
        {
            query = query.Where(l => l.BookId == bookId.Value);
        }

        if (!string.IsNullOrEmpty(borrowerEmail))
        {
            query = query.Where(l => l.BorrowerEmail.Contains(borrowerEmail));
        }

        if (isOverdue.HasValue)
        {
            if (isOverdue.Value)
            {
                query = query.Where(l => l.Status == LoanStatus.Active && l.DueDate < DateTime.UtcNow);
            }
            else
            {
                query = query.Where(l => l.Status != LoanStatus.Active || l.DueDate >= DateTime.UtcNow);
            }
        }

        var totalCount = await query.CountAsync();
        var loans = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LoanDto
            {
                Id = l.Id,
                BorrowerName = l.BorrowerName,
                BorrowerEmail = l.BorrowerEmail,
                BorrowerPhone = l.BorrowerPhone,
                LoanDate = l.LoanDate,
                DueDate = l.DueDate,
                ReturnDate = l.ReturnDate,
                Status = l.Status,
                Notes = l.Notes,
                FineAmount = l.FineAmount,
                FinePaid = l.FinePaid,
                RenewalCount = l.RenewalCount,
                MaxRenewals = l.MaxRenewals,
                BookId = l.BookId,
                BookTitle = l.Book.Title,
                BookISBN = l.Book.ISBN,
                AuthorName = l.Book.Author.FullName,
                IsOverdue = l.IsOverdue,
                DaysOverdue = l.DaysOverdue,
                CanRenew = l.CanRenew,
                LoanDurationDays = l.LoanDurationDays
            })
            .ToListAsync();

        var result = new PagedResult<LoanDto>
        {
            Items = loans,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return Ok(result);
    }

    /// <summary>
    /// Retrieves a specific loan by ID
    /// </summary>
    /// <param name="id">The loan ID</param>
    /// <returns>The loan details</returns>
    /// <response code="200">Returns the loan</response>
    /// <response code="404">Loan not found</response>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Get loan by ID",
        Description = "Retrieves detailed information about a specific loan including book and borrower details"
    )]
    [SwaggerResponse(200, "Loan retrieved successfully", typeof(LoanDetailDto))]
    [SwaggerResponse(404, "Loan not found")]
    public async Task<ActionResult<LoanDetailDto>> GetLoan(int id)
    {
        var loan = await _context.Loans
            .Include(l => l.Book)
            .ThenInclude(b => b.Author)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (loan == null)
        {
            return NotFound($"Loan with ID {id} not found.");
        }

        var loanDetail = new LoanDetailDto
        {
            Id = loan.Id,
            BorrowerName = loan.BorrowerName,
            BorrowerEmail = loan.BorrowerEmail,
            BorrowerPhone = loan.BorrowerPhone,
            LoanDate = loan.LoanDate,
            DueDate = loan.DueDate,
            ReturnDate = loan.ReturnDate,
            Status = loan.Status,
            Notes = loan.Notes,
            FineAmount = loan.FineAmount,
            FinePaid = loan.FinePaid,
            RenewalCount = loan.RenewalCount,
            MaxRenewals = loan.MaxRenewals,
            CreatedAt = loan.CreatedAt,
            UpdatedAt = loan.UpdatedAt,
            Book = new BookLoanInfoDto
            {
                Id = loan.Book.Id,
                Title = loan.Book.Title,
                ISBN = loan.Book.ISBN,
                AuthorName = loan.Book.Author.FullName,
                Genre = loan.Book.Genre,
                Publisher = loan.Book.Publisher
            },
            IsOverdue = loan.IsOverdue,
            DaysOverdue = loan.DaysOverdue,
            CanRenew = loan.CanRenew,
            LoanDurationDays = loan.LoanDurationDays
        };

        return Ok(loanDetail);
    }

    /// <summary>
    /// Creates a new loan (borrows a book)
    /// </summary>
    /// <param name="createLoanDto">The loan creation data</param>
    /// <returns>The created loan</returns>
    /// <response code="201">Loan created successfully</response>
    /// <response code="400">Invalid loan data or book not available</response>
    /// <response code="404">Book not found</response>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create a new loan",
        Description = "Creates a new loan by borrowing a book to a user. The book must be available."
    )]
    [SwaggerResponse(201, "Loan created successfully", typeof(LoanDto))]
    [SwaggerResponse(400, "Invalid loan data or book not available")]
    [SwaggerResponse(404, "Book not found")]
    public async Task<ActionResult<LoanDto>> CreateLoan([FromBody] CreateLoanDto createLoanDto)
    {
        var book = await _context.Books
            .Include(b => b.Author)
            .FirstOrDefaultAsync(b => b.Id == createLoanDto.BookId);

        if (book == null)
        {
            return NotFound($"Book with ID {createLoanDto.BookId} not found.");
        }

        if (!book.IsAvailable)
        {
            return BadRequest("Book is not available for loan.");
        }

        // Calculate due date (default 14 days from now)
        var dueDate = createLoanDto.DueDate ?? DateTime.UtcNow.AddDays(14);

        var loan = new Loan
        {
            BorrowerName = createLoanDto.BorrowerName,
            BorrowerEmail = createLoanDto.BorrowerEmail,
            BorrowerPhone = createLoanDto.BorrowerPhone,
            LoanDate = DateTime.UtcNow,
            DueDate = dueDate,
            Status = LoanStatus.Active,
            Notes = createLoanDto.Notes,
            BookId = createLoanDto.BookId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Decrease available copies
        book.AvailableCopies--;
        book.UpdatedAt = DateTime.UtcNow;

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();

        var loanDto = new LoanDto
        {
            Id = loan.Id,
            BorrowerName = loan.BorrowerName,
            BorrowerEmail = loan.BorrowerEmail,
            BorrowerPhone = loan.BorrowerPhone,
            LoanDate = loan.LoanDate,
            DueDate = loan.DueDate,
            ReturnDate = loan.ReturnDate,
            Status = loan.Status,
            Notes = loan.Notes,
            FineAmount = loan.FineAmount,
            FinePaid = loan.FinePaid,
            RenewalCount = loan.RenewalCount,
            MaxRenewals = loan.MaxRenewals,
            BookId = loan.BookId,
            BookTitle = book.Title,
            BookISBN = book.ISBN,
            AuthorName = book.Author.FullName,
            IsOverdue = loan.IsOverdue,
            DaysOverdue = loan.DaysOverdue,
            CanRenew = loan.CanRenew,
            LoanDurationDays = loan.LoanDurationDays
        };

        return CreatedAtAction(nameof(GetLoan), new { id = loan.Id }, loanDto);
    }

    /// <summary>
    /// Returns a borrowed book
    /// </summary>
    /// <param name="id">The loan ID</param>
    /// <param name="returnLoanDto">The return loan data</param>
    /// <returns>The updated loan</returns>
    /// <response code="200">Book returned successfully</response>
    /// <response code="400">Invalid operation or loan already returned</response>
    /// <response code="404">Loan not found</response>
    [HttpPost("{id}/return")]
    [SwaggerOperation(
        Summary = "Return a borrowed book",
        Description = "Marks a loan as returned and updates the book availability. Calculates fines for overdue returns."
    )]
    [SwaggerResponse(200, "Book returned successfully", typeof(LoanDto))]
    [SwaggerResponse(400, "Invalid operation or loan already returned")]
    [SwaggerResponse(404, "Loan not found")]
    public async Task<ActionResult<LoanDto>> ReturnBook(int id, [FromBody] ReturnLoanDto returnLoanDto)
    {
        var loan = await _context.Loans
            .Include(l => l.Book)
            .ThenInclude(b => b.Author)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (loan == null)
        {
            return NotFound($"Loan with ID {id} not found.");
        }

        if (loan.Status != LoanStatus.Active)
        {
            return BadRequest("Loan is not active and cannot be returned.");
        }

        var returnDate = returnLoanDto.ReturnDate ?? DateTime.UtcNow;
        var isLate = returnDate > loan.DueDate;

        // Update loan
        loan.ReturnDate = returnDate;
        loan.Status = isLate ? LoanStatus.ReturnedLate : LoanStatus.Returned;
        loan.Notes = string.IsNullOrEmpty(returnLoanDto.Notes) ? loan.Notes : returnLoanDto.Notes;
        loan.UpdatedAt = DateTime.UtcNow;

        // Calculate fine for late return (e.g., $1 per day)
        if (isLate)
        {
            var daysLate = (returnDate - loan.DueDate).Days;
            loan.FineAmount = daysLate * 1.00m; // $1 per day fine
        }

        // Increase available copies
        loan.Book.AvailableCopies++;
        loan.Book.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var loanDto = new LoanDto
        {
            Id = loan.Id,
            BorrowerName = loan.BorrowerName,
            BorrowerEmail = loan.BorrowerEmail,
            BorrowerPhone = loan.BorrowerPhone,
            LoanDate = loan.LoanDate,
            DueDate = loan.DueDate,
            ReturnDate = loan.ReturnDate,
            Status = loan.Status,
            Notes = loan.Notes,
            FineAmount = loan.FineAmount,
            FinePaid = loan.FinePaid,
            RenewalCount = loan.RenewalCount,
            MaxRenewals = loan.MaxRenewals,
            BookId = loan.BookId,
            BookTitle = loan.Book.Title,
            BookISBN = loan.Book.ISBN,
            AuthorName = loan.Book.Author.FullName,
            IsOverdue = loan.IsOverdue,
            DaysOverdue = loan.DaysOverdue,
            CanRenew = loan.CanRenew,
            LoanDurationDays = loan.LoanDurationDays
        };

        return Ok(loanDto);
    }

    /// <summary>
    /// Renews a loan (extends the due date)
    /// </summary>
    /// <param name="id">The loan ID</param>
    /// <param name="renewLoanDto">The renewal data</param>
    /// <returns>The updated loan</returns>
    /// <response code="200">Loan renewed successfully</response>
    /// <response code="400">Loan cannot be renewed</response>
    /// <response code="404">Loan not found</response>
    [HttpPost("{id}/renew")]
    [SwaggerOperation(
        Summary = "Renew a loan",
        Description = "Extends the due date of an active loan. Loans can only be renewed if they are not overdue and haven't exceeded the maximum renewal count."
    )]
    [SwaggerResponse(200, "Loan renewed successfully", typeof(LoanDto))]
    [SwaggerResponse(400, "Loan cannot be renewed")]
    [SwaggerResponse(404, "Loan not found")]
    public async Task<ActionResult<LoanDto>> RenewLoan(int id, [FromBody] RenewLoanDto renewLoanDto)
    {
        var loan = await _context.Loans
            .Include(l => l.Book)
            .ThenInclude(b => b.Author)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (loan == null)
        {
            return NotFound($"Loan with ID {id} not found.");
        }

        if (!loan.CanRenew)
        {
            var reason = loan.Status != LoanStatus.Active ? "Loan is not active" :
                        loan.IsOverdue ? "Loan is overdue" :
                        "Maximum renewals exceeded";
            return BadRequest($"Loan cannot be renewed: {reason}");
        }

        // Extend due date (default 14 days from current due date)
        var extensionDays = renewLoanDto.ExtensionDays ?? 14;
        loan.DueDate = loan.DueDate.AddDays(extensionDays);
        loan.RenewalCount++;
        loan.Status = LoanStatus.Renewed;
        loan.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var loanDto = new LoanDto
        {
            Id = loan.Id,
            BorrowerName = loan.BorrowerName,
            BorrowerEmail = loan.BorrowerEmail,
            BorrowerPhone = loan.BorrowerPhone,
            LoanDate = loan.LoanDate,
            DueDate = loan.DueDate,
            ReturnDate = loan.ReturnDate,
            Status = loan.Status,
            Notes = loan.Notes,
            FineAmount = loan.FineAmount,
            FinePaid = loan.FinePaid,
            RenewalCount = loan.RenewalCount,
            MaxRenewals = loan.MaxRenewals,
            BookId = loan.BookId,
            BookTitle = loan.Book.Title,
            BookISBN = loan.Book.ISBN,
            AuthorName = loan.Book.Author.FullName,
            IsOverdue = loan.IsOverdue,
            DaysOverdue = loan.DaysOverdue,
            CanRenew = loan.CanRenew,
            LoanDurationDays = loan.LoanDurationDays
        };

        return Ok(loanDto);
    }

    /// <summary>
    /// Pays the fine for a loan
    /// </summary>
    /// <param name="id">The loan ID</param>
    /// <returns>The updated loan</returns>
    /// <response code="200">Fine paid successfully</response>
    /// <response code="400">No fine to pay or fine already paid</response>
    /// <response code="404">Loan not found</response>
    [HttpPost("{id}/pay-fine")]
    [SwaggerOperation(
        Summary = "Pay fine for a loan",
        Description = "Marks the fine for a loan as paid"
    )]
    [SwaggerResponse(200, "Fine paid successfully", typeof(LoanDto))]
    [SwaggerResponse(400, "No fine to pay or fine already paid")]
    [SwaggerResponse(404, "Loan not found")]
    public async Task<ActionResult<LoanDto>> PayFine(int id)
    {
        var loan = await _context.Loans
            .Include(l => l.Book)
            .ThenInclude(b => b.Author)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (loan == null)
        {
            return NotFound($"Loan with ID {id} not found.");
        }

        if (loan.FineAmount <= 0)
        {
            return BadRequest("No fine to pay for this loan.");
        }

        if (loan.FinePaid)
        {
            return BadRequest("Fine has already been paid for this loan.");
        }

        loan.FinePaid = true;
        loan.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var loanDto = new LoanDto
        {
            Id = loan.Id,
            BorrowerName = loan.BorrowerName,
            BorrowerEmail = loan.BorrowerEmail,
            BorrowerPhone = loan.BorrowerPhone,
            LoanDate = loan.LoanDate,
            DueDate = loan.DueDate,
            ReturnDate = loan.ReturnDate,
            Status = loan.Status,
            Notes = loan.Notes,
            FineAmount = loan.FineAmount,
            FinePaid = loan.FinePaid,
            RenewalCount = loan.RenewalCount,
            MaxRenewals = loan.MaxRenewals,
            BookId = loan.BookId,
            BookTitle = loan.Book.Title,
            BookISBN = loan.Book.ISBN,
            AuthorName = loan.Book.Author.FullName,
            IsOverdue = loan.IsOverdue,
            DaysOverdue = loan.DaysOverdue,
            CanRenew = loan.CanRenew,
            LoanDurationDays = loan.LoanDurationDays
        };

        return Ok(loanDto);
    }

    /// <summary>
    /// Gets overdue loans
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10, max: 100)</param>
    /// <returns>A paginated list of overdue loans</returns>
    /// <response code="200">Returns the list of overdue loans</response>
    [HttpGet("overdue")]
    [SwaggerOperation(
        Summary = "Get overdue loans",
        Description = "Retrieves a paginated list of loans that are currently overdue"
    )]
    [SwaggerResponse(200, "Overdue loans retrieved successfully", typeof(PagedResult<LoanDto>))]
    public async Task<ActionResult<PagedResult<LoanDto>>> GetOverdueLoans(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 10)
    {
        var query = _context.Loans
            .Include(l => l.Book)
            .ThenInclude(b => b.Author)
            .Where(l => l.Status == LoanStatus.Active && l.DueDate < DateTime.UtcNow);

        var totalCount = await query.CountAsync();
        var loans = await query
            .OrderBy(l => l.DueDate) // Most overdue first
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LoanDto
            {
                Id = l.Id,
                BorrowerName = l.BorrowerName,
                BorrowerEmail = l.BorrowerEmail,
                BorrowerPhone = l.BorrowerPhone,
                LoanDate = l.LoanDate,
                DueDate = l.DueDate,
                ReturnDate = l.ReturnDate,
                Status = l.Status,
                Notes = l.Notes,
                FineAmount = l.FineAmount,
                FinePaid = l.FinePaid,
                RenewalCount = l.RenewalCount,
                MaxRenewals = l.MaxRenewals,
                BookId = l.BookId,
                BookTitle = l.Book.Title,
                BookISBN = l.Book.ISBN,
                AuthorName = l.Book.Author.FullName,
                IsOverdue = l.IsOverdue,
                DaysOverdue = l.DaysOverdue,
                CanRenew = l.CanRenew,
                LoanDurationDays = l.LoanDurationDays
            })
            .ToListAsync();

        var result = new PagedResult<LoanDto>
        {
            Items = loans,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return Ok(result);
    }
}