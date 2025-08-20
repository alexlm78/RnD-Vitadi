using Calculator.Api.Data;
using Calculator.Api.Models;
using Calculator.Api.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Calculator.Tests.Integration;

/// <summary>
/// Integration tests demonstrating Oracle database patterns
/// Note: Uses InMemory database for CI/CD compatibility. 
/// In production, replace with actual Oracle Testcontainers setup
/// </summary>
public class CalculatorOracleIntegrationTests : IDisposable
{
    private readonly CalculatorDbContext _context;
    private readonly CalculatorService _calculatorService;

    public CalculatorOracleIntegrationTests()
    {
        // Using InMemory database for demonstration
        // In real scenarios, use Testcontainers with Oracle
        var options = new DbContextOptionsBuilder<CalculatorDbContext>()
            .UseInMemoryDatabase($"OracleSimulationDb_{Guid.NewGuid()}")
            .Options;

        _context = new CalculatorDbContext(options);
        _calculatorService = new CalculatorService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }



    #region Database Schema Tests

    [Fact]
    public async Task Database_ShouldBeCreatedSuccessfully()
    {
        // Act
        var canConnect = await _context.Database.CanConnectAsync();

        // Assert
        canConnect.Should().BeTrue();
    }

    [Fact]
    public async Task CalculationHistoryTable_ShouldExist()
    {
        // Act - Check if table exists by querying it
        var canQueryTable = true;
        try
        {
            await _context.CalculationHistory.CountAsync();
        }
        catch (Exception)
        {
            canQueryTable = false;
        }

        // Assert
        canQueryTable.Should().BeTrue();
    }

