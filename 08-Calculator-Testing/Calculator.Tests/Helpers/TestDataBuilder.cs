using Calculator.Api.Models;

namespace Calculator.Tests.Helpers;

/// <summary>
/// Test data builder for creating test objects with fluent API
/// </summary>
public class TestDataBuilder
{
    public static CalculationRequestBuilder CalculationRequest() => new();
    public static CalculationHistoryBuilder CalculationHistory() => new();
}

/// <summary>
/// Builder for CalculationRequest test objects
/// </summary>
public class CalculationRequestBuilder
{
    private double _firstOperand = 10;
    private double _secondOperand = 5;
    private string _operation = "+";

    public CalculationRequestBuilder WithFirstOperand(double value)
    {
        _firstOperand = value;
        return this;
    }

    public CalculationRequestBuilder WithSecondOperand(double value)
    {
        _secondOperand = value;
        return this;
    }

    public CalculationRequestBuilder WithOperation(string operation)
    {
        _operation = operation;
        return this;
    }

    public CalculationRequestBuilder WithAddition() => WithOperation("+");
    public CalculationRequestBuilder WithSubtraction() => WithOperation("-");
    public CalculationRequestBuilder WithMultiplication() => WithOperation("*");
    public CalculationRequestBuilder WithDivision() => WithOperation("/");

    public CalculationRequest Build() => new()
    {
        FirstOperand = _firstOperand,
        SecondOperand = _secondOperand,
        Operation = _operation
    };
}

/// <summary>
/// Builder for CalculationHistory test objects
/// </summary>
public class CalculationHistoryBuilder
{
    private int _id = 1;
    private double _firstOperand = 10;
    private double _secondOperand = 5;
    private string _operation = "+";
    private double _result = 15;
    private DateTime _createdAt = DateTime.UtcNow;
    private string _expression = "10 + 5 = 15";

    public CalculationHistoryBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public CalculationHistoryBuilder WithFirstOperand(double value)
    {
        _firstOperand = value;
        UpdateExpression();
        return this;
    }

    public CalculationHistoryBuilder WithSecondOperand(double value)
    {
        _secondOperand = value;
        UpdateExpression();
        return this;
    }

    public CalculationHistoryBuilder WithOperation(string operation)
    {
        _operation = operation;
        UpdateResult();
        UpdateExpression();
        return this;
    }

    public CalculationHistoryBuilder WithResult(double result)
    {
        _result = result;
        UpdateExpression();
        return this;
    }

    public CalculationHistoryBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public CalculationHistoryBuilder WithExpression(string expression)
    {
        _expression = expression;
        return this;
    }

    public CalculationHistoryBuilder CreatedMinutesAgo(int minutes)
    {
        _createdAt = DateTime.UtcNow.AddMinutes(-minutes);
        return this;
    }

    private void UpdateResult()
    {
        _result = _operation switch
        {
            "+" => _firstOperand + _secondOperand,
            "-" => _firstOperand - _secondOperand,
            "*" => _firstOperand * _secondOperand,
            "/" => _firstOperand / _secondOperand,
            _ => _result
        };
    }

    private void UpdateExpression()
    {
        _expression = $"{_firstOperand} {_operation} {_secondOperand} = {_result}";
    }

    public CalculationHistory Build() => new()
    {
        Id = _id,
        FirstOperand = _firstOperand,
        SecondOperand = _secondOperand,
        Operation = _operation,
        Result = _result,
        CreatedAt = _createdAt,
        Expression = _expression
    };
}

/// <summary>
/// Extension methods for test assertions
/// </summary>
public static class TestExtensions
{
    public static void ShouldBeEquivalentToCalculationRequest(this CalculationResult result, CalculationRequest request, double expectedResult)
    {
        result.Should().NotBeNull();
        result.FirstOperand.Should().Be(request.FirstOperand);
        result.SecondOperand.Should().Be(request.SecondOperand);
        result.Operation.Should().Be(request.Operation);
        result.Result.Should().Be(expectedResult);
        result.Expression.Should().Be($"{request.FirstOperand} {request.Operation} {request.SecondOperand} = {expectedResult}");
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}