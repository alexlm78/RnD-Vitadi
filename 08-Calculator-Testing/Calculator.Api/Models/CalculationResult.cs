namespace Calculator.Api.Models;

/// <summary>
/// Response model for calculator operations
/// </summary>
public class CalculationResult
{
    public double FirstOperand { get; set; }
    public double SecondOperand { get; set; }
    public string Operation { get; set; } = string.Empty;
    public double Result { get; set; }
    public string Expression { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}