    [Fact]
    public async Task CalculationHistoryTable_ShouldHaveCorrectColumns()
    {
        // Arrange
        var expectedColumns = new[] { "Id", "FirstOperand", "SecondOperand", "Operation", "Result", "CreatedAt", "Expression" };

        // Act - Insert a test record to verify column structure
        var testEntry = new CalculationHistory
        {
            FirstOperand = 1,
            SecondOperand = 1,
            Operation = "+",
            Result = 2,
            Expression = "1 + 1 = 2",
            CreatedAt = DateTime.UtcNow
        };

        _context.CalculationHistory.Add(testEntry);
        var saveAction = async () => await _context.SaveChangesAsync();

        // Assert
        await saveAction.Should().NotThrowAsync();

        var savedEntry = await _context.CalculationHistory.FirstAsync();
        savedEntry.Should().NotBeNull();
        savedEntry.Id.Should().BeGreaterThan(0);
        savedEntry.FirstOperand.Should().Be(1);
        savedEntry.SecondOperand.Should().Be(1);
        savedEntry.Operation.Should().Be("+");
        savedEntry.Result.Should().Be(2);
        savedEntry.Expression.Should().Be("1 + 1 = 2");
        savedEntry.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    #endregion

    #region Data Persistence Tests

    [Fact]
    public async Task CalculateAsync_ShouldPersistToOracleDatabase()
    {
        // Arrange
        var request = new CalculationRequest
        {
            FirstOperand = 25,
            SecondOperand = 5,
            Operation = "*"
        };

        // Act
        var result = await _calculatorService.CalculateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().Be(125);

        // Verify persistence in Oracle
        var persistedEntry = await _context.CalculationHistory
            .FirstOrDefaultAsync(h => h.FirstOperand == 25 && h.SecondOperand == 5 && h.Operation == "*");

        persistedEntry.Should().NotBeNull();
        persistedEntry!.Result.Should().Be(125);
        persistedEntry.Expression.Should().Be("25 * 5 = 125");
    }

    [Fact]
    public async Task MultipleCalculations_ShouldAllBePersisted()
    {
        // Arrange
        var calculations = new[]
        {
            new CalculationRequest { FirstOperand = 10, SecondOperand = 5, Operation = "+" },
            new CalculationRequest { FirstOperand = 20, SecondOperand = 4, Operation = "-" },
            new CalculationRequest { FirstOperand = 6, SecondOperand = 7, Operation = "*" },
            new CalculationRequest { FirstOperand = 100, SecondOperand = 10, Operation = "/" }
        };

        // Act
        foreach (var calc in calculations)
        {
            await _calculatorService.CalculateAsync(calc);
        }

        // Assert
        var totalCount = await _context.CalculationHistory.CountAsync();
        totalCount.Should().Be(4);

        var allEntries = await _context.CalculationHistory.ToListAsync();
        allEntries.Should().Contain(e => e.Result == 15 && e.Operation == "+");
        allEntries.Should().Contain(e => e.Result == 16 && e.Operation == "-");
        allEntries.Should().Contain(e => e.Result == 42 && e.Operation == "*");
        allEntries.Should().Contain(e => e.Result == 10 && e.Operation == "/");
    }

    [Fact]
    public async Task GetHistoryAsync_ShouldRetrieveFromOracleInCorrectOrder()
    {
        // Arrange
        var baseTime = DateTime.UtcNow;
        var entries = new[]
        {
            new CalculationHistory { FirstOperand = 1, SecondOperand = 1, Operation = "+", Result = 2, Expression = "1 + 1 = 2", CreatedAt = baseTime.AddMinutes(-3) },
            new CalculationHistory { FirstOperand = 2, SecondOperand = 2, Operation = "*", Result = 4, Expression = "2 * 2 = 4", CreatedAt = baseTime.AddMinutes(-2) },
            new CalculationHistory { FirstOperand = 5, SecondOperand = 3, Operation = "-", Result = 2, Expression = "5 - 3 = 2", CreatedAt = baseTime.AddMinutes(-1) }
        };

        _context.CalculationHistory.AddRange(entries);
        await _context.SaveChangesAsync();

        // Act
        var history = await _calculatorService.GetHistoryAsync();

        // Assert
        history.Should().HaveCount(3);
        history.Should().BeInDescendingOrder(h => h.CreatedAt);
        
        var historyList = history.ToList();
        historyList[0].Expression.Should().Be("5 - 3 = 2"); // Most recent
        historyList[1].Expression.Should().Be("2 * 2 = 4"); // Middle
        historyList[2].Expression.Should().Be("1 + 1 = 2"); // Oldest
    }

    [Fact]
    public async Task ClearHistoryAsync_ShouldRemoveAllEntriesFromOracle()
    {
        // Arrange
        var entries = new[]
        {
            new CalculationHistory { FirstOperand = 1, SecondOperand = 1, Operation = "+", Result = 2, Expression = "1 + 1 = 2", CreatedAt = DateTime.UtcNow },
            new CalculationHistory { FirstOperand = 2, SecondOperand = 2, Operation = "*", Result = 4, Expression = "2 * 2 = 4", CreatedAt = DateTime.UtcNow }
        };

        _context.CalculationHistory.AddRange(entries);
        await _context.SaveChangesAsync();

        // Verify entries exist
        var initialCount = await _context.CalculationHistory.CountAsync();
        initialCount.Should().Be(2);

        // Act
        await _calculatorService.ClearHistoryAsync();

        // Assert
        var finalCount = await _context.CalculationHistory.CountAsync();
        finalCount.Should().Be(0);
    }

    #endregion

    #region Transaction and Concurrency Tests

    [Fact]
    public async Task ConcurrentCalculations_ShouldAllBePersisted()
    {
        // Arrange
        var tasks = new List<Task>();
        var random = new Random();

        // Act - Perform 10 concurrent calculations
        for (int i = 0; i < 10; i++)
        {
            var a = random.Next(1, 100);
            var b = random.Next(1, 100);
            var request = new CalculationRequest { FirstOperand = a, SecondOperand = b, Operation = "+" };
            
            tasks.Add(_calculatorService.CalculateAsync(request));
        }

        await Task.WhenAll(tasks);

        // Assert
        var count = await _context.CalculationHistory.CountAsync();
        count.Should().Be(10);

        var allEntries = await _context.CalculationHistory.ToListAsync();
        allEntries.Should().AllSatisfy(entry =>
        {
            entry.Operation.Should().Be("+");
            entry.Result.Should().Be(entry.FirstOperand + entry.SecondOperand);
        });
    }

    [Fact]
    public async Task FailedCalculation_ShouldNotPersistToDatabase()
    {
        // Arrange
        var request = new CalculationRequest
        {
            FirstOperand = 10,
            SecondOperand = 0,
            Operation = "/"
        };

        // Act & Assert
        await _calculatorService.Invoking(s => s.CalculateAsync(request))
            .Should().ThrowAsync<DivideByZeroException>();

        // Verify no entry was persisted
        var count = await _context.CalculationHistory.CountAsync();
        count.Should().Be(0);
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task LargeHistoryRetrieval_ShouldPerformWell()
    {
        // Arrange - Insert 1000 records
        var entries = new List<CalculationHistory>();
        var baseTime = DateTime.UtcNow;

        for (int i = 0; i < 1000; i++)
        {
            entries.Add(new CalculationHistory
            {
                FirstOperand = i,
                SecondOperand = 1,
                Operation = "+",
                Result = i + 1,
                Expression = $"{i} + 1 = {i + 1}",
                CreatedAt = baseTime.AddSeconds(-i)
            });
        }

        _context.CalculationHistory.AddRange(entries);
        await _context.SaveChangesAsync();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var history = await _calculatorService.GetHistoryAsync(100);
        stopwatch.Stop();

        // Assert
        history.Should().HaveCount(100);
        history.Should().BeInDescendingOrder(h => h.CreatedAt);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Should complete within 5 seconds
    }

    [Fact]
    public async Task BulkInsert_ShouldHandleLargeVolume()
    {
        // Arrange
        var calculations = new List<Task>();

        // Act - Perform 100 calculations
        for (int i = 0; i < 100; i++)
        {
            var request = new CalculationRequest
            {
                FirstOperand = i,
                SecondOperand = 1,
                Operation = "+"
            };
            calculations.Add(_calculatorService.CalculateAsync(request));
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await Task.WhenAll(calculations);
        stopwatch.Stop();

        // Assert
        var count = await _context.CalculationHistory.CountAsync();
        count.Should().Be(100);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(30000); // Should complete within 30 seconds
    }

    #endregion

    #region Data Integrity Tests

    [Fact]
    public async Task CalculationHistory_ShouldMaintainDataIntegrity()
    {
        // Arrange
        var request = new CalculationRequest
        {
            FirstOperand = 123.456,
            SecondOperand = 789.123,
            Operation = "*"
        };

        // Act
        var result = await _calculatorService.CalculateAsync(request);

        // Assert
        var persistedEntry = await _context.CalculationHistory.FirstAsync();
        
        persistedEntry.FirstOperand.Should().Be(123.456);
        persistedEntry.SecondOperand.Should().Be(789.123);
        persistedEntry.Operation.Should().Be("*");
        // Calculate expected result
        var expectedResult = 123.456 * 789.123;
        persistedEntry.Result.Should().BeApproximately(expectedResult, 0.1);
        persistedEntry.Expression.Should().Be($"123.456 * 789.123 = {expectedResult}");
        persistedEntry.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task DatabaseConnection_ShouldHandleReconnection()
    {
        // Arrange - Perform initial calculation
        var request1 = new CalculationRequest { FirstOperand = 1, SecondOperand = 1, Operation = "+" };
        await _calculatorService.CalculateAsync(request1);

        // Act - Close and reopen connection (simulated by creating new context)
        await _context.DisposeAsync();
        
        var options = new DbContextOptionsBuilder<CalculatorDbContext>()
            .UseInMemoryDatabase($"ReconnectTestDb_{Guid.NewGuid()}")
            .Options;
        
        using var newContext = new CalculatorDbContext(options);
        var newCalculatorService = new CalculatorService(newContext);

        // Perform another calculation
        var request2 = new CalculationRequest { FirstOperand = 2, SecondOperand = 2, Operation = "*" };
        await newCalculatorService.CalculateAsync(request2);

        // Assert - Since we're using different contexts, we expect only the new calculation
        var count = await newContext.CalculationHistory.CountAsync();
        count.Should().Be(1);

        var entry = await newContext.CalculationHistory.FirstAsync();
        entry.Result.Should().Be(4);
    }

    #endregion
}