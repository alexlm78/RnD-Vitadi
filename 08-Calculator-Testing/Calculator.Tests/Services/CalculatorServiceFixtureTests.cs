using Calculator.Api.Models;
using Calculator.Tests.Fixtures;
using Calculator.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Calculator.Tests.Services;

/// <summary>
/// Tests using shared test fixture to demonstrate fixture-based testing
/// </summary>
[Collection("Calculator Collection")]
public class CalculatorServiceFixtureTests
{
    private readonly CalculatorTestFixture _fixture;

    public CalculatorServiceFixtureTests(CalculatorTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetHistoryAsync_WithSeededData_ShouldReturnCorrectOrder()
    {
        // Arrange
        await _fixture.ClearTestDataAsync();
        await _fixture.SeedTestDataAsync();

        // Act
        var history = await _fixture.Service.GetHistoryAsync();

        // Assert
        history.Should().HaveCount(3);
        history.Should().BeInDescendingOrder(h => h.CreatedAt);
        
        var historyList = history.ToList();
        historyList[0].Operation.Should().Be("/"); // Most recent
        historyList[1].Operation.Should().Be("*"); // Middle
        historyList[2].Operation.Should().Be("+"); // Oldest
    }

    [Fact]
    public async Task CalculateAsync_WithExistingHistory_ShouldAddToHistory()
    {
        // Arrange
        await _fixture.ClearTestDataAsync();
        await _fixture.SeedTestDataAsync();
        
        var initialCount = await _fixture.Context.CalculationHistory.CountAsync();
        initialCount.Should().Be(3);

        var request = TestDataBuilder.CalculationRequest()
            .WithFirstOperand(50)
            .WithSecondOperand(25)
            .WithSubtraction()
            .Build();

        // Act
        var result = await _fixture.Service.CalculateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().Be(25);

        var finalCount = await _fixture.Context.CalculationHistory.CountAsync();
        finalCount.Should().Be(4);

        var latestEntry = await _fixture.Context.CalculationHistory
            .OrderByDescending(h => h.CreatedAt)
            .FirstAsync();
        
        latestEntry.FirstOperand.Should().Be(50);
        latestEntry.SecondOperand.Should().Be(25);
        latestEntry.Operation.Should().Be("-");
        latestEntry.Result.Should().Be(25);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(10)]
    public async Task GetHistoryAsync_WithDifferentLimits_ShouldRespectLimit(int limit)
    {
        // Arrange
        await _fixture.ClearTestDataAsync();
        await _fixture.SeedTestDataAsync();

        // Act
        var history = await _fixture.Service.GetHistoryAsync(limit);

        // Assert
        var expectedCount = Math.Min(limit, 3); // We seeded 3 records
        history.Should().HaveCount(expectedCount);
        history.Should().BeInDescendingOrder(h => h.CreatedAt);
    }

    [Fact]
    public async Task ClearHistoryAsync_WithSeededData_ShouldRemoveAllEntries()
    {
        // Arrange
        await _fixture.ClearTestDataAsync();
        await _fixture.SeedTestDataAsync();
        
        var initialCount = await _fixture.Context.CalculationHistory.CountAsync();
        initialCount.Should().Be(3);

        // Act
        await _fixture.Service.ClearHistoryAsync();

        // Assert
        var finalCount = await _fixture.Context.CalculationHistory.CountAsync();
        finalCount.Should().Be(0);
    }

    [Fact]
    public async Task MultipleOperations_ShouldMaintainHistoryIntegrity()
    {
        // Arrange
        await _fixture.ClearTestDataAsync();
        
        var operations = new[]
        {
            TestDataBuilder.CalculationRequest().WithFirstOperand(10).WithSecondOperand(2).WithAddition().Build(),
            TestDataBuilder.CalculationRequest().WithFirstOperand(15).WithSecondOperand(3).WithMultiplication().Build(),
            TestDataBuilder.CalculationRequest().WithFirstOperand(20).WithSecondOperand(4).WithDivision().Build(),
            TestDataBuilder.CalculationRequest().WithFirstOperand(25).WithSecondOperand(5).WithSubtraction().Build()
        };

        // Act
        var results = new List<CalculationResult>();
        foreach (var operation in operations)
        {
            var result = await _fixture.Service.CalculateAsync(operation);
            results.Add(result);
        }

        // Assert
        results.Should().HaveCount(4);
        results[0].Result.Should().Be(12);  // 10 + 2
        results[1].Result.Should().Be(45);  // 15 * 3
        results[2].Result.Should().Be(5);   // 20 / 4
        results[3].Result.Should().Be(20);  // 25 - 5

        var history = await _fixture.Service.GetHistoryAsync();
        history.Should().HaveCount(4);
        history.Should().BeInDescendingOrder(h => h.CreatedAt);

        // Verify all operations are in history with correct results
        var historyList = history.ToList();
        historyList.Should().Contain(h => h.Result == 12 && h.Operation == "+");
        historyList.Should().Contain(h => h.Result == 45 && h.Operation == "*");
        historyList.Should().Contain(h => h.Result == 5 && h.Operation == "/");
        historyList.Should().Contain(h => h.Result == 20 && h.Operation == "-");
    }
}