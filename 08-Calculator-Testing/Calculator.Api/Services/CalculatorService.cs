using Calculator.Api.Data;
using Calculator.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Calculator.Api.Services;

/// <summary>
/// Service for performing calculator operations and managing history
/// </summary>
public class CalculatorService : ICalculatorService
{
    private readonly CalculatorDbContext _context;

    public CalculatorService(CalculatorDbContext context)
    {
        _context = context;
    }

    public async Task<CalculationResult> CalculateAsync(CalculationRequest request)
    {
        double result = request.Operation switch
        {
            "+" => Add(request.FirstOperand, request.SecondOperand),
            "-" => Subtract(request.FirstOperand, request.SecondOperand),
            "*" => Multiply(request.FirstOperand, request.SecondOperand),
            "/" => Divide(request.FirstOperand, request.SecondOperand),
            _ => throw new ArgumentException($"Unsupported operation: {request.Operation}")
        };

        var expression = $"{request.FirstOperand} {request.Operation} {request.SecondOperand} = {result}";
        var timestamp = DateTime.UtcNow;

        // Store in history
        var historyEntry = new CalculationHistory
        {
            FirstOperand = request.FirstOperand,
            SecondOperand = request.SecondOperand,
            Operation = request.Operation,
            Result = result,
            Expression = expression,
            CreatedAt = timestamp
        };

        _context.CalculationHistory.Add(historyEntry);
        await _context.SaveChangesAsync();

        return new CalculationResult
        {
            FirstOperand = request.FirstOperand,
            SecondOperand = request.SecondOperand,
            Operation = request.Operation,
            Result = result,
            Expression = expression,
            Timestamp = timestamp
        };
    }

    public double Add(double a, double b)
    {
        return a + b;
    }

    public double Subtract(double a, double b)
    {
        return a - b;
    }

    public double Multiply(double a, double b)
    {
        return a * b;
    }

    public double Divide(double a, double b)
    {
        if (b == 0)
        {
            throw new DivideByZeroException("Cannot divide by zero");
        }
        return a / b;
    }

    public async Task<IEnumerable<CalculationHistory>> GetHistoryAsync(int limit = 50)
    {
        return await _context.CalculationHistory
            .OrderByDescending(h => h.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task ClearHistoryAsync()
    {
        _context.CalculationHistory.RemoveRange(_context.CalculationHistory);
        await _context.SaveChangesAsync();
    }
}