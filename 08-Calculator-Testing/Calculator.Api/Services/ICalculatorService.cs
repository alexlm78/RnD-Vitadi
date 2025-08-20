using Calculator.Api.Models;

namespace Calculator.Api.Services;

/// <summary>
/// Interface for calculator operations and history management
/// </summary>
public interface ICalculatorService
{
    /// <summary>
    /// Performs a mathematical calculation and stores it in history
    /// </summary>
    /// <param name="request">The calculation request</param>
    /// <returns>The calculation result</returns>
    Task<CalculationResult> CalculateAsync(CalculationRequest request);

    /// <summary>
    /// Adds two numbers
    /// </summary>
    /// <param name="a">First operand</param>
    /// <param name="b">Second operand</param>
    /// <returns>Sum of a and b</returns>
    double Add(double a, double b);

    /// <summary>
    /// Subtracts second number from first
    /// </summary>
    /// <param name="a">First operand</param>
    /// <param name="b">Second operand</param>
    /// <returns>Difference of a and b</returns>
    double Subtract(double a, double b);

    /// <summary>
    /// Multiplies two numbers
    /// </summary>
    /// <param name="a">First operand</param>
    /// <param name="b">Second operand</param>
    /// <returns>Product of a and b</returns>
    double Multiply(double a, double b);

    /// <summary>
    /// Divides first number by second
    /// </summary>
    /// <param name="a">First operand</param>
    /// <param name="b">Second operand</param>
    /// <returns>Quotient of a and b</returns>
    /// <exception cref="DivideByZeroException">Thrown when b is zero</exception>
    double Divide(double a, double b);

    /// <summary>
    /// Gets the calculation history
    /// </summary>
    /// <param name="limit">Maximum number of records to return</param>
    /// <returns>List of calculation history records</returns>
    Task<IEnumerable<CalculationHistory>> GetHistoryAsync(int limit = 50);

    /// <summary>
    /// Clears all calculation history
    /// </summary>
    Task ClearHistoryAsync();
}