using Calculator.Api.Data;
using Calculator.Api.Models;
using Calculator.Api.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Calculator.Tests.Services;

/// <summary>
/// Unit tests for CalculatorService
/// </summary>
public class CalculatorServiceTests : IDisposable
{
    private readonly CalculatorDbContext _context;
    private readonly CalculatorService _calculatorService;

    public CalculatorServiceTests()
    {
        // Setup in-memory database for testing
        var options = new DbContextOptionsBuilder<CalculatorDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CalculatorDbContext(options);
        _calculatorService = new CalculatorService(_context);
    }

    #region Basic Math Operations Tests

    [Theory]
    [InlineData(5, 3, 8)]
    [InlineData(-5, 3, -2)]
    [InlineData(0, 0, 0)]
    [InlineData(10.5, 2.3, 12.8)]
    [InlineData(double.MaxValue, 0, double.MaxValue)]
    public void Add_ShouldReturnCorrectSum(double a, double b, double expected)
    {
        // Act
        var result = _calculatorService.Add(a, b);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(5, 3, 2)]
    [InlineData(-5, 3, -8)]
    [InlineData(0, 0, 0)]
    [InlineData(10.5, 2.3, 8.2)]
    [InlineData(double.MaxValue, 0, double.MaxValue)]
    public void Subtract_ShouldReturnCorrectDifference(double a, double b, double expected)
    {
        // Act
        var result = _calculatorService.Subtract(a, b);

        // Assert
        result.Should().BeApproximately(expected, 0.0001);
    }

