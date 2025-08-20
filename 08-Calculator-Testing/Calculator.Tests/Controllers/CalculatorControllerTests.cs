using Calculator.Api.Controllers;
using Calculator.Api.Models;
using Calculator.Api.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Calculator.Tests.Controllers;

/// <summary>
/// Unit tests for CalculatorController using Moq for service mocking
/// </summary>
public class CalculatorControllerTests
{
    private readonly Mock<ICalculatorService> _mockCalculatorService;
    private readonly CalculatorController _controller;

    public CalculatorControllerTests()
    {
        _mockCalculatorService = new Mock<ICalculatorService>();
        _controller = new CalculatorController(_mockCalculatorService.Object);
    }

    #region Calculate Endpoint Tests

    [Fact]
    public async Task Calculate_ValidRequest_ShouldReturnOkWithResult()
    {
        // Arrange
        var request = new CalculationRequest
        {
            FirstOperand = 10,
            SecondOperand = 5,
            Operation = "+"
        };

        var expectedResult = new CalculationResult
        {
            FirstOperand = 10,
            SecondOperand = 5,
            Operation = "+",
            Result = 15,
            Expression = "10 + 5 = 15",
            Timestamp = DateTime.UtcNow
        };

        _mockCalculatorService
            .Setup(s => s.CalculateAsync(It.IsAny<CalculationRequest>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Calculate(request);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResult);

        _mockCalculatorService.Verify(s => s.CalculateAsync(request), Times.Once);
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

        _mockCalculatorService
            .Setup(s => s.CalculateAsync(It.IsAny<CalculationRequest>()))
            .ThrowsAsync(new DivideByZeroException("Cannot divide by zero"));

        // Act
        var result = await _controller.Calculate(request);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Cannot divide by zero");

        _mockCalculatorService.Verify(s => s.CalculateAsync(request), Times.Once);
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

        _mockCalculatorService
            .Setup(s => s.CalculateAsync(It.IsAny<CalculationRequest>()))
            .ThrowsAsync(new ArgumentException("Unsupported operation: ^"));

        // Act
        var result = await _controller.Calculate(request);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Unsupported operation: ^");

        _mockCalculatorService.Verify(s => s.CalculateAsync(request), Times.Once);
    }

    [Fact]
    public async Task Calculate_ServiceThrowsUnexpectedException_ShouldPropagate()
    {
        // Arrange
        var request = new CalculationRequest
        {
            FirstOperand = 10,
            SecondOperand = 5,
            Operation = "+"
        };

        _mockCalculatorService
            .Setup(s => s.CalculateAsync(It.IsAny<CalculationRequest>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act & Assert
        await _controller.Invoking(c => c.Calculate(request))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database connection failed");

        _mockCalculatorService.Verify(s => s.CalculateAsync(request), Times.Once);
    }

    #endregion

    #region Basic Operation Endpoints Tests

    [Theory]
    [InlineData(10, 5, 15)]
    [InlineData(-10, 5, -5)]
    [InlineData(0, 0, 0)]
    [InlineData(2.5, 3.7, 6.2)]
    public void Add_ValidNumbers_ShouldReturnSum(double a, double b, double expected)
    {
        // Arrange
        _mockCalculatorService
            .Setup(s => s.Add(a, b))
            .Returns(expected);

        // Act
        var result = _controller.Add(a, b);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().Be(expected);

        _mockCalculatorService.Verify(s => s.Add(a, b), Times.Once);
    }

    [Theory]
    [InlineData(10, 5, 5)]
    [InlineData(-10, 5, -15)]
    [InlineData(0, 0, 0)]
    [InlineData(7.5, 2.3, 5.2)]
    public void Subtract_ValidNumbers_ShouldReturnDifference(double a, double b, double expected)
    {
        // Arrange
        _mockCalculatorService
            .Setup(s => s.Subtract(a, b))
            .Returns(expected);

        // Act
        var result = _controller.Subtract(a, b);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().Be(expected);

        _mockCalculatorService.Verify(s => s.Subtract(a, b), Times.Once);
    }

    [Theory]
    [InlineData(10, 5, 50)]
    [InlineData(-10, 5, -50)]
    [InlineData(0, 5, 0)]
    [InlineData(2.5, 4, 10)]
    public void Multiply_ValidNumbers_ShouldReturnProduct(double a, double b, double expected)
    {
        // Arrange
        _mockCalculatorService
            .Setup(s => s.Multiply(a, b))
            .Returns(expected);

        // Act
        var result = _controller.Multiply(a, b);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().Be(expected);

        _mockCalculatorService.Verify(s => s.Multiply(a, b), Times.Once);
    }

    [Theory]
    [InlineData(10, 5, 2)]
    [InlineData(-10, 5, -2)]
    [InlineData(0, 5, 0)]
    [InlineData(7.5, 2.5, 3)]
    public void Divide_ValidNumbers_ShouldReturnQuotient(double a, double b, double expected)
    {
        // Arrange
        _mockCalculatorService
            .Setup(s => s.Divide(a, b))
            .Returns(expected);

        // Act
        var result = _controller.Divide(a, b);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().Be(expected);

        _mockCalculatorService.Verify(s => s.Divide(a, b), Times.Once);
    }

    [Fact]
    public void Divide_ByZero_ShouldReturnBadRequest()
    {
        // Arrange
        _mockCalculatorService
            .Setup(s => s.Divide(10, 0))
            .Throws(new DivideByZeroException("Cannot divide by zero"));

        // Act
        var result = _controller.Divide(10, 0);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Cannot divide by zero");

        _mockCalculatorService.Verify(s => s.Divide(10, 0), Times.Once);
    }

    #endregion

    #region History Endpoints Tests

    [Fact]
    public async Task GetHistory_DefaultLimit_ShouldReturnHistoryFromService()
    {
        // Arrange
        var expectedHistory = new List<CalculationHistory>
        {
            new() { Id = 1, FirstOperand = 10, SecondOperand = 5, Operation = "+", Result = 15, Expression = "10 + 5 = 15", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, FirstOperand = 20, SecondOperand = 4, Operation = "*", Result = 80, Expression = "20 * 4 = 80", CreatedAt = DateTime.UtcNow.AddMinutes(-1) }
        };

        _mockCalculatorService
            .Setup(s => s.GetHistoryAsync(50))
            .ReturnsAsync(expectedHistory);

        // Act
        var result = await _controller.GetHistory();

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedHistory);

        _mockCalculatorService.Verify(s => s.GetHistoryAsync(50), Times.Once);
    }

    [Fact]
    public async Task GetHistory_CustomLimit_ShouldReturnHistoryWithLimit()
    {
        // Arrange
        var expectedHistory = new List<CalculationHistory>
        {
            new() { Id = 1, FirstOperand = 10, SecondOperand = 5, Operation = "+", Result = 15, Expression = "10 + 5 = 15", CreatedAt = DateTime.UtcNow }
        };

        _mockCalculatorService
            .Setup(s => s.GetHistoryAsync(10))
            .ReturnsAsync(expectedHistory);

        // Act
        var result = await _controller.GetHistory(10);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedHistory);

        _mockCalculatorService.Verify(s => s.GetHistoryAsync(10), Times.Once);
    }

    [Fact]
    public async Task GetHistory_EmptyHistory_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyHistory = new List<CalculationHistory>();

        _mockCalculatorService
            .Setup(s => s.GetHistoryAsync(It.IsAny<int>()))
            .ReturnsAsync(emptyHistory);

        // Act
        var result = await _controller.GetHistory();

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(emptyHistory);

        _mockCalculatorService.Verify(s => s.GetHistoryAsync(50), Times.Once);
    }

