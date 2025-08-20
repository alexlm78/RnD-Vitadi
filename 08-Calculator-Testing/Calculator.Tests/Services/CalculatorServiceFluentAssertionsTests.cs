using Calculator.Api.Data;
using Calculator.Api.Models;
using Calculator.Api.Services;
using Calculator.Tests.Helpers;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Calculator.Tests.Services;

/// <summary>
/// Tests demonstrating advanced FluentAssertions usage for expressive testing
/// </summary>
public class CalculatorServiceFluentAssertionsTests : IDisposable
{
    private readonly CalculatorDbContext _context;
    private readonly CalculatorService _calculatorService;

    public CalculatorServiceFluentAssertionsTests()
    {
        var options = new DbContextOptionsBuilder<CalculatorDbContext>()
            .UseInMemoryDatabase($"FluentTestDb_{Guid.NewGuid()}")
            .Options;

        _context = new CalculatorDbContext(options);
        _calculatorService = new CalculatorService(_context);
    }

    #region Advanced FluentAssertions Demonstrations

    [Fact]
    public async Task CalculateAsync_Result_ShouldSatisfyAllConditions()
    {
        // Arrange
        var request = TestDataBuilder.CalculationRequest()
            .WithFirstOperand(100)
            .WithSecondOperand(25)
            .WithDivision()
            .Build();

        // Act
        var result = await _calculatorService.CalculateAsync(request);

        // Assert - Using Should().Satisfy() for complex conditions
        result.Should().NotBeNull();
        result.FirstOperand.Should().Be(100);
        result.SecondOperand.Should().Be(25);
        result.Operation.Should().Be("/");
        result.Result.Should().Be(4);
        result.Expression.Should().Contain("100 / 25 = 4");
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, 1.Seconds());
    }

    [Fact]
    public async Task CalculateAsync_WithMultipleOperations_ShouldHaveExpectedCollectionProperties()
    {
        // Arrange
        var operations = new[]
        {
            TestDataBuilder.CalculationRequest().WithFirstOperand(10).WithSecondOperand(2).WithAddition().Build(),
            TestDataBuilder.CalculationRequest().WithFirstOperand(20).WithSecondOperand(4).WithSubtraction().Build(),
            TestDataBuilder.CalculationRequest().WithFirstOperand(5).WithSecondOperand(6).WithMultiplication().Build()
        };

        // Act
        var results = new List<CalculationResult>();
        foreach (var operation in operations)
        {
            results.Add(await _calculatorService.CalculateAsync(operation));
        }

        // Assert - Advanced collection assertions
        results.Should()
            .HaveCount(3)
            .And.OnlyContain(r => r.Timestamp <= DateTime.UtcNow.AddSeconds(5) && r.Timestamp >= DateTime.UtcNow.AddSeconds(-5))
            .And.Satisfy(
                r => r.Result == 12 && r.Operation == "+",
                r => r.Result == 16 && r.Operation == "-",
                r => r.Result == 30 && r.Operation == "*"
            );

        // Collection should be ordered by execution time (approximately)
        results.Should().BeInAscendingOrder(r => r.Timestamp);

        // All results should have valid expressions
        results.Should().OnlyContain(r => !string.IsNullOrEmpty(r.Expression));
        results.Should().OnlyContain(r => r.Expression.Contains("="));
    }

    [Fact]
    public async Task GetHistoryAsync_WithVariousEntries_ShouldHaveExpectedStructure()
    {
        // Arrange
        var baseTime = DateTime.UtcNow;
        var testEntries = new[]
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

        _context.CalculationHistory.AddRange(testEntries);
        await _context.SaveChangesAsync();

        // Act
        var history = await _calculatorService.GetHistoryAsync();

        // Assert - Complex collection assertions
        history.Should()
            .NotBeNull()
            .And.HaveCount(3)
            .And.BeInDescendingOrder(h => h.CreatedAt)
            .And.OnlyContain(h => h.Id > 0)
            .And.OnlyContain(h => !string.IsNullOrEmpty(h.Operation))
            .And.OnlyContain(h => !string.IsNullOrEmpty(h.Expression))
            .And.OnlyContain(h => h.CreatedAt <= DateTime.UtcNow);

        // Specific assertions on collection elements
        history.Should().ContainSingle(h => h.Operation == "/" && h.Result == 10);
        history.Should().ContainSingle(h => h.Operation == "*" && h.Result == 80);
        history.Should().ContainSingle(h => h.Operation == "+" && h.Result == 15);

        // Time-based assertions
        history.Should().OnlyContain(h => h.CreatedAt <= DateTime.UtcNow);
        history.First().CreatedAt.Should().BeCloseTo(baseTime.AddMinutes(-1), 1.Minutes());
        history.Last().CreatedAt.Should().BeCloseTo(baseTime.AddMinutes(-5), 1.Minutes());
    }

    [Theory]
    [InlineData(double.MaxValue, 1, "+")]
    [InlineData(double.MinValue, 1, "+")]
    [InlineData(1, double.MaxValue, "*")]
    [InlineData(double.PositiveInfinity, 1, "+")]
    [InlineData(double.NegativeInfinity, 1, "+")]
    [InlineData(double.NaN, 5, "+")]
    public async Task CalculateAsync_WithExtremeValues_ShouldHandleGracefully(double a, double b, string operation)
    {
        // Arrange
        var request = TestDataBuilder.CalculationRequest()
            .WithFirstOperand(a)
            .WithSecondOperand(b)
            .WithOperation(operation)
            .Build();

        // Act
        var action = async () => await _calculatorService.CalculateAsync(request);

        // Assert - Should not throw and should handle extreme values
        await action.Should().NotThrowAsync();

        var result = await _calculatorService.CalculateAsync(request);
        result.Should().NotBeNull();
        result.FirstOperand.Should().Be(a);
        result.SecondOperand.Should().Be(b);
        result.Operation.Should().Be(operation);
        
        // Result might be extreme value, but should be a valid double
        result.Result.Should().BeOfType(typeof(double));
        result.Expression.Should().NotBeNullOrEmpty();
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, 1.Seconds());
    }

    [Fact]
    public async Task CalculateAsync_WithPrecisionChallenges_ShouldMaintainAccuracy()
    {
        // Arrange - Classic floating point precision issues
        var precisionTests = new[]
        {
            new { A = 0.1, B = 0.2, Op = "+", Expected = 0.3 },
            new { A = 1.0, B = 3.0, Op = "/", Expected = 0.3333333333333333 },
            new { A = 999999999999999.0, B = 1.0, Op = "+", Expected = 1000000000000000.0 }
        };

        foreach (var test in precisionTests)
        {
            var request = TestDataBuilder.CalculationRequest()
                .WithFirstOperand(test.A)
                .WithSecondOperand(test.B)
                .WithOperation(test.Op)
                .Build();

            // Act
            var result = await _calculatorService.CalculateAsync(request);

            // Assert - Using appropriate precision for floating point comparisons
            if (test.Op == "/" && test.A == 1.0 && test.B == 3.0)
            {
                result.Result.Should().BeApproximately(test.Expected, 0.0000000000001);
            }
            else if (test.A == 0.1 && test.B == 0.2)
            {
                result.Result.Should().BeApproximately(test.Expected, 0.0000000000001);
            }
            else
            {
                result.Result.Should().Be(test.Expected);
            }

            result.FirstOperand.Should().Be(test.A);
            result.SecondOperand.Should().Be(test.B);
            result.Operation.Should().Be(test.Op);
            result.Expression.Should().NotBeNullOrEmpty();
            result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, 1.Seconds());
        }
    }

    [Fact]
    public async Task ClearHistoryAsync_WithLargeDataset_ShouldRemoveAllEfficiently()
    {
        // Arrange - Create large dataset
        var largeDataset = Enumerable.Range(1, 1000)
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

        _context.CalculationHistory.AddRange(largeDataset);
        await _context.SaveChangesAsync();

        // Verify initial state
        var initialCount = await _context.CalculationHistory.CountAsync();
        initialCount.Should().Be(1000);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await _calculatorService.ClearHistoryAsync();
        stopwatch.Stop();

        // Assert - Performance and correctness
        var finalCount = await _context.CalculationHistory.CountAsync();
        finalCount.Should().Be(0);
        
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, "clearing 1000 records should be fast");

        // Verify database is truly empty
        var remainingEntries = await _context.CalculationHistory.ToListAsync();
        remainingEntries.Should().BeEmpty();
    }

    [Fact]
    public async Task CalculateAsync_ConcurrentOperations_ShouldMaintainDataIntegrity()
    {
        // Arrange
        var concurrentOperations = Enumerable.Range(1, 50)
            .Select(i => TestDataBuilder.CalculationRequest()
                .WithFirstOperand(i)
                .WithSecondOperand(1)
                .WithAddition()
                .Build())
            .ToList();

        // Act - Execute operations concurrently
        var tasks = concurrentOperations.Select(op => _calculatorService.CalculateAsync(op));
        var results = await Task.WhenAll(tasks);

        // Assert - All operations should complete successfully
        results.Should()
            .HaveCount(50)
            .And.OnlyContain(r => r != null)
            .And.OnlyContain(r => r.Operation == "+")
            .And.OnlyContain(r => r.Result == r.FirstOperand + r.SecondOperand);

        // Verify all entries were persisted
        var historyCount = await _context.CalculationHistory.CountAsync();
        historyCount.Should().Be(50);

        // Verify data integrity
        var history = await _context.CalculationHistory.ToListAsync();
        history.Should()
            .OnlyContain(h => h.Operation == "+")
            .And.OnlyContain(h => h.Result == h.FirstOperand + h.SecondOperand)
            .And.OnlyContain(h => h.FirstOperand >= 1 && h.FirstOperand <= 50)
            .And.OnlyContain(h => h.SecondOperand == 1);

        // Verify unique entries (no duplicates due to concurrency issues)
        history.Select(h => h.FirstOperand).Should().OnlyHaveUniqueItems();
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }
}

