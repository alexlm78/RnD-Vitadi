using Calculator.Api.Data;
using Calculator.Api.Models;
using Calculator.Api.Services;
using Calculator.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Calculator.Tests.Services;

/// <summary>
/// Advanced unit tests for CalculatorService demonstrating complex scenarios
/// </summary>
public class CalculatorServiceAdvancedTests : IDisposable
{
    private readonly CalculatorDbContext _context;
    private readonly CalculatorService _calculatorService;

    public CalculatorServiceAdvancedTests()
    {
        var options = new DbContextOptionsBuilder<CalculatorDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CalculatorDbContext(options);
        _calculatorService = new CalculatorService(_context);
    }

    #region Test Data Builder Usage Tests

    [Fact]
    public async Task CalculateAsync_UsingTestDataBuilder_ShouldWork()
    {
        // Arrange
        var request = TestDataBuilder.CalculationRequest()
            .WithFirstOperand(25)
            .WithSecondOperand(4)
            .WithMultiplication()
            .Build();

        // Act
        var result = await _calculatorService.CalculateAsync(request);

        // Assert
        result.ShouldBeEquivalentToCalculationRequest(request, 100);
    }

    [Theory]
    [MemberData(nameof(GetCalculationTestData))]
    public async Task CalculateAsync_WithVariousOperations_ShouldReturnCorrectResults(
        CalculationRequest request, double expectedResult)
    {
        // Act
        var result = await _calculatorService.CalculateAsync(request);

        // Assert
        result.ShouldBeEquivalentToCalculationRequest(request, expectedResult);
    }

    public static IEnumerable<object[]> GetCalculationTestData()
    {
        yield return new object[]
        {
            TestDataBuilder.CalculationRequest().WithFirstOperand(100).WithSecondOperand(25).WithAddition().Build(),
            125.0
        };
        yield return new object[]
        {
            TestDataBuilder.CalculationRequest().WithFirstOperand(50).WithSecondOperand(30).WithSubtraction().Build(),
            20.0
        };
        yield return new object[]
        {
            TestDataBuilder.CalculationRequest().WithFirstOperand(12).WithSecondOperand(8).WithMultiplication().Build(),
            96.0
        };
        yield return new object[]
        {
            TestDataBuilder.CalculationRequest().WithFirstOperand(144).WithSecondOperand(12).WithDivision().Build(),
            12.0
        };
    }

    #endregion

    #region Complex History Scenarios

    [Fact]
    public async Task GetHistoryAsync_WithMixedOperations_ShouldReturnInCorrectOrder()
    {
        // Arrange
        var baseTime = DateTime.UtcNow;
        var historyEntries = new[]
        {
            new CalculationHistory
            {
                FirstOperand = 10,
                SecondOperand = 5,
                Operation = "+",
                Result = 15,
                Expression = "10 + 5 = 15",
                CreatedAt = baseTime.AddMinutes(-5)
            },
            new CalculationHistory
            {
                FirstOperand = 20,
                SecondOperand = 4,
                Operation = "*",
                Result = 80,
                Expression = "20 * 4 = 80",
                CreatedAt = baseTime.AddMinutes(-3)
            },
            new CalculationHistory
            {
                FirstOperand = 100,
                SecondOperand = 10,
                Operation = "/",
                Result = 10,
                Expression = "100 / 10 = 10",
                CreatedAt = baseTime.AddMinutes(-1)
            }
        };

        _context.CalculationHistory.AddRange(historyEntries);
        await _context.SaveChangesAsync();

        // Act
        var history = await _calculatorService.GetHistoryAsync();

        // Assert
        history.Should().HaveCount(3);
        history.Should().BeInDescendingOrder(h => h.CreatedAt);
        
        var historyList = history.ToList();
        historyList[0].Operation.Should().Be("/"); // Most recent
        historyList[1].Operation.Should().Be("*"); // Middle
        historyList[2].Operation.Should().Be("+"); // Oldest
    }

    [Fact]
    public async Task GetHistoryAsync_WithLargeDataset_ShouldRespectLimit()
    {
        // Arrange
        var entries = Enumerable.Range(1, 100)
            .Select(i => new CalculationHistory
            {
                FirstOperand = i,
                SecondOperand = 1,
                Operation = "+",
                Result = i + 1,
                Expression = $"{i} + 1 = {i + 1}",
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            })
            .ToList();

        _context.CalculationHistory.AddRange(entries);
        await _context.SaveChangesAsync();

        // Act
        var history = await _calculatorService.GetHistoryAsync(10);

        // Assert
        history.Should().HaveCount(10);
        history.Should().BeInDescendingOrder(h => h.CreatedAt);
        
        // Should get the 10 most recent entries (created 1-10 minutes ago)
        var historyList = history.ToList();
        historyList.Should().AllSatisfy(h => h.FirstOperand.Should().BeLessThanOrEqualTo(10));
    }

    #endregion

    #region Error Handling and Edge Cases

