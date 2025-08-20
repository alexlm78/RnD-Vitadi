using System.ComponentModel.DataAnnotations;

namespace Calculator.Api.Models;

/// <summary>
/// Request model for calculator operations
/// </summary>
public class CalculationRequest
{
    [Required]
    public double FirstOperand { get; set; }
    
    [Required]
    public double SecondOperand { get; set; }
    
    [Required]
    [RegularExpression(@"^[\+\-\*\/]$", ErrorMessage = "Operation must be one of: +, -, *, /")]
    public string Operation { get; set; } = string.Empty;
}