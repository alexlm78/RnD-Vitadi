using Microsoft.AspNetCore.Mvc;
using ResilientClient.Api.Models;
using ResilientClient.Api.Services;

namespace ResilientClient.Api.Controllers;

/// <summary>
/// Controller consolidado que demuestra todos los patrones de resiliencia con APIs externas
/// </summary>
[ApiController]
[Route("api/external")]
[Produces("application/json")]
public class ExternalApiController : ControllerBase
{
    private readonly IWeatherService _weatherService;
    private readonly INewsService _newsService;
    private readonly IResilienceTestService _resilienceTestService;
    private readonly ILogger<ExternalApiController> _logger;

    public ExternalApiController(
        IWeatherService weatherService,
        INewsService newsService,
        IResilienceTestService resilienceTestService,
        ILogger<ExternalApiController> logger)
    {
        _weatherService = weatherService;
        _newsService = newsService;
        _resilienceTestService = resilienceTestService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene información consolidada de múltiples APIs externas con resiliencia completa
    /// </summary>
    /// <param name="city">Ciudad para información del clima</param>
    /// <param name="newsCategory">Categoría de noticias (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Información consolidada de clima y noticias</returns>
    /// <response code="200">Información obtenida exitosamente</response>
    /// <response code="400">Parámetros inválidos</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ExternalApiDashboard), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ExternalApiDashboard>> GetDashboard(
        [FromQuery] string city = "Madrid",
        [FromQuery] string? newsCategory = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return BadRequest("El parámetro 'city' es requerido");
        }

        _logger.LogInformation("Obteniendo dashboard para ciudad: {City}, categoría de noticias: {Category}", 
            city, newsCategory ?? "general");

        var dashboard = new ExternalApiDashboard
        {
            RequestId = Guid.NewGuid().ToString(),
            City = city,
            NewsCategory = newsCategory ?? "general",
            RequestTime = DateTime.UtcNow
        };

        // Ejecutar llamadas en paralelo con manejo independiente de errores
        var weatherTask = GetWeatherWithResilience(city, cancellationToken);
        var newsTask = GetNewsWithResilience(newsCategory, cancellationToken);
        var circuitBreakerTask = _resilienceTestService.GetCircuitBreakerStatusAsync(cancellationToken);

        await Task.WhenAll(weatherTask, newsTask, circuitBreakerTask);

        dashboard.Weather = await weatherTask;
        dashboard.News = await newsTask;
        dashboard.CircuitBreakerStatus = await circuitBreakerTask;
        dashboard.ResponseTime = DateTime.UtcNow - dashboard.RequestTime;

        _logger.LogInformation("Dashboard completado en {Duration}ms para {City}", 
            dashboard.ResponseTime.TotalMilliseconds, city);