    [Theory]
    [InlineData(5, 3, 15)]
    [InlineData(-5, 3, -15)]
    [InlineData(0, 5, 0)]
    [InlineData(2.5, 4, 10)]
    [InlineData(-2.5, -4, 10)]
    public void Multiply_ShouldReturnCorrectProduct(double a, double b, double expected)
    {
        // Act
        var result = _calculatorService.Multiply(a, b);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(6, 3, 2)]
    [InlineData(-6, 3, -2)]
    [InlineData(0, 5, 0)]
    [InlineData(10, 4, 2.5)]
    [InlineData(-10, -4, 2.5)]
    public void Divide_ShouldReturnCorrectQuotient(double a, double b, double expected)
    {
        // Act
        var result = _calculatorService.Divide(a, b);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Divide_ByZero_ShouldThrowDivideByZeroException()
    {
        // Act & Assert
        _calculatorService.Invoking(s => s.Divide(10, 0))
            .Should().Throw<DivideByZeroException>()
            .WithMessage("Cannot divide by zero");
    }

    #endregion

    #region CalculateAsync Tests

    [Theory]
    [InlineData(5, 3, "+", 8)]
    [InlineData(5, 3, "-", 2)]
    [InlineData(5, 3, "*", 15)]
    [InlineData(6, 3, "/", 2)]
    public async Task CalculateAsync_ValidOperations_ShouldReturnCorrectResult(
        double firstOperand, double secondOperand, string operation, double expectedResult)
    {
        // Arrange
        var request = new CalculationRequest
        {
            FirstOperand = firstOperand,
            SecondOperand = secondOperand,
            Operation = operation
        };

        // Act
        var result = await _calculatorService.CalculateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.FirstOperand.Should().Be(firstOperand);
        result.SecondOperand.Should().Be(secondOperand);
        result.Operation.Should().Be(operation);
        result.Result.Should().Be(expectedResult);
        result.Expression.Should().Be($"{firstOperand} {operation} {secondOperand} = {expectedResult}");
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task CalculateAsync_ValidOperation_ShouldSaveToHistory()
    {
        // Arrange
        var request = new CalculationRequest
        {
            FirstOperand = 10,
            SecondOperand = 5,
            Operation = "+"
        };

        // Act
        await _calculatorService.CalculateAsync(request);

        // Assert
        var historyCount = await _context.CalculationHistory.CountAsync();
        historyCount.Should().Be(1);

        var historyEntry = await _context.CalculationHistory.FirstAsync();
        historyEntry.FirstOperand.Should().Be(10);
        historyEntry.SecondOperand.Should().Be(5);
        historyEntry.Operation.Should().Be("+");
        historyEntry.Result.Should().Be(15);
        historyEntry.Expression.Should().Be("10 + 5 = 15");
        historyEntry.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task CalculateAsync_DivisionByZero_ShouldThrowException()
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
            .Should().ThrowAsync<DivideByZeroException>()
            .WithMessage("Cannot divide by zero");

        // Verify no history entry was created
        var historyCount = await _context.CalculationHistory.CountAsync();
        historyCount.Should().Be(0);
    }

    [Theory]
    [InlineData("^")]
    [InlineData("%")]
    [InlineData("sqrt")]
    [InlineData("")]
    [InlineData("add")]
    public async Task CalculateAsync_InvalidOperation_ShouldThrowArgumentException(string invalidOperation)
    {
        // Arrange
        var request = new CalculationRequest
        {
            FirstOperand = 10,
            SecondOperand = 5,
            Operation = invalidOperation
        };

        // Act & Assert
        await _calculatorService.Invoking(s => s.CalculateAsync(request))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage($"Unsupported operation: {invalidOperation}");

        // Verify no history entry was created
        var historyCount = await _context.CalculationHistory.CountAsync();
        historyCount.Should().Be(0);
    }

    #endregion

    #region History Management Tests

    [Fact]
    public async Task GetHistoryAsync_EmptyHistory_ShouldReturnEmptyList()
    {
        // Act
        var history = await _calculatorService.GetHistoryAsync();

        // Assert
        history.Should().NotBeNull();
        history.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHistoryAsync_WithHistory_ShouldReturnOrderedByCreatedAtDescending()
    {
        // Arrange
        var entries = new[]
        {
            new CalculationHistory { FirstOperand = 1, SecondOperand = 1, Operation = "+", Result = 2, Expression = "1 + 1 = 2", CreatedAt = DateTime.UtcNow.AddMinutes(-2) },
            new CalculationHistory { FirstOperand = 2, SecondOperand = 2, Operation = "*", Result = 4, Expression = "2 * 2 = 4", CreatedAt = DateTime.UtcNow.AddMinutes(-1) },
            new CalculationHistory { FirstOperand = 5, SecondOperand = 3, Operation = "-", Result = 2, Expression = "5 - 3 = 2", CreatedAt = DateTime.UtcNow }
        };

        _context.CalculationHistory.AddRange(entries);
        await _context.SaveChangesAsync();

        // Act
        var history = await _calculatorService.GetHistoryAsync();

        // Assert
        history.Should().HaveCount(3);
        history.Should().BeInDescendingOrder(h => h.CreatedAt);
        history.First().Result.Should().Be(2); // Most recent (5 - 3 = 2)
        history.Last().Result.Should().Be(2);  // Oldest (1 + 1 = 2)
    }

    [Fact]
    public async Task GetHistoryAsync_WithLimit_ShouldReturnLimitedResults()
    {
        // Arrange
        var entries = Enumerable.Range(1, 10)
            .Select(i => new CalculationHistory
            {
                FirstOperand = i,
                SecondOperand = 1,
                Operation = "+",
                Result = i + 1,
                Expression = $"{i} + 1 = {i + 1}",
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            });

        _context.CalculationHistory.AddRange(entries);
        await _context.SaveChangesAsync();

        // Act
        var history = await _calculatorService.GetHistoryAsync(5);

        // Assert
        history.Should().HaveCount(5);
        history.Should().BeInDescendingOrder(h => h.CreatedAt);
    }

    [Fact]
    public async Task ClearHistoryAsync_WithHistory_ShouldRemoveAllEntries()
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

    [Fact]
    public async Task ClearHistoryAsync_EmptyHistory_ShouldNotThrow()
    {
        // Act & Assert
        await _calculatorService.Invoking(s => s.ClearHistoryAsync())
            .Should().NotThrowAsync();

        var count = await _context.CalculationHistory.CountAsync();
        count.Should().Be(0);
    }

    #endregion

    #region Edge Cases and Error Handling

    [Theory]
    [InlineData(double.MaxValue, double.MaxValue)]
    [InlineData(double.MinValue, double.MinValue)]
    [InlineData(double.PositiveInfinity, 1)]
    [InlineData(double.NegativeInfinity, 1)]
    public async Task CalculateAsync_ExtremeValues_ShouldHandleGracefully(double a, double b)
    {
        // Arrange
        var request = new CalculationRequest
        {
            FirstOperand = a,
            SecondOperand = b,
            Operation = "+"
        };

        // Act & Assert
        await _calculatorService.Invoking(s => s.CalculateAsync(request))
            .Should().NotThrowAsync();
    }

    [Fact]
    public void Add_WithNaN_ShouldReturnNaN()
    {
        // Act
        var result = _calculatorService.Add(double.NaN, 5);

        // Assert
        result.Should().Be(double.NaN);
    }

    [Fact]
    public void Divide_PositiveInfinityByPositiveInfinity_ShouldReturnNaN()
    {
        // Act
        var result = _calculatorService.Divide(double.PositiveInfinity, double.PositiveInfinity);

        // Assert
        result.Should().Be(double.NaN);
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }
}