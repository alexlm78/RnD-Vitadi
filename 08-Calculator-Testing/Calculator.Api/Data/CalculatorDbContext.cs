using Calculator.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Calculator.Api.Data;

/// <summary>
/// Database context for the Calculator application
/// </summary>
public class CalculatorDbContext : DbContext
{
    public CalculatorDbContext(DbContextOptions<CalculatorDbContext> options) : base(options)
    {
    }

    public DbSet<CalculationHistory> CalculationHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CalculationHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Operation).IsRequired().HasMaxLength(1);
            entity.Property(e => e.Expression).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
        });
    }
}