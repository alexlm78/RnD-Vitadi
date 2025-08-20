using Calculator.Api.Data;
using Calculator.Api.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Calculator.Tests.Integration;

/// <summary>
/// Integration tests for Calculator API using TestServer
/// </summary>
public class CalculatorIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly IServiceScope _scope;
    private readonly CalculatorDbContext _context;

    public CalculatorIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<CalculatorDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add InMemory database for testing - SHARED DATABASE NAME
                services.AddDbContext<CalculatorDbContext>(options =>
                {
                    options.UseInMemoryDatabase("SharedTestDb");
                });
            });
        });

        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<CalculatorDbContext>();
        
        // Clear database before each test
        _context.CalculationHistory.RemoveRange(_context.CalculationHistory);
        _context.SaveChanges();
    }

    #region Calculate Endpoint Tests

    [Theory]
    [InlineData(10, 5, "+", 15)]
    [InlineData(10, 5, "-", 5)]
    [InlineData(10, 5, "*", 50)]
    [InlineData(10, 5, "/", 2)]
    public async Task Calculate_ValidRequest_ShouldReturnCorrectResult(
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
        var response = await _client.PostAsJsonAsync("/api/calculator/calculate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CalculationResult>();
        result.Should().NotBeNull();
        result!.FirstOperand.Should().Be(firstOperand);
        result.SecondOperand.Should().Be(secondOperand);
        result.Operation.Should().Be(operation);
        result.Result.Should().Be(expectedResult);
        result.Expression.Should().Be($"{firstOperand} {operation} {secondOperand} = {expectedResult}");
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Verify history was saved
        var historyCount = await _context.CalculationHistory.CountAsync();
        historyCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Calculate_DivisionByZero_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CalculationRequest
        {
            FirstOperand = 10,
            SecondOperand = 0,
            Operation = "/"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/calculator/calculate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorMessage = await response.Content.ReadAsStringAsync();
        errorMessage.Should().Contain("Cannot divide by zero");

        // Verify no history was saved
        var historyCount = await _context.CalculationHistory.CountAsync();
        historyCount.Should().Be(0);
    }

    [Fact]
    public async Task Calculate_InvalidOperation_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CalculationRequest
        {
            FirstOperand = 10,
            SecondOperand = 5,
            Operation = "^"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/calculator/calculate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorMessage = await response.Content.ReadAsStringAsync();
        // The validation error message comes from model validation, not the service
        errorMessage.Should().Contain("Operation must be one of");
    }

    [Fact]
    public async Task Calculate_InvalidModel_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidRequest = new { FirstOperand = 10 }; // Missing required fields

        // Act
        var response = await _client.PostAsJsonAsync("/api/calculator/calculate", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Basic Operation Endpoints Tests

    [Theory]
    [InlineData(10, 5, 15)]
    [InlineData(-10, 5, -5)]
    [InlineData(0, 0, 0)]
    [InlineData(2.5, 3.7, 6.2)]
    public async Task Add_ValidNumbers_ShouldReturnSum(double a, double b, double expected)
    {
        // Act
        var response = await _client.GetAsync($"/api/calculator/add?a={a}&b={b}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<double>();
        result.Should().BeApproximately(expected, 0.0001);
    }

    [Theory]
    [InlineData(10, 5, 5)]
    [InlineData(-10, 5, -15)]
    [InlineData(0, 0, 0)]
    [InlineData(7.5, 2.3, 5.2)]
    public async Task Subtract_ValidNumbers_ShouldReturnDifference(double a, double b, double expected)
    {
        // Act
        var response = await _client.GetAsync($"/api/calculator/subtract?a={a}&b={b}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<double>();
        result.Should().BeApproximately(expected, 0.0001);
    }

    [Theory]
    [InlineData(10, 5, 50)]
    [InlineData(-10, 5, -50)]
    [InlineData(0, 5, 0)]
    [InlineData(2.5, 4, 10)]
    public async Task Multiply_ValidNumbers_ShouldReturnProduct(double a, double b, double expected)
    {
        // Act
        var response = await _client.GetAsync($"/api/calculator/multiply?a={a}&b={b}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<double>();
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(10, 5, 2)]
    [InlineData(-10, 5, -2)]
    [InlineData(0, 5, 0)]
    [InlineData(7.5, 2.5, 3)]
    public async Task Divide_ValidNumbers_ShouldReturnQuotient(double a, double b, double expected)
    {
        // Act
        var response = await _client.GetAsync($"/api/calculator/divide?a={a}&b={b}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<double>();
        result.Should().Be(expected);
    }

    [Fact]
    public async Task Divide_ByZero_ShouldReturnBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/calculator/divide?a=10&b=0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorMessage = await response.Content.ReadAsStringAsync();
        errorMessage.Should().Contain("Cannot divide by zero");
    }

    #endregion

    #region History Endpoints Tests

    [Fact]
    public async Task GetHistory_EmptyHistory_ShouldReturnEmptyArray()
    {
        // Act
        var response = await _client.GetAsync("/api/calculator/history");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var history = await response.Content.ReadFromJsonAsync<CalculationHistory[]>();
        history.Should().NotBeNull();
        history!.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHistory_WithCalculations_ShouldReturnHistory()
    {
        // Arrange - Perform some calculations first
        var calculations = new[]
        {
            new CalculationRequest { FirstOperand = 10, SecondOperand = 5, Operation = "+" },
            new CalculationRequest { FirstOperand = 20, SecondOperand = 4, Operation = "*" },
            new CalculationRequest { FirstOperand = 15, SecondOperand = 3, Operation = "/" }
        };

        foreach (var calc in calculations)
        {
            await _client.PostAsJsonAsync("/api/calculator/calculate", calc);
        }

        // Act
        var response = await _client.GetAsync("/api/calculator/history");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var history = await response.Content.ReadFromJsonAsync<CalculationHistory[]>();
        history.Should().NotBeNull();
        history!.Should().HaveCount(3);
        history.Should().BeInDescendingOrder(h => h.CreatedAt);

        // Verify the calculations are correct
        history.Should().Contain(h => h.Expression == "10 + 5 = 15");
        history.Should().Contain(h => h.Expression == "20 * 4 = 80");
        history.Should().Contain(h => h.Expression == "15 / 3 = 5");
    }

    [Fact]
    public async Task GetHistory_WithLimit_ShouldReturnLimitedResults()
    {
        // Arrange - Perform multiple calculations
        for (int i = 1; i <= 10; i++)
        {
            var calc = new CalculationRequest { FirstOperand = i, SecondOperand = 1, Operation = "+" };
            await _client.PostAsJsonAsync("/api/calculator/calculate", calc);
        }

        // Act
        var response = await _client.GetAsync("/api/calculator/history?limit=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var history = await response.Content.ReadFromJsonAsync<CalculationHistory[]>();
        history.Should().NotBeNull();
        history!.Should().HaveCount(5);
        history.Should().BeInDescendingOrder(h => h.CreatedAt);
    }

    [Fact]
    public async Task ClearHistory_WithHistory_ShouldClearAllEntries()
    {
        // Arrange - Add some calculations
        var calc = new CalculationRequest { FirstOperand = 10, SecondOperand = 5, Operation = "+" };
        await _client.PostAsJsonAsync("/api/calculator/calculate", calc);

        // Verify history exists
        var historyResponse = await _client.GetAsync("/api/calculator/history");
        var history = await historyResponse.Content.ReadFromJsonAsync<CalculationHistory[]>();
        history!.Should().HaveCount(1);

        // Act
        var response = await _client.DeleteAsync("/api/calculator/history");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var message = await response.Content.ReadAsStringAsync();
        message.Should().Contain("History cleared successfully");

        // Verify history is empty
        var emptyHistoryResponse = await _client.GetAsync("/api/calculator/history");
        var emptyHistory = await emptyHistoryResponse.Content.ReadFromJsonAsync<CalculationHistory[]>();
        emptyHistory!.Should().BeEmpty();
    }

    [Fact]
    public async Task ClearHistory_EmptyHistory_ShouldReturnSuccess()
    {
        // Act
        var response = await _client.DeleteAsync("/api/calculator/history");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var message = await response.Content.ReadAsStringAsync();
        message.Should().Contain("History cleared successfully");
    }

    #endregion

    #region End-to-End Workflow Tests

    [Fact]
    public async Task CompleteWorkflow_CalculateAndRetrieveHistory_ShouldWorkCorrectly()
    {
        // Arrange
        var calculations = new[]
        {
            new { Request = new CalculationRequest { FirstOperand = 10, SecondOperand = 5, Operation = "+" }, Expected = 15.0 },
            new { Request = new CalculationRequest { FirstOperand = 20, SecondOperand = 4, Operation = "-" }, Expected = 16.0 },
            new { Request = new CalculationRequest { FirstOperand = 6, SecondOperand = 7, Operation = "*" }, Expected = 42.0 },
            new { Request = new CalculationRequest { FirstOperand = 100, SecondOperand = 10, Operation = "/" }, Expected = 10.0 }
        };

        // Act & Assert - Perform calculations
        foreach (var calc in calculations)
        {
            var response = await _client.PostAsJsonAsync("/api/calculator/calculate", calc.Request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<CalculationResult>();
            result!.Result.Should().Be(calc.Expected);
        }

        // Verify complete history
        var historyResponse = await _client.GetAsync("/api/calculator/history");
        historyResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var history = await historyResponse.Content.ReadFromJsonAsync<CalculationHistory[]>();
        history!.Should().HaveCount(4);
        history.Should().BeInDescendingOrder(h => h.CreatedAt);

        // Verify all calculations are present
        history.Should().Contain(h => h.Result == 15 && h.Operation == "+");
        history.Should().Contain(h => h.Result == 16 && h.Operation == "-");
        history.Should().Contain(h => h.Result == 42 && h.Operation == "*");
        history.Should().Contain(h => h.Result == 10 && h.Operation == "/");

        // Clear history and verify
        var clearResponse = await _client.DeleteAsync("/api/calculator/history");
        clearResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var emptyHistoryResponse = await _client.GetAsync("/api/calculator/history");
        var emptyHistory = await emptyHistoryResponse.Content.ReadFromJsonAsync<CalculationHistory[]>();
        emptyHistory!.Should().BeEmpty();
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Api_InvalidEndpoint_ShouldReturn404()
    {
        // Act
        var response = await _client.GetAsync("/api/calculator/invalid");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Api_InvalidHttpMethod_ShouldReturn405()
    {
        // Act
        var response = await _client.PutAsync("/api/calculator/add", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Theory]
    [InlineData("not-a-number", "5")]
    [InlineData("10", "not-a-number")]
    [InlineData("", "5")]
    [InlineData("10", "")]
    public async Task BasicOperations_InvalidParameters_ShouldReturnBadRequest(string a, string b)
    {
        // Act
        var response = await _client.GetAsync($"/api/calculator/add?a={a}&b={b}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    public void Dispose()
    {
        _scope.Dispose();
        _client.Dispose();
    }
}