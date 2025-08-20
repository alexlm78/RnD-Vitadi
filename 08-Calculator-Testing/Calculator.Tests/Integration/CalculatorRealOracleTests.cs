using Calculator.Api.Data;
using Calculator.Api.Models;
using Calculator.Api.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.Oracle;

namespace Calculator.Tests.Integration;

/// <summary>
/// Example of real Oracle integration tests using Testcontainers
/// These tests are commented out to avoid Docker dependency in CI/CD
/// Uncomment and configure for local development with Docker
/// </summary>
public class CalculatorRealOracleTests // : IAsyncLifetime
{
    // Uncomment for real Oracle testing
    /*
    private readonly OracleContainer _oracleContainer;
    private CalculatorDbContext _context = null!;
    private CalculatorService _calculatorService = null!;

    public CalculatorRealOracleTests()
    {
        _oracleContainer = new OracleBuilder()
            .WithImage("gvenzl/oracle-xe:21-slim") // Use a lightweight Oracle image
            .WithUsername("testuser")
            .WithPassword("testpass123")
            .WithEnvironment("ORACLE_PASSWORD", "testpass123")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1521))
            .Build();
    }

    public async Task InitializeAsync()
    {
        // Start the Oracle container
        await _oracleContainer.StartAsync();

        // Create DbContext with Oracle connection
        var connectionString = _oracleContainer.GetConnectionString();
        var options = new DbContextOptionsBuilder<CalculatorDbContext>()
            .UseOracle(connectionString)
            .Options;

        _context = new CalculatorDbContext(options);
        
        // Ensure database is created and migrated
        await _context.Database.EnsureCreatedAsync();
        
        _calculatorService = new CalculatorService(_context);
    }

    public async Task DisposeAsync()
    {
        if (_context != null)
        {
            await _context.DisposeAsync();
        }
        
        await _oracleContainer.DisposeAsync();
    }

    [Fact]
    public async Task CalculateAsync_WithRealOracle_ShouldPersistCorrectly()
    {
        // Arrange
        var request = new CalculationRequest
        {
            FirstOperand = 42,
            SecondOperand = 8,
            Operation = "*"
        };

        // Act
        var result = await _calculatorService.CalculateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().Be(336);

        // Verify persistence in real Oracle database
        var persistedEntry = await _context.CalculationHistory
            .FirstOrDefaultAsync(h => h.FirstOperand == 42 && h.SecondOperand == 8);

        persistedEntry.Should().NotBeNull();
        persistedEntry!.Result.Should().Be(336);
        persistedEntry.Expression.Should().Be("42 * 8 = 336");
    }

    [Fact]
    public async Task ConcurrentOperations_WithRealOracle_ShouldHandleCorrectly()
    {
        // Arrange
        var operations = Enumerable.Range(1, 20)
            .Select(i => new CalculationRequest 
            { 
                FirstOperand = i, 
                SecondOperand = 2, 
                Operation = "*" 
            })
            .ToList();

        // Act
        var tasks = operations.Select(op => _calculatorService.CalculateAsync(op));
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(20);
        results.Should().OnlyContain(r => r.Operation == "*");

        var historyCount = await _context.CalculationHistory.CountAsync();
        historyCount.Should().Be(20);
    }
    */

    // Placeholder test to keep the class valid
    [Fact]
    public void TestcontainersOracle_ConfigurationExample()
    {
        // This test demonstrates how to configure Testcontainers for Oracle
        // Uncomment the class implementation above for real Oracle testing
        
        var expectedConfiguration = new
        {
            Image = "gvenzl/oracle-xe:21-slim",
            Username = "testuser",
            Password = "testpass123",
            Port = 1521
        };

        expectedConfiguration.Should().NotBeNull();
        expectedConfiguration.Image.Should().Contain("oracle");
        expectedConfiguration.Port.Should().Be(1521);
    }
}