using Microsoft.EntityFrameworkCore;

namespace HealthDashboard.Api.Data;

/// <summary>
/// Simple DbContext for health check demonstrations
/// This context is used to verify database connectivity in health checks
/// </summary>
public class HealthDashboardDbContext : DbContext
{
    public HealthDashboardDbContext(DbContextOptions<HealthDashboardDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Simple entity to test database operations
    /// </summary>
    public DbSet<HealthCheckEntity> HealthCheckEntities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the HealthCheckEntity
        modelBuilder.Entity<HealthCheckEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastChecked).IsRequired();
        });

        // Seed some initial data for testing
        modelBuilder.Entity<HealthCheckEntity>().HasData(
            new HealthCheckEntity
            {
                Id = 1,
                Name = "Database Connection Test",
                Status = "Healthy",
                LastChecked = DateTime.UtcNow
            }
        );
    }
}

/// <summary>
/// Simple entity for health check testing
/// </summary>
public class HealthCheckEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime LastChecked { get; set; }
}