        return Ok(dashboard);
    }

    /// <summary>
    /// Demuestra el patrón de retry con diferentes configuraciones
    /// </summary>
    /// <param name="maxRetries">Número máximo de reintentos (1-10)</param>
    /// <param name="baseDelayMs">Delay base en milisegundos (100-5000)</param>
    /// <param name="errorType">Tipo de error a simular</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado del test de retry</returns>
    /// <response code="200">Test de retry completado</response>
    /// <response code="400">Parámetros inválidos</response>
    [HttpPost("retry-demo")]
    [ProducesResponseType(typeof(RetryDemoResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RetryDemoResult>> DemonstrateRetryPattern(
        [FromQuery] int maxRetries = 3,
        [FromQuery] int baseDelayMs = 1000,
        [FromQuery] string errorType = "http500",
        CancellationToken cancellationToken = default)
    {
        if (maxRetries < 1 || maxRetries > 10)
        {
            return BadRequest("maxRetries debe estar entre 1 y 10");
        }

        if (baseDelayMs < 100 || baseDelayMs > 5000)
        {
            return BadRequest("baseDelayMs debe estar entre 100 y 5000");
        }

        _logger.LogInformation("Demostrando retry pattern: {MaxRetries} reintentos, {BaseDelay}ms delay, error: {ErrorType}", 
            maxRetries, baseDelayMs, errorType);

        var startTime = DateTime.UtcNow;
        var attempts = new List<RetryAttempt>();

        try
        {
            // Simular múltiples intentos para demostrar el patrón
            for (int attempt = 1; attempt <= maxRetries + 1; attempt++)
            {
                var attemptStart = DateTime.UtcNow;
                
                try
                {
                    var result = await _resilienceTestService.SimulateTransientErrorAsync(errorType, cancellationToken);
                    
                    attempts.Add(new RetryAttempt
                    {
                        AttemptNumber = attempt,
                        Success = true,
                        Duration = DateTime.UtcNow - attemptStart,
                        Message = "Intento exitoso",
                        Timestamp = attemptStart
                    });

                    break; // Éxito, salir del loop
                }
                catch (Exception ex)
                {
                    attempts.Add(new RetryAttempt
                    {
                        AttemptNumber = attempt,
                        Success = false,
                        Duration = DateTime.UtcNow - attemptStart,
                        Message = ex.Message,
                        Error = ex.GetType().Name,
                        Timestamp = attemptStart
                    });

                    if (attempt <= maxRetries)
                    {
                        var delay = Math.Min(baseDelayMs * Math.Pow(2, attempt - 1), 30000);
                        await Task.Delay((int)delay, cancellationToken);
                    }
                }
            }

            var totalDuration = DateTime.UtcNow - startTime;
            var finalSuccess = attempts.LastOrDefault()?.Success ?? false;

            return Ok(new RetryDemoResult
            {
                MaxRetries = maxRetries,
                BaseDelayMs = baseDelayMs,
                ErrorType = errorType,
                TotalAttempts = attempts.Count,
                FinalSuccess = finalSuccess,
                TotalDuration = totalDuration,
                Attempts = attempts,
                Summary = $"Completado en {attempts.Count} intentos, éxito final: {finalSuccess}"
            });
        }
        catch (OperationCanceledException)
        {
            return StatusCode(408, "Demo de retry cancelado");
        }
    }

    /// <summary>
    /// Demuestra el patrón de circuit breaker con múltiples requests
    /// </summary>
    /// <param name="requestCount">Número de requests a enviar (5-50)</param>
    /// <param name="failureRate">Porcentaje de fallos esperado (0-100)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado del test de circuit breaker</returns>
    /// <response code="200">Test de circuit breaker completado</response>
    /// <response code="400">Parámetros inválidos</response>
    [HttpPost("circuit-breaker-demo")]
    [ProducesResponseType(typeof(CircuitBreakerDemoResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CircuitBreakerDemoResult>> DemonstrateCircuitBreakerPattern(
        [FromQuery] int requestCount = 20,
        [FromQuery] int failureRate = 70,
        CancellationToken cancellationToken = default)
    {
        if (requestCount < 5 || requestCount > 50)
        {
            return BadRequest("requestCount debe estar entre 5 y 50");
        }

        if (failureRate < 0 || failureRate > 100)
        {
            return BadRequest("failureRate debe estar entre 0 y 100");
        }

        _logger.LogInformation("Demostrando circuit breaker: {RequestCount} requests con {FailureRate}% de fallos", 
            requestCount, failureRate);

        var startTime = DateTime.UtcNow;
        var requests = new List<CircuitBreakerRequest>();
        var random = new Random();

        for (int i = 1; i <= requestCount; i++)
        {
            var requestStart = DateTime.UtcNow;
            var shouldFail = random.Next(1, 101) <= failureRate;
            var errorType = shouldFail ? "http500" : "success";

            try
            {
                var result = await _resilienceTestService.SimulateTransientErrorAsync(errorType, cancellationToken);
                
                requests.Add(new CircuitBreakerRequest
                {
                    RequestNumber = i,
                    Success = true,
                    Duration = DateTime.UtcNow - requestStart,
                    Message = "Request exitosa",
                    Timestamp = requestStart,
                    CircuitState = "Closed" // Simplificado para demo
                });
            }
            catch (Exception ex)
            {
                requests.Add(new CircuitBreakerRequest
                {
                    RequestNumber = i,
                    Success = false,
                    Duration = DateTime.UtcNow - requestStart,
                    Message = ex.Message,
                    Error = ex.GetType().Name,
                    Timestamp = requestStart,
                    CircuitState = requests.Count(r => !r.Success) >= 5 ? "Open" : "Closed"
                });
            }

            // Pequeño delay entre requests
            await Task.Delay(100, cancellationToken);
        }

        var totalDuration = DateTime.UtcNow - startTime;
        var successCount = requests.Count(r => r.Success);
        var actualFailureRate = (double)(requestCount - successCount) / requestCount * 100;

        return Ok(new CircuitBreakerDemoResult
        {
            RequestCount = requestCount,
            ExpectedFailureRate = failureRate,
            ActualFailureRate = actualFailureRate,
            SuccessfulRequests = successCount,
            FailedRequests = requestCount - successCount,
            TotalDuration = totalDuration,
            Requests = requests,
            CircuitBreakerTriggered = requests.Any(r => r.CircuitState == "Open"),
            Summary = $"Circuit breaker {(requests.Any(r => r.CircuitState == "Open") ? "activado" : "no activado")} - {successCount}/{requestCount} exitosas"
        });
    }

    /// <summary>
    /// Demuestra diferentes estrategias de fallback con APIs reales
    /// </summary>
    /// <param name="strategy">Estrategia de fallback: cache, default, alternative, hybrid</param>
    /// <param name="forceError">Forzar error para activar fallback</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado de la estrategia de fallback</returns>
    /// <response code="200">Estrategia de fallback demostrada</response>
    /// <response code="400">Estrategia desconocida</response>
    [HttpGet("fallback-demo/{strategy}")]
    [ProducesResponseType(typeof(FallbackDemoResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FallbackDemoResult>> DemonstrateFallbackStrategies(
        string strategy,
        [FromQuery] bool forceError = true,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Demostrando fallback strategy: {Strategy}, forzar error: {ForceError}", 
            strategy, forceError);

        var startTime = DateTime.UtcNow;

        try
        {
            switch (strategy.ToLowerInvariant())
            {
                case "cache":
                    return await DemonstrateCacheFallback(forceError, startTime, cancellationToken);

                case "default":
                    return await DemonstrateDefaultFallback(forceError, startTime, cancellationToken);

                case "alternative":
                    return await DemonstrateAlternativeFallback(forceError, startTime, cancellationToken);

                case "hybrid":
                    return await DemonstrateHybridFallback(forceError, startTime, cancellationToken);

                default:
                    return BadRequest(new
                    {
                        Error = $"Estrategia desconocida: {strategy}",
                        AvailableStrategies = new[] { "cache", "default", "alternative", "hybrid" },
                        Description = "Use estas estrategias para demostrar diferentes patrones de fallback"
                    });
            }
        }
        catch (OperationCanceledException)
        {
            return StatusCode(408, "Demo de fallback cancelado");
        }
    }

    /// <summary>
    /// Obtiene métricas consolidadas de resiliencia de todas las APIs
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Métricas de resiliencia consolidadas</returns>
    /// <response code="200">Métricas obtenidas exitosamente</response>
    [HttpGet("resilience-metrics")]
    [ProducesResponseType(typeof(ResilienceMetrics), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResilienceMetrics>> GetResilienceMetrics(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo métricas de resiliencia consolidadas");

        var circuitBreakerStatus = await _resilienceTestService.GetCircuitBreakerStatusAsync(cancellationToken);

        var metrics = new ResilienceMetrics
        {
            Timestamp = DateTime.UtcNow,
            CircuitBreakerStatus = circuitBreakerStatus,
            Services = new Dictionary<string, ServiceMetrics>
            {
                ["Weather"] = new ServiceMetrics
                {
                    ServiceName = "Weather API",
                    IsHealthy = circuitBreakerStatus.IsHealthy,
                    LastCheckTime = DateTime.UtcNow,
                    ResponseTimeMs = 150, // Simulado
                    SuccessRate = 95.5,
                    ErrorRate = 4.5,
                    ActivePolicies = new[] { "Retry", "CircuitBreaker", "Timeout", "Fallback" }
                },
                ["News"] = new ServiceMetrics
                {
                    ServiceName = "News API",
                    IsHealthy = true,
                    LastCheckTime = DateTime.UtcNow,
                    ResponseTimeMs = 200, // Simulado
                    SuccessRate = 92.3,
                    ErrorRate = 7.7,
                    ActivePolicies = new[] { "Retry", "CircuitBreaker", "Timeout", "Fallback" }
                }
            },
            OverallHealth = new OverallHealthMetrics
            {
                IsHealthy = circuitBreakerStatus.IsHealthy,
                AverageResponseTime = 175,
                OverallSuccessRate = 93.9,
                TotalRequests = 1250, // Simulado
                FailedRequests = 76
            }
        };

        return Ok(metrics);
    }

    /// <summary>
    /// Ejecuta un test completo de todos los patrones de resiliencia
    /// </summary>
    /// <param name="testDurationSeconds">Duración del test en segundos (10-300)</param>
    /// <param name="requestsPerSecond">Requests por segundo (1-10)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado completo del test de resiliencia</returns>
    /// <response code="200">Test completo ejecutado</response>
    /// <response code="400">Parámetros inválidos</response>
    [HttpPost("comprehensive-test")]
    [ProducesResponseType(typeof(ComprehensiveTestResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ComprehensiveTestResult>> ExecuteComprehensiveTest(
        [FromQuery] int testDurationSeconds = 30,
        [FromQuery] int requestsPerSecond = 2,
        CancellationToken cancellationToken = default)
    {
        if (testDurationSeconds < 10 || testDurationSeconds > 300)
        {
            return BadRequest("testDurationSeconds debe estar entre 10 y 300");
        }

        if (requestsPerSecond < 1 || requestsPerSecond > 10)
        {
            return BadRequest("requestsPerSecond debe estar entre 1 y 10");
        }

        _logger.LogInformation("Ejecutando test comprehensivo: {Duration}s, {RPS} req/s", 
            testDurationSeconds, requestsPerSecond);

        var startTime = DateTime.UtcNow;
        var endTime = startTime.AddSeconds(testDurationSeconds);
        var results = new List<TestResult>();
        var requestCounter = 0;

        while (DateTime.UtcNow < endTime && !cancellationToken.IsCancellationRequested)
        {
            var batchTasks = new List<Task<TestResult>>();

            // Crear batch de requests
            for (int i = 0; i < requestsPerSecond; i++)
            {
                requestCounter++;
                batchTasks.Add(ExecuteSingleTestRequest(requestCounter, cancellationToken));
            }

            var batchResults = await Task.WhenAll(batchTasks);
            results.AddRange(batchResults);

            // Esperar hasta el siguiente segundo
            await Task.Delay(1000, cancellationToken);
        }

        var totalDuration = DateTime.UtcNow - startTime;
        var successCount = results.Count(r => r.Success);

        return Ok(new ComprehensiveTestResult
        {
            TestDurationSeconds = testDurationSeconds,
            RequestsPerSecond = requestsPerSecond,
            TotalRequests = results.Count,
            SuccessfulRequests = successCount,
            FailedRequests = results.Count - successCount,
            SuccessRate = (double)successCount / results.Count * 100,
            ActualDuration = totalDuration,
            AverageResponseTime = TimeSpan.FromMilliseconds(results.Average(r => r.Duration.TotalMilliseconds)),
            Results = results.Take(100).ToList(), // Limitar para evitar respuestas muy grandes
            Summary = $"Test completado: {successCount}/{results.Count} exitosas ({(double)successCount / results.Count * 100:F1}%)"
        });
    }

    // Métodos privados auxiliares

    private async Task<WeatherResponse?> GetWeatherWithResilience(string city, CancellationToken cancellationToken)
    {
        try
        {
            return await _weatherService.GetWeatherWithAdvancedResilienceAsync(city, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error obteniendo clima para {City}, usando fallback", city);
            return new WeatherResponse
            {
                Location = city,
                Temperature = 20.0,
                Description = "Datos no disponibles (fallback)",
                Humidity = 50.0,
                WindSpeed = 5.0,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    private async Task<IEnumerable<NewsResponse>> GetNewsWithResilience(string? category, CancellationToken cancellationToken)
    {
        try
        {
            return await _newsService.GetNewsWithAdvancedResilienceAsync(category, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error obteniendo noticias para categoría {Category}, usando fallback", category);
            return new[]
            {
                new NewsResponse
                {
                    Title = "Servicio de noticias no disponible",
                    Description = "Los datos de noticias no están disponibles en este momento (fallback)",
                    Author = "Sistema",
                    Url = "https://example.com/fallback",
                    PublishedAt = DateTime.UtcNow,
                    Source = "Fallback News"
                }
            };
        }
    }

    private async Task<ActionResult<FallbackDemoResult>> DemonstrateCacheFallback(bool forceError, DateTime startTime, CancellationToken cancellationToken)
    {
        var primaryData = forceError ? null : await GetPrimaryData(cancellationToken);
        var fallbackData = GetCachedData();

        return Ok(new FallbackDemoResult
        {
            Strategy = "cache",
            PrimarySuccess = !forceError,
            FallbackUsed = forceError,
            Duration = DateTime.UtcNow - startTime,
            Data = primaryData ?? fallbackData,
            Message = forceError ? "Datos obtenidos de caché debido a fallo del servicio primario" : "Datos obtenidos del servicio primario"
        });
    }

    private async Task<ActionResult<FallbackDemoResult>> DemonstrateDefaultFallback(bool forceError, DateTime startTime, CancellationToken cancellationToken)
    {
        var primaryData = forceError ? null : await GetPrimaryData(cancellationToken);
        var fallbackData = GetDefaultData();

        return Ok(new FallbackDemoResult
        {
            Strategy = "default",
            PrimarySuccess = !forceError,
            FallbackUsed = forceError,
            Duration = DateTime.UtcNow - startTime,
            Data = primaryData ?? fallbackData,
            Message = forceError ? "Usando valores por defecto debido a fallo del servicio" : "Datos obtenidos del servicio primario"
        });
    }

    private async Task<ActionResult<FallbackDemoResult>> DemonstrateAlternativeFallback(bool forceError, DateTime startTime, CancellationToken cancellationToken)
    {
        var primaryData = forceError ? null : await GetPrimaryData(cancellationToken);
        var fallbackData = await GetAlternativeServiceData(cancellationToken);

        return Ok(new FallbackDemoResult
        {
            Strategy = "alternative",
            PrimarySuccess = !forceError,
            FallbackUsed = forceError,
            Duration = DateTime.UtcNow - startTime,
            Data = primaryData ?? fallbackData,
            Message = forceError ? "Datos obtenidos de servicio alternativo" : "Datos obtenidos del servicio primario"
        });
    }

    private async Task<ActionResult<FallbackDemoResult>> DemonstrateHybridFallback(bool forceError, DateTime startTime, CancellationToken cancellationToken)
    {
        var primaryData = forceError ? null : await GetPrimaryData(cancellationToken);
        
        // Estrategia híbrida: intentar caché, luego alternativo, finalmente default
        object? fallbackData = null;
        string fallbackSource = "";

        if (forceError)
        {
            fallbackData = GetCachedData();
            if (fallbackData != null)
            {
                fallbackSource = "caché";
            }
            else
            {
                fallbackData = await GetAlternativeServiceData(cancellationToken);
                fallbackSource = fallbackData != null ? "servicio alternativo" : "valores por defecto";
                fallbackData ??= GetDefaultData();
            }
        }

        return Ok(new FallbackDemoResult
        {
            Strategy = "hybrid",
            PrimarySuccess = !forceError,
            FallbackUsed = forceError,
            Duration = DateTime.UtcNow - startTime,
            Data = primaryData ?? fallbackData,
            Message = forceError ? $"Datos obtenidos de {fallbackSource} usando estrategia híbrida" : "Datos obtenidos del servicio primario"
        });
    }

    private async Task<object?> GetPrimaryData(CancellationToken cancellationToken)
    {
        await Task.Delay(100, cancellationToken); // Simular latencia
        return new { Source = "Primary Service", Data = "Datos del servicio primario", Timestamp = DateTime.UtcNow };
    }

    private object? GetCachedData()
    {
        // Simular datos en caché (50% de probabilidad de tener datos)
        var random = new Random();
        if (random.Next(1, 3) == 1)
        {
            return new { Source = "Cache", Data = "Datos en caché", CacheAge = "5 minutos", Timestamp = DateTime.UtcNow.AddMinutes(-5) };
        }
        return null;
    }

    private object GetDefaultData()
    {
        return new { Source = "Default Values", Data = "Valores por defecto del sistema", Timestamp = DateTime.UtcNow };
    }

    private async Task<object?> GetAlternativeServiceData(CancellationToken cancellationToken)
    {
        await Task.Delay(200, cancellationToken); // Simular latencia del servicio alternativo
        
        // Simular servicio alternativo (70% de probabilidad de éxito)
        var random = new Random();
        if (random.Next(1, 11) <= 7)
        {
            return new { Source = "Alternative Service", Data = "Datos del servicio alternativo", Timestamp = DateTime.UtcNow };
        }
        return null;
    }

    private async Task<TestResult> ExecuteSingleTestRequest(int requestId, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        var random = new Random();
        var errorTypes = new[] { "success", "http500", "timeout", "intermittent" };
        var errorType = errorTypes[random.Next(errorTypes.Length)];

        try
        {
            var result = await _resilienceTestService.SimulateTransientErrorAsync(errorType, cancellationToken);
            
            return new TestResult
            {
                RequestId = requestId,
                Success = true,
                Duration = DateTime.UtcNow - startTime,
                ErrorType = errorType,
                Message = "Request exitosa",
                Timestamp = startTime
            };
        }
        catch (Exception ex)
        {
            return new TestResult
            {
                RequestId = requestId,
                Success = false,
                Duration = DateTime.UtcNow - startTime,
                ErrorType = errorType,
                Message = ex.Message,
                Error = ex.GetType().Name,
                Timestamp = startTime
            };
        }
    }
}

// Modelos adicionales para el ExternalApiController

public class ExternalApiDashboard
{
    public string RequestId { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string NewsCategory { get; set; } = string.Empty;
    public DateTime RequestTime { get; set; }
    public TimeSpan ResponseTime { get; set; }
    public WeatherResponse? Weather { get; set; }
    public IEnumerable<NewsResponse>? News { get; set; }
    public CircuitBreakerStatus? CircuitBreakerStatus { get; set; }
}

public class RetryDemoResult
{
    public int MaxRetries { get; set; }
    public int BaseDelayMs { get; set; }
    public string ErrorType { get; set; } = string.Empty;
    public int TotalAttempts { get; set; }
    public bool FinalSuccess { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public List<RetryAttempt> Attempts { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
}

public class RetryAttempt
{
    public int AttemptNumber { get; set; }
    public bool Success { get; set; }
    public TimeSpan Duration { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Error { get; set; }
    public DateTime Timestamp { get; set; }
}

public class CircuitBreakerDemoResult
{
    public int RequestCount { get; set; }
    public double ExpectedFailureRate { get; set; }
    public double ActualFailureRate { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public List<CircuitBreakerRequest> Requests { get; set; } = new();
    public bool CircuitBreakerTriggered { get; set; }
    public string Summary { get; set; } = string.Empty;
}

public class CircuitBreakerRequest
{
    public int RequestNumber { get; set; }
    public bool Success { get; set; }
    public TimeSpan Duration { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Error { get; set; }
    public DateTime Timestamp { get; set; }
    public string CircuitState { get; set; } = string.Empty;
}

public class FallbackDemoResult
{
    public string Strategy { get; set; } = string.Empty;
    public bool PrimarySuccess { get; set; }
    public bool FallbackUsed { get; set; }
    public TimeSpan Duration { get; set; }
    public object? Data { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ResilienceMetrics
{
    public DateTime Timestamp { get; set; }
    public CircuitBreakerStatus? CircuitBreakerStatus { get; set; }
    public Dictionary<string, ServiceMetrics> Services { get; set; } = new();
    public OverallHealthMetrics? OverallHealth { get; set; }
}

public class ServiceMetrics
{
    public string ServiceName { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
    public DateTime LastCheckTime { get; set; }
    public double ResponseTimeMs { get; set; }
    public double SuccessRate { get; set; }
    public double ErrorRate { get; set; }
    public string[] ActivePolicies { get; set; } = Array.Empty<string>();
}

public class OverallHealthMetrics
{
    public bool IsHealthy { get; set; }
    public double AverageResponseTime { get; set; }
    public double OverallSuccessRate { get; set; }
    public int TotalRequests { get; set; }
    public int FailedRequests { get; set; }
}

public class ComprehensiveTestResult
{
    public int TestDurationSeconds { get; set; }
    public int RequestsPerSecond { get; set; }
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public double SuccessRate { get; set; }
    public TimeSpan ActualDuration { get; set; }
    public TimeSpan AverageResponseTime { get; set; }
    public List<TestResult> Results { get; set; } = new();
    public string Summary { get; set; } = string.Empty;
}

public class TestResult
{
    public int RequestId { get; set; }
    public bool Success { get; set; }
    public TimeSpan Duration { get; set; }
    public string ErrorType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Error { get; set; }
    public DateTime Timestamp { get; set; }
}