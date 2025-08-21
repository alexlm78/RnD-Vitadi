using DigitalLibrary.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalLibrary.Api.Data;

/// <summary>
/// Entity Framework DbContext for the Digital Library system
/// </summary>
public class DigitalLibraryDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the DigitalLibraryDbContext
    /// </summary>
    /// <param name="options">Database context options</param>
    public DigitalLibraryDbContext(DbContextOptions<DigitalLibraryDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Authors in the digital library
    /// </summary>
    public DbSet<Author> Authors { get; set; }

    /// <summary>
    /// Books in the digital library
    /// </summary>
    public DbSet<Book> Books { get; set; }

    /// <summary>
    /// Book loans in the digital library
    /// </summary>
    public DbSet<Loan> Loans { get; set; }

    /// <summary>
    /// Configures the entity relationships and constraints
    /// </summary>
    /// <param name="modelBuilder">The model builder instance</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Author entity
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(a => a.LastName).IsRequired().HasMaxLength(100);
            entity.Property(a => a.Biography).HasMaxLength(2000);
            entity.Property(a => a.Nationality).HasMaxLength(100);
            entity.Property(a => a.Email).HasMaxLength(255);
            entity.HasIndex(a => a.Email).IsUnique();
            
            // Configure relationship with Books
            entity.HasMany(a => a.Books)
                  .WithOne(b => b.Author)
                  .HasForeignKey(b => b.AuthorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Book entity
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Title).IsRequired().HasMaxLength(500);
            entity.Property(b => b.Description).HasMaxLength(2000);
            entity.Property(b => b.ISBN).HasMaxLength(20);
            entity.Property(b => b.Publisher).HasMaxLength(200);
            entity.Property(b => b.Genre).HasMaxLength(100);
            entity.Property(b => b.Language).HasMaxLength(50);
            entity.Property(b => b.AverageRating).HasPrecision(3, 2);
            entity.HasIndex(b => b.ISBN).IsUnique();
            entity.HasIndex(b => b.Title);
            
            // Configure relationship with Loans
            entity.HasMany(b => b.Loans)
                  .WithOne(l => l.Book)
                  .HasForeignKey(l => l.BookId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Loan entity
        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.BorrowerName).IsRequired().HasMaxLength(200);
            entity.Property(l => l.BorrowerEmail).IsRequired().HasMaxLength(255);
            entity.Property(l => l.BorrowerPhone).HasMaxLength(20);
            entity.Property(l => l.Notes).HasMaxLength(1000);
            entity.Property(l => l.FineAmount).HasPrecision(10, 2);
            entity.Property(l => l.Status).HasConversion<int>();
            entity.HasIndex(l => l.BorrowerEmail);
            entity.HasIndex(l => l.Status);
            entity.HasIndex(l => l.DueDate);
        });

        // Seed data
        SeedData(modelBuilder);
    }

    /// <summary>
    /// Seeds initial data for development and testing
    /// </summary>
    /// <param name="modelBuilder">The model builder instance</param>
    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Authors
        modelBuilder.Entity<Author>().HasData(
            new Author
            {
                Id = 1,
                FirstName = "Gabriel",
                LastName = "García Márquez",
                Biography = "Colombian novelist, short-story writer, screenwriter, and journalist, known affectionately as Gabo throughout Latin America.",
                BirthDate = new DateTime(1927, 3, 6),
                Nationality = "Colombian",
                Email = "gabo@example.com",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Author
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Austen",
                Biography = "English novelist known primarily for her six major novels, which interpret, critique and comment upon the British landed gentry at the end of the 18th century.",
                BirthDate = new DateTime(1775, 12, 16),
                Nationality = "British",
                Email = "jane.austen@example.com",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Author
            {
                Id = 3,
                FirstName = "George",
                LastName = "Orwell",
                Biography = "English novelist, essayist, journalist and critic whose work is characterised by lucid prose, biting social criticism, opposition to totalitarianism.",
                BirthDate = new DateTime(1903, 6, 25),
                Nationality = "British",
                Email = "george.orwell@example.com",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );

        // Seed Books
        modelBuilder.Entity<Book>().HasData(
            new Book
            {
                Id = 1,
                Title = "One Hundred Years of Solitude",
                Description = "A landmark 1967 novel by Colombian author Gabriel García Márquez that tells the multi-generational story of the Buendía family.",
                ISBN = "978-0060883287",
                PublicationDate = new DateTime(1967, 5, 30),
                Publisher = "Harper & Row",
                PageCount = 417,
                Genre = "Magical Realism",
                Language = "Spanish",
                TotalCopies = 5,
                AvailableCopies = 3,
                AverageRating = 4.5m,
                RatingCount = 150,
                AuthorId = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Book
            {
                Id = 2,
                Title = "Pride and Prejudice",
                Description = "A romantic novel of manners written by Jane Austen in 1813.",
                ISBN = "978-0141439518",
                PublicationDate = new DateTime(1813, 1, 28),
                Publisher = "T. Egerton",
                PageCount = 432,
                Genre = "Romance",
                Language = "English",
                TotalCopies = 8,
                AvailableCopies = 6,
                AverageRating = 4.3m,
                RatingCount = 200,
                AuthorId = 2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Book
            {
                Id = 3,
                Title = "1984",
                Description = "A dystopian social science fiction novel and cautionary tale written by English writer George Orwell.",
                ISBN = "978-0452284234",
                PublicationDate = new DateTime(1949, 6, 8),
                Publisher = "Secker & Warburg",
                PageCount = 328,
                Genre = "Dystopian Fiction",
                Language = "English",
                TotalCopies = 10,
                AvailableCopies = 7,
                AverageRating = 4.7m,
                RatingCount = 300,
                AuthorId = 3,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );

        // Seed Loans
        modelBuilder.Entity<Loan>().HasData(
            new Loan
            {
                Id = 1,
                BorrowerName = "John Smith",
                BorrowerEmail = "john.smith@example.com",
                BorrowerPhone = "+1-555-0123",
                LoanDate = DateTime.UtcNow.AddDays(-10),
                DueDate = DateTime.UtcNow.AddDays(4),
                Status = LoanStatus.Active,
                Notes = "First-time borrower",
                BookId = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new Loan
            {
                Id = 2,
                BorrowerName = "Mary Johnson",
                BorrowerEmail = "mary.johnson@example.com",
                BorrowerPhone = "+1-555-0456",
                LoanDate = DateTime.UtcNow.AddDays(-25),
                DueDate = DateTime.UtcNow.AddDays(-11),
                ReturnDate = DateTime.UtcNow.AddDays(-5),
                Status = LoanStatus.ReturnedLate,
                FineAmount = 5.00m,
                FinePaid = true,
                Notes = "Returned late but fine paid",
                BookId = 2,
                CreatedAt = DateTime.UtcNow.AddDays(-25),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            }
        );
    }
}