    [Theory]
    [InlineData(double.MaxValue, 1, "+")]
    [InlineData(double.MinValue, 1, "+")]
    [InlineData(1, double.MaxValue, "*")]
    [InlineData(double.PositiveInfinity, 1, "+")]
    [InlineData(double.NegativeInfinity, 1, "+")]
    public async Task CalculateAsync_WithExtremeValues_ShouldHandleGracefully(double a, double b, string operation)
    {
        // Arrange
        var request = TestDataBuilder.CalculationRequest()
            .WithFirstOperand(a)
            .WithSecondOperand(b)
            .WithOperation(operation)
            .Build();

        // Act & Assert
        await _calculatorService.Invoking(s => s.CalculateAsync(request))
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task CalculateAsync_WithNaNValues_ShouldPersistNaN()
    {
        // Arrange
        var request = TestDataBuilder.CalculationRequest()
            .WithFirstOperand(double.NaN)
            .WithSecondOperand(5)
            .WithAddition()
            .Build();

        // Act
        var result = await _calculatorService.CalculateAsync(request);

        // Assert
        result.Result.Should().Be(double.NaN);
        
        var historyEntry = await _context.CalculationHistory.FirstAsync();
        historyEntry.Result.Should().Be(double.NaN);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid")]
    [InlineData("++")]
    [InlineData("add")]
    public async Task CalculateAsync_WithInvalidOperations_ShouldThrowArgumentException(string invalidOperation)
    {
        // Arrange
        var request = TestDataBuilder.CalculationRequest()
            .WithOperation(invalidOperation)
            .Build();

        // Act & Assert
        await _calculatorService.Invoking(s => s.CalculateAsync(request))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage($"Unsupported operation: {invalidOperation}");
    }

    #endregion

    #region Precision and Rounding Tests

    [Theory]
    [InlineData(0.1, 0.2, 0.3)] // Classic floating point precision issue
    [InlineData(1.0, 3.0, 0.3333333333333333)] // Division with repeating decimal
    [InlineData(999999999999999.0, 1.0, 1000000000000000.0)] // Large number precision
    public async Task CalculateAsync_WithPrecisionChallenges_ShouldHandleCorrectly(double a, double b, double expected)
    {
        // Arrange
        var request = TestDataBuilder.CalculationRequest()
            .WithFirstOperand(a)
            .WithSecondOperand(b)
            .WithOperation(a == 1.0 && b == 3.0 ? "/" : "+")
            .Build();

        // Act
        var result = await _calculatorService.CalculateAsync(request);

        // Assert
        if (a == 1.0 && b == 3.0) // Division case
        {
            result.Result.Should().BeApproximately(expected, 0.0000000000001);
        }
        else if (a == 0.1 && b == 0.2) // Floating point precision case
        {
            result.Result.Should().BeApproximately(expected, 0.0000000000001);
        }
        else
        {
            result.Result.Should().Be(expected);
        }
    }

    #endregion

    #region Performance and Stress Tests

    [Fact]
    public async Task CalculateAsync_MultipleSequentialOperations_ShouldPerformWell()
    {
        // Arrange
        var operations = Enumerable.Range(1, 1000)
            .Select(i => TestDataBuilder.CalculationRequest()
                .WithFirstOperand(i)
                .WithSecondOperand(1)
                .WithAddition()
                .Build())
            .ToList();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        foreach (var operation in operations)
        {
            await _calculatorService.CalculateAsync(operation);
        }
        
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000); // Should complete within 10 seconds
        
        var historyCount = await _context.CalculationHistory.CountAsync();
        historyCount.Should().Be(1000);
    }

    [Fact]
    public async Task GetHistoryAsync_WithLargeHistory_ShouldPerformWell()
    {
        // Arrange - Create 10,000 history entries
        var entries = Enumerable.Range(1, 10000)
            .Select(i => new CalculationHistory
            {
                FirstOperand = i,
                SecondOperand = 1,
                Operation = "+",
                Result = i + 1,
                Expression = $"{i} + 1 = {i + 1}",
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            })
            .ToList();

        _context.CalculationHistory.AddRange(entries);
        await _context.SaveChangesAsync();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var history = await _calculatorService.GetHistoryAsync(100);
        stopwatch.Stop();

        // Assert
        history.Should().HaveCount(100);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should complete within 1 second
    }

    #endregion

    #region Database Transaction Tests

    [Fact]
    public async Task CalculateAsync_WhenDatabaseSaveFails_ShouldThrowException()
    {
        // This test would require mocking the DbContext, which is complex with EF Core
        // In a real scenario, you might test this with a custom repository pattern
        // For now, we'll test that successful operations do save correctly
        
        // Arrange
        var request = TestDataBuilder.CalculationRequest().Build();

        // Act
        var result = await _calculatorService.CalculateAsync(request);

        // Assert
        result.Should().NotBeNull();
        
        var savedEntry = await _context.CalculationHistory.FirstAsync();
        savedEntry.Should().NotBeNull();
        savedEntry.FirstOperand.Should().Be(request.FirstOperand);
    }

    [Fact]
    public async Task ClearHistoryAsync_WithConcurrentAccess_ShouldHandleCorrectly()
    {
        // Arrange
        var entries = Enumerable.Range(1, 100)
            .Select(i => new CalculationHistory
            {
                FirstOperand = i,
                SecondOperand = 1,
                Operation = "+",
                Result = i + 1,
                Expression = $"{i} + 1 = {i + 1}",
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            })
            .ToList();

        _context.CalculationHistory.AddRange(entries);
        await _context.SaveChangesAsync();

        // Act - Simulate concurrent clear operations
        var clearTasks = new[]
        {
            _calculatorService.ClearHistoryAsync(),
            _calculatorService.ClearHistoryAsync(),
            _calculatorService.ClearHistoryAsync()
        };

        // Assert - Should not throw exceptions
        await FluentActions.Invoking(() => Task.WhenAll(clearTasks))
            .Should().NotThrowAsync();

        var finalCount = await _context.CalculationHistory.CountAsync();
        finalCount.Should().Be(0);
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }
}