using Calculator.Api.Data;
using Calculator.Api.Models;
using Calculator.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Calculator.Tests.Fixtures;

/// <summary>
/// Test fixture for sharing test setup across multiple test classes
/// </summary>
public class CalculatorTestFixture : IDisposable
{
    public CalculatorDbContext Context { get; }
    public CalculatorService Service { get; }

    public CalculatorTestFixture()
    {
        var options = new DbContextOptionsBuilder<CalculatorDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        Context = new CalculatorDbContext(options);
        Service = new CalculatorService(Context);
    }

    /// <summary>
    /// Seeds the database with test data
    /// </summary>
    public async Task SeedTestDataAsync()
    {
        var testData = new[]
        {
            new CalculationHistory
            {
                FirstOperand = 10,
                SecondOperand = 5,
                Operation = "+",
                Result = 15,
                Expression = "10 + 5 = 15",
                CreatedAt = DateTime.UtcNow.AddMinutes(-10)
            },
            new CalculationHistory
            {
                FirstOperand = 20,
                SecondOperand = 4,
                Operation = "*",
                Result = 80,
                Expression = "20 * 4 = 80",
                CreatedAt = DateTime.UtcNow.AddMinutes(-5)
            },
            new CalculationHistory
            {
                FirstOperand = 100,
                SecondOperand = 10,
                Operation = "/",
                Result = 10,
                Expression = "100 / 10 = 10",
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            }
        };

        Context.CalculationHistory.AddRange(testData);
        await Context.SaveChangesAsync();
    }

    /// <summary>
    /// Clears all test data from the database
    /// </summary>
    public async Task ClearTestDataAsync()
    {
        Context.CalculationHistory.RemoveRange(Context.CalculationHistory);
        await Context.SaveChangesAsync();
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}

/// <summary>
/// Collection definition for sharing the fixture across test classes
/// </summary>
[CollectionDefinition("Calculator Collection")]
public class CalculatorCollection : ICollectionFixture<CalculatorTestFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}