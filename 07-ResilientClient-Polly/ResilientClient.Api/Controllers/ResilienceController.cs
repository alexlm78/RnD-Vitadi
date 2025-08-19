using Microsoft.AspNetCore.Mvc;
using ResilientClient.Api.Models;
using ResilientClient.Api.Services;

namespace ResilientClient.Api.Controllers;

/// <summary>
/// Controller para demostrar y probar patrones de resiliencia
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ResilienceController : ControllerBase
{
    private readonly IResilienceTestService _resilienceTestService;
    private readonly ILogger<ResilienceController> _logger;

    public ResilienceController(IResilienceTestService resilienceTestService, ILogger<ResilienceController> logger)
    {
        _resilienceTestService = resilienceTestService;
        _logger = logger;
    }

    /// <summary>
    /// Simula diferentes tipos de errores transitorios para demostrar patrones de retry
    /// </summary>
    /// <param name="errorType">Tipo de error: http500, http503, timeout, network, dns, intermittent, success</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado del test de resiliencia</returns>
    /// <response code="200">Test ejecutado exitosamente</response>
    /// <response code="400">Tipo de error inválido</response>
    /// <response code="500">Error simulado (manejado por políticas de resiliencia)</response>
    [HttpGet("transient-error/{errorType}")]
    [ProducesResponseType(typeof(ResilienceTestResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResilienceTestResult>> SimulateTransientError(
        string errorType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Iniciando simulación de error transitorio: {ErrorType}", errorType);

            var result = await _resilienceTestService.SimulateTransientErrorAsync(errorType, cancellationToken);
            
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Tipo de error inválido: {ErrorType}", errorType);
            return BadRequest(new { 
                Error = ex.Message,
                AvailableTypes = new[] { "http500", "http503", "timeout", "network", "dns", "intermittent", "success" },
                Description = "Use estos tipos para simular diferentes errores transitorios"
            });
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Simulación de error cancelada: {ErrorType}", errorType);
            return StatusCode(408, "Request cancelada");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en simulación de error transitorio: {ErrorType}", errorType);
            
            // Retornar información sobre el error para fines educativos
            return StatusCode(500, new {
                Message = "Error simulado capturado",
                ErrorType = errorType,
                Exception = ex.GetType().Name,
                Details = ex.Message,
                Note = "Este error fue manejado por las políticas de resiliencia configuradas"
            });
        }
    }

    /// <summary>
    /// Obtiene el estado actual del circuit breaker
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Estado del circuit breaker</returns>
    /// <response code="200">Estado obtenido exitosamente</response>
    [HttpGet("circuit-breaker/status")]
    [ProducesResponseType(typeof(CircuitBreakerStatus), StatusCodes.Status200OK)]
    public async Task<ActionResult<CircuitBreakerStatus>> GetCircuitBreakerStatus(
        CancellationToken cancellationToken = default)
    {
        var status = await _resilienceTestService.GetCircuitBreakerStatusAsync(cancellationToken);
        
        _logger.LogInformation("Circuit breaker status: {State} con {FailureCount} fallos", 
            status.State, status.FailureCount);
        
        return Ok(status);
    }

    /// <summary>
    /// Ejecuta múltiples requests para demostrar patrones de resiliencia bajo carga
    /// </summary>
    /// <param name="requestCount">Número de requests (1-100)</param>
    /// <param name="errorRate">Porcentaje de errores esperado (0-100)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Estadísticas de las requests ejecutadas</returns>
    /// <response code="200">Estadísticas generadas exitosamente</response>
    /// <response code="400">Parámetros inválidos</response>
    [HttpPost("bulk-test")]
    [ProducesResponseType(typeof(ResilienceStatistics), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResilienceStatistics>> ExecuteBulkTest(
        [FromQuery] int requestCount = 10,
        [FromQuery] int errorRate = 30,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Iniciando bulk test: {RequestCount} requests con {ErrorRate}% error rate", 
                requestCount, errorRate);

            var statistics = await _resilienceTestService.ExecuteBulkRequestsAsync(requestCount, errorRate, cancellationToken);
            
            _logger.LogInformation("Bulk test completado: {Success}/{Total} exitosas en {Duration}ms", 
                statistics.SuccessfulRequests, statistics.TotalRequests, statistics.Duration.TotalMilliseconds);
            
            return Ok(statistics);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { 
                Error = ex.Message,
                Constraints = new {
                    RequestCount = "1-100",
                    ErrorRate = "0-100"
                }
            });
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Bulk test cancelado");
            return StatusCode(408, "Bulk test cancelado");
        }
    }

    /// <summary>
    /// Demuestra diferentes estrategias de fallback
    /// </summary>
    /// <param name="fallbackType">Tipo de fallback: cache, default, alternative</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado con datos de fallback</returns>
    /// <response code="200">Fallback ejecutado exitosamente</response>
    /// <response code="400">Tipo de fallback inválido</response>
    [HttpGet("fallback/{fallbackType}")]
    [ProducesResponseType(typeof(FallbackResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FallbackResult>> DemonstrateFallback(
        string fallbackType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Demostrando fallback: {FallbackType}", fallbackType);

            var result = await _resilienceTestService.DemonstrateFallbackAsync(fallbackType, cancellationToken);
            
            _logger.LogInformation("Fallback {FallbackType} ejecutado exitosamente", fallbackType);
            
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { 
                Error = ex.Message,
                AvailableTypes = new[] { "cache", "default", "alternative" },
                Description = "Use estos tipos para demostrar diferentes estrategias de fallback"
            });
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Demostración de fallback cancelada: {FallbackType}", fallbackType);
            return StatusCode(408, "Request cancelada");
        }
    }

    /// <summary>
    /// Obtiene información sobre los patrones de resiliencia implementados
    /// </summary>
    /// <returns>Información sobre patrones de resiliencia</returns>
    /// <response code="200">Información obtenida exitosamente</response>
    [HttpGet("patterns")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public ActionResult GetResiliencePatterns()
    {
        var patterns = new
        {
            Retry = new
            {
                Description = "Reintenta operaciones fallidas con backoff exponencial y jitter",
                Configuration = new
                {
                    MaxRetries = 3,
                    BaseDelay = "1 segundo",
                    MaxDelay = "30 segundos",
                    Jitter = "10% del delay base"
                },
                HandledErrors = new[] { "HttpRequestException", "TaskCanceledException", "HTTP 5XX", "HTTP 408" }
            },
            CircuitBreaker = new
            {
                Description = "Previene llamadas a servicios que fallan consistentemente",
                Configuration = new
                {
                    FailureThreshold = "50% de fallos",
                    SamplingDuration = "10 segundos",
                    MinimumThroughput = "3 requests",
                    DurationOfBreak = "30 segundos"
                },
                States = new[] { "Closed (normal)", "Open (failing)", "HalfOpen (testing)" }
            },
            Timeout = new
            {
                Description = "Limita el tiempo máximo de espera para operaciones",
                Configuration = new
                {
                    TimeoutSeconds = 30,
                    Strategy = "Optimistic"
                }
            },
            Fallback = new
            {
                Description = "Proporciona respuestas alternativas cuando fallan las operaciones principales",
                Strategies = new[] { "Cache", "Default values", "Alternative service" }
            },
            Bulkhead = new
            {
                Description = "Aísla recursos para prevenir fallos en cascada",
                Configuration = new
                {
                    MaxParallelization = 10,
                    MaxQueuingActions = 20
                }
            }
        };

        return Ok(new
        {
            Title = "Patrones de Resiliencia Implementados",
            Description = "Esta API demuestra patrones de resiliencia usando Polly",
            Patterns = patterns,
            TestEndpoints = new
            {
                TransientErrors = "/api/resilience/transient-error/{errorType}",
                CircuitBreakerStatus = "/api/resilience/circuit-breaker/status",
                BulkTest = "/api/resilience/bulk-test?requestCount=10&errorRate=30",
                Fallback = "/api/resilience/fallback/{fallbackType}"
            },
            Documentation = "Consulte el README.md para ejemplos detallados de uso"
        });
    }
}