namespace Calculator.Api.Models;

/// <summary>
/// Represents a calculation operation stored in the history
/// </summary>
public class CalculationHistory
{
    public int Id { get; set; }
    public double FirstOperand { get; set; }
    public double SecondOperand { get; set; }
    public string Operation { get; set; } = string.Empty;
    public double Result { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Expression { get; set; } = string.Empty;
}