    [Fact]
    public async Task ClearHistory_ShouldCallServiceAndReturnSuccessMessage()
    {
        // Arrange
        _mockCalculatorService
            .Setup(s => s.ClearHistoryAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ClearHistory();

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
        
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().Be("History cleared successfully");

        _mockCalculatorService.Verify(s => s.ClearHistoryAsync(), Times.Once);
    }

    [Fact]
    public async Task ClearHistory_ServiceThrowsException_ShouldPropagate()
    {
        // Arrange
        _mockCalculatorService
            .Setup(s => s.ClearHistoryAsync())
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await _controller.Invoking(c => c.ClearHistory())
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");

        _mockCalculatorService.Verify(s => s.ClearHistoryAsync(), Times.Once);
    }

    #endregion

    #region Mock Verification Tests

    [Fact]
    public async Task Calculate_ShouldCallServiceWithExactParameters()
    {
        // Arrange
        var request = new CalculationRequest
        {
            FirstOperand = 123.45,
            SecondOperand = 67.89,
            Operation = "*"
        };

        var expectedResult = new CalculationResult
        {
            FirstOperand = 123.45,
            SecondOperand = 67.89,
            Operation = "*",
            Result = 8385.1305,
            Expression = "123.45 * 67.89 = 8385.1305",
            Timestamp = DateTime.UtcNow
        };

        _mockCalculatorService
            .Setup(s => s.CalculateAsync(It.Is<CalculationRequest>(r => 
                r.FirstOperand == 123.45 && 
                r.SecondOperand == 67.89 && 
                r.Operation == "*")))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Calculate(request);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();

        _mockCalculatorService.Verify(s => s.CalculateAsync(It.Is<CalculationRequest>(r => 
            r.FirstOperand == 123.45 && 
            r.SecondOperand == 67.89 && 
            r.Operation == "*")), Times.Once);
    }

    [Fact]
    public void BasicOperations_ShouldNeverCallCalculateAsync()
    {
        // Arrange
        _mockCalculatorService.Setup(s => s.Add(It.IsAny<double>(), It.IsAny<double>())).Returns(0);
        _mockCalculatorService.Setup(s => s.Subtract(It.IsAny<double>(), It.IsAny<double>())).Returns(0);
        _mockCalculatorService.Setup(s => s.Multiply(It.IsAny<double>(), It.IsAny<double>())).Returns(0);
        _mockCalculatorService.Setup(s => s.Divide(It.IsAny<double>(), It.IsAny<double>())).Returns(0);

        // Act
        _controller.Add(1, 2);
        _controller.Subtract(3, 4);
        _controller.Multiply(5, 6);
        _controller.Divide(7, 8);

        // Assert
        _mockCalculatorService.Verify(s => s.CalculateAsync(It.IsAny<CalculationRequest>()), Times.Never);
        _mockCalculatorService.Verify(s => s.Add(1, 2), Times.Once);
        _mockCalculatorService.Verify(s => s.Subtract(3, 4), Times.Once);
        _mockCalculatorService.Verify(s => s.Multiply(5, 6), Times.Once);
        _mockCalculatorService.Verify(s => s.Divide(7, 8), Times.Once);
    }

    [Fact]
    public async Task MultipleHistoryOperations_ShouldCallServiceCorrectNumberOfTimes()
    {
        // Arrange
        var emptyHistory = new List<CalculationHistory>();
        _mockCalculatorService.Setup(s => s.GetHistoryAsync(It.IsAny<int>())).ReturnsAsync(emptyHistory);
        _mockCalculatorService.Setup(s => s.ClearHistoryAsync()).Returns(Task.CompletedTask);

        // Act
        await _controller.GetHistory();
        await _controller.GetHistory(10);
        await _controller.GetHistory(25);
        await _controller.ClearHistory();

        // Assert
        _mockCalculatorService.Verify(s => s.GetHistoryAsync(50), Times.Once);
        _mockCalculatorService.Verify(s => s.GetHistoryAsync(10), Times.Once);
        _mockCalculatorService.Verify(s => s.GetHistoryAsync(25), Times.Once);
        _mockCalculatorService.Verify(s => s.ClearHistoryAsync(), Times.Once);
    }

    #endregion

    #region Advanced Mock Scenarios

    [Fact]
    public async Task Calculate_WithCallback_ShouldExecuteCallback()
    {
        // Arrange
        var callbackExecuted = false;
        var request = new CalculationRequest { FirstOperand = 1, SecondOperand = 1, Operation = "+" };
        var expectedResult = new CalculationResult { Result = 2 };

        _mockCalculatorService
            .Setup(s => s.CalculateAsync(It.IsAny<CalculationRequest>()))
            .Callback<CalculationRequest>(r => callbackExecuted = true)
            .ReturnsAsync(expectedResult);

        // Act
        await _controller.Calculate(request);

        // Assert
        callbackExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task Calculate_WithSequentialResults_ShouldReturnInOrder()
    {
        // Arrange
        var request = new CalculationRequest { FirstOperand = 1, SecondOperand = 1, Operation = "+" };
        var results = new[]
        {
            new CalculationResult { Result = 2 },
            new CalculationResult { Result = 4 },
            new CalculationResult { Result = 6 }
        };

        _mockCalculatorService
            .SetupSequence(s => s.CalculateAsync(It.IsAny<CalculationRequest>()))
            .ReturnsAsync(results[0])
            .ReturnsAsync(results[1])
            .ReturnsAsync(results[2]);

        // Act
        var result1 = await _controller.Calculate(request);
        var result2 = await _controller.Calculate(request);
        var result3 = await _controller.Calculate(request);

        // Assert
        ((result1.Result as OkObjectResult)!.Value as CalculationResult)!.Result.Should().Be(2);
        ((result2.Result as OkObjectResult)!.Value as CalculationResult)!.Result.Should().Be(4);
        ((result3.Result as OkObjectResult)!.Value as CalculationResult)!.Result.Should().Be(6);
    }

    [Fact]
    public void Divide_WithConditionalMock_ShouldBehaveDifferently()
    {
        // Arrange
        _mockCalculatorService
            .Setup(s => s.Divide(It.IsAny<double>(), It.Is<double>(b => b == 0)))
            .Throws<DivideByZeroException>();

        _mockCalculatorService
            .Setup(s => s.Divide(It.IsAny<double>(), It.Is<double>(b => b != 0)))
            .Returns<double, double>((a, b) => a / b);

        // Act & Assert - Test division by zero
        var divideByZeroResult = _controller.Divide(10, 0);
        divideByZeroResult.Result.Should().BeOfType<BadRequestObjectResult>();

        // Test normal division
        var result = _controller.Divide(10, 2);
        ((result.Result as OkObjectResult)!.Value).Should().Be(5);
    }

    #endregion
}