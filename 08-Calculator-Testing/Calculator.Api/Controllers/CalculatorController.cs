using Calculator.Api.Models;
using Calculator.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Calculator.Api.Controllers;

/// <summary>
/// Controller for calculator operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CalculatorController : ControllerBase
{
    private readonly ICalculatorService _calculatorService;

    public CalculatorController(ICalculatorService calculatorService)
    {
        _calculatorService = calculatorService;
    }

    /// <summary>
    /// Performs a mathematical calculation
    /// </summary>
    /// <param name="request">The calculation request</param>
    /// <returns>The calculation result</returns>
    /// <response code="200">Returns the calculation result</response>
    /// <response code="400">If the request is invalid or division by zero</response>
    [HttpPost("calculate")]
    [ProducesResponseType(typeof(CalculationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CalculationResult>> Calculate([FromBody] CalculationRequest request)
    {
        try
        {
            var result = await _calculatorService.CalculateAsync(request);
            return Ok(result);
        }
        catch (DivideByZeroException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Adds two numbers
    /// </summary>
    /// <param name="a">First number</param>
    /// <param name="b">Second number</param>
    /// <returns>Sum of the two numbers</returns>
    [HttpGet("add")]
    [ProducesResponseType(typeof(double), StatusCodes.Status200OK)]
    public ActionResult<double> Add([FromQuery] double a, [FromQuery] double b)
    {
        var result = _calculatorService.Add(a, b);
        return Ok(result);
    }

    /// <summary>
    /// Subtracts second number from first
    /// </summary>
    /// <param name="a">First number</param>
    /// <param name="b">Second number</param>
    /// <returns>Difference of the two numbers</returns>
    [HttpGet("subtract")]
    [ProducesResponseType(typeof(double), StatusCodes.Status200OK)]
    public ActionResult<double> Subtract([FromQuery] double a, [FromQuery] double b)
    {
        var result = _calculatorService.Subtract(a, b);
        return Ok(result);
    }

    /// <summary>
    /// Multiplies two numbers
    /// </summary>
    /// <param name="a">First number</param>
    /// <param name="b">Second number</param>
    /// <returns>Product of the two numbers</returns>
    [HttpGet("multiply")]
    [ProducesResponseType(typeof(double), StatusCodes.Status200OK)]
    public ActionResult<double> Multiply([FromQuery] double a, [FromQuery] double b)
    {
        var result = _calculatorService.Multiply(a, b);
        return Ok(result);
    }

    /// <summary>
    /// Divides first number by second
    /// </summary>
    /// <param name="a">First number</param>
    /// <param name="b">Second number</param>
    /// <returns>Quotient of the two numbers</returns>
    /// <response code="400">If division by zero</response>
    [HttpGet("divide")]
    [ProducesResponseType(typeof(double), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public ActionResult<double> Divide([FromQuery] double a, [FromQuery] double b)
    {
        try
        {
            var result = _calculatorService.Divide(a, b);
            return Ok(result);
        }
        catch (DivideByZeroException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Gets the calculation history
    /// </summary>
    /// <param name="limit">Maximum number of records to return (default: 50)</param>
    /// <returns>List of calculation history records</returns>
    [HttpGet("history")]
    [ProducesResponseType(typeof(IEnumerable<CalculationHistory>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CalculationHistory>>> GetHistory([FromQuery] int limit = 50)
    {
        var history = await _calculatorService.GetHistoryAsync(limit);
        return Ok(history);
    }

    /// <summary>
    /// Clears all calculation history
    /// </summary>
    /// <returns>Success message</returns>
    [HttpDelete("history")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<ActionResult<string>> ClearHistory()
    {
        await _calculatorService.ClearHistoryAsync();
        return Ok("History cleared successfully");
    }
}