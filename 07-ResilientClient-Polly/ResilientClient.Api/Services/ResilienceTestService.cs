using System.Diagnostics;
using ResilientClient.Api.Models;

namespace ResilientClient.Api.Services;

/// <summary>
/// Implementación del servicio para demostrar patrones de resiliencia
/// </summary>
public class ResilienceTestService : IResilienceTestService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ResilienceTestService> _logger;
    private static int _requestCounter = 0;
    private static int _circuitBreakerFailures = 0;
    private static DateTime _lastCircuitBreakerReset = DateTime.UtcNow;

    public ResilienceTestService(HttpClient httpClient, ILogger<ResilienceTestService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ResilienceTestResult> SimulateTransientErrorAsync(string errorType, CancellationToken cancellationToken = default)
    {
        var requestId = Interlocked.Increment(ref _requestCounter);
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("[Request {RequestId}] Simulando error transitorio: {ErrorType}", requestId, errorType);

        try
        {
            switch (errorType.ToLowerInvariant())
            {
                case "http500":
                    // Simular error HTTP 500
                    throw new HttpRequestException("Simulated HTTP 500 Internal Server Error", null, System.Net.HttpStatusCode.InternalServerError);

                case "http503":
                    // Simular error HTTP 503
                    throw new HttpRequestException("Simulated HTTP 503 Service Unavailable", null, System.Net.HttpStatusCode.ServiceUnavailable);

                case "timeout":
                    // Simular timeout
                    await Task.Delay(35000, cancellationToken); // Más que el timeout configurado
                    break;

                case "network":
                    // Simular error de red
                    throw new HttpRequestException("Simulated network connectivity error");

                case "dns":
                    // Simular error DNS
                    throw new HttpRequestException("Simulated DNS resolution error");

                case "intermittent":
                    // Error intermitente basado en el contador de requests
                    if (requestId % 3 != 0) // Falla 2 de cada 3 veces
                    {
                        throw new HttpRequestException($"Simulated intermittent failure (request {requestId})");
                    }
                    break;

                case "success":
                    // Simular éxito
                    await Task.Delay(100, cancellationToken); // Simular latencia normal
                    break;

                default:
                    throw new ArgumentException($"Tipo de error desconocido: {errorType}");
            }

            stopwatch.Stop();
            var result = new ResilienceTestResult
            {
                RequestId = requestId,
                ErrorType = errorType,
                Success = true,
                Duration = stopwatch.Elapsed,
                Message = $"Request {requestId} completada exitosamente",
                Timestamp = DateTime.UtcNow,
                RetryAttempts = 0 // Las políticas de Polly manejan esto automáticamente
            };

            _logger.LogInformation("[Request {RequestId}] Completada exitosamente en {Duration}ms", 
                requestId, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            _logger.LogWarning("[Request {RequestId}] Cancelada después de {Duration}ms", 
                requestId, stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Interlocked.Increment(ref _circuitBreakerFailures);
            
            var result = new ResilienceTestResult
            {
                RequestId = requestId,
                ErrorType = errorType,
                Success = false,
                Duration = stopwatch.Elapsed,
                Message = ex.Message,
                Timestamp = DateTime.UtcNow,
                RetryAttempts = 0, // Las políticas de Polly manejan esto
                Error = ex.GetType().Name
            };

            _logger.LogError(ex, "[Request {RequestId}] Falló después de {Duration}ms", 
                requestId, stopwatch.ElapsedMilliseconds);

            throw; // Re-throw para que las políticas de Polly puedan manejarlo
        }
    }

    public async Task<CircuitBreakerStatus> GetCircuitBreakerStatusAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken); // Simular operación async

        var timeSinceReset = DateTime.UtcNow - _lastCircuitBreakerReset;
        var isOpen = _circuitBreakerFailures >= 5 && timeSinceReset.TotalSeconds < 30;
        var isHalfOpen = _circuitBreakerFailures >= 5 && timeSinceReset.TotalSeconds >= 30 && timeSinceReset.TotalSeconds < 60;

        // Reset automático después de 60 segundos
        if (timeSinceReset.TotalSeconds >= 60)
        {
            _circuitBreakerFailures = 0;
            _lastCircuitBreakerReset = DateTime.UtcNow;
        }

        return new CircuitBreakerStatus
        {
            State = isOpen ? "Open" : isHalfOpen ? "HalfOpen" : "Closed",
            FailureCount = _circuitBreakerFailures,
            LastFailureTime = _lastCircuitBreakerReset,
            NextAttemptTime = _lastCircuitBreakerReset.AddSeconds(30),
            IsHealthy = !isOpen,
            Description = GetCircuitBreakerDescription(isOpen, isHalfOpen)
        };
    }

    public async Task<ResilienceStatistics> ExecuteBulkRequestsAsync(int requestCount, int errorRate, CancellationToken cancellationToken = default)
    {
        if (requestCount <= 0 || requestCount > 100)
            throw new ArgumentException("Request count debe estar entre 1 y 100");
        
        if (errorRate < 0 || errorRate > 100)
            throw new ArgumentException("Error rate debe estar entre 0 y 100");

        _logger.LogInformation("Ejecutando {RequestCount} requests con {ErrorRate}% de error rate", requestCount, errorRate);

        var statistics = new ResilienceStatistics
        {
            TotalRequests = requestCount,
            ExpectedErrorRate = errorRate,
            StartTime = DateTime.UtcNow
        };

        var tasks = new List<Task<ResilienceTestResult>>();
        var random = new Random();

        for (int i = 0; i < requestCount; i++)
        {
            var shouldFail = random.Next(1, 101) <= errorRate;
            var errorType = shouldFail ? GetRandomErrorType(random) : "success";
            
            // Agregar delay aleatorio para simular carga real
            var delay = random.Next(10, 100);
            await Task.Delay(delay, cancellationToken);

            tasks.Add(ExecuteRequestWithStatistics(errorType, cancellationToken));
        }

        var results = await Task.WhenAll(tasks);
        
        statistics.EndTime = DateTime.UtcNow;
        statistics.Duration = statistics.EndTime - statistics.StartTime;
        statistics.SuccessfulRequests = results.Count(r => r.Success);
        statistics.FailedRequests = results.Count(r => !r.Success);
        statistics.ActualErrorRate = (double)statistics.FailedRequests / statistics.TotalRequests * 100;
        statistics.AverageResponseTime = TimeSpan.FromMilliseconds(results.Average(r => r.Duration.TotalMilliseconds));
        statistics.MaxResponseTime = results.Max(r => r.Duration);
        statistics.MinResponseTime = results.Min(r => r.Duration);

        _logger.LogInformation("Bulk requests completadas: {Success}/{Total} exitosas ({ErrorRate:F1}% error rate real)", 
            statistics.SuccessfulRequests, statistics.TotalRequests, statistics.ActualErrorRate);

        return statistics;
    }

    public async Task<FallbackResult> DemonstrateFallbackAsync(string fallbackType, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Demostrando fallback tipo: {FallbackType}", fallbackType);

        try
        {
            // Simular fallo que activará fallback
            switch (fallbackType.ToLowerInvariant())
            {
                case "cache":
                    throw new HttpRequestException("Simulated failure to demonstrate cache fallback");
                
                case "default":
                    throw new HttpRequestException("Simulated failure to demonstrate default value fallback");
                
                case "alternative":
                    throw new HttpRequestException("Simulated failure to demonstrate alternative service fallback");
                
                default:
                    throw new ArgumentException($"Tipo de fallback desconocido: {fallbackType}");
            }
        }
        catch (HttpRequestException ex)
        {
            // Simular estrategia de fallback
            await Task.Delay(50, cancellationToken); // Simular latencia de fallback
            
            return new FallbackResult
            {
                FallbackType = fallbackType,
                OriginalError = ex.Message,
                FallbackData = GetFallbackData(fallbackType),
                FallbackExecuted = true,
                Timestamp = DateTime.UtcNow,
                Message = $"Fallback '{fallbackType}' ejecutado exitosamente debido a: {ex.Message}"
            };
        }
    }

    private async Task<ResilienceTestResult> ExecuteRequestWithStatistics(string errorType, CancellationToken cancellationToken)
    {
        try
        {
            return await SimulateTransientErrorAsync(errorType, cancellationToken);
        }
        catch (Exception ex)
        {
            // Para estadísticas, capturamos el error pero no lo re-lanzamos
            return new ResilienceTestResult
            {
                RequestId = _requestCounter,
                ErrorType = errorType,
                Success = false,
                Duration = TimeSpan.Zero,
                Message = ex.Message,
                Timestamp = DateTime.UtcNow,
                Error = ex.GetType().Name
            };
        }
    }

    private static string GetRandomErrorType(Random random)
    {
        var errorTypes = new[] { "http500", "http503", "network", "timeout", "intermittent" };
        return errorTypes[random.Next(errorTypes.Length)];
    }

    private static string GetCircuitBreakerDescription(bool isOpen, bool isHalfOpen)
    {
        if (isOpen)
            return "Circuit breaker está ABIERTO - todas las requests fallan inmediatamente";
        if (isHalfOpen)
            return "Circuit breaker está SEMI-ABIERTO - probando requests limitadas";
        return "Circuit breaker está CERRADO - operación normal";
    }

    private static object GetFallbackData(string fallbackType)
    {
        return fallbackType.ToLowerInvariant() switch
        {
            "cache" => new { 
                Source = "Cache", 
                Data = "Datos en caché del último request exitoso",
                CacheAge = "5 minutos"
            },
            "default" => new { 
                Source = "Default Values", 
                Data = "Valores por defecto configurados",
                Note = "Experiencia degradada pero funcional"
            },
            "alternative" => new { 
                Source = "Alternative Service", 
                Data = "Datos de servicio alternativo",
                Provider = "Backup API"
            },
            _ => new { Source = "Unknown", Data = "Fallback genérico" }
        };
    }
}