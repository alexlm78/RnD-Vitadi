using Polly;
using Polly.Extensions.Http;
using Polly.Wrap;
using ResilientClient.Api.Configuration;
using ResilientClient.Api.Services;

namespace ResilientClient.Api.Extensions;

/// <summary>
/// Extensiones para configurar servicios con patrones de resiliencia
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configura los servicios HTTP con políticas de resiliencia usando Polly
    /// </summary>
    public static IServiceCollection AddResilientHttpServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurar opciones
        services.Configure<ResilienceOptions>(configuration.GetSection(ResilienceOptions.SectionName));
        services.Configure<ExternalApiOptions>(configuration.GetSection(ExternalApiOptions.SectionName));

        var resilienceOptions = configuration.GetSection(ResilienceOptions.SectionName).Get<ResilienceOptions>() ?? new ResilienceOptions();
        var externalApiOptions = configuration.GetSection(ExternalApiOptions.SectionName).Get<ExternalApiOptions>() ?? new ExternalApiOptions();

        // Configurar HttpClient para WeatherService con políticas combinadas
        services.AddHttpClient<IWeatherService, WeatherService>(client =>
        {
            client.BaseAddress = new Uri(externalApiOptions.Weather.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(externalApiOptions.Weather.TimeoutSeconds);
            
            // Agregar headers comunes
            client.DefaultRequestHeaders.Add("User-Agent", "ResilientClient/1.0");
            
            if (!string.IsNullOrEmpty(externalApiOptions.Weather.ApiKey))
            {
                client.DefaultRequestHeaders.Add("X-API-Key", externalApiOptions.Weather.ApiKey);
            }
        })
        .AddPolicyHandler(GetWeatherPolicyWrap(resilienceOptions));

        // Configurar HttpClient para NewsService con políticas combinadas
        services.AddHttpClient<INewsService, NewsService>(client =>
        {
            client.BaseAddress = new Uri(externalApiOptions.News.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(externalApiOptions.News.TimeoutSeconds);
            
            // Agregar headers comunes
            client.DefaultRequestHeaders.Add("User-Agent", "ResilientClient/1.0");
            
            if (!string.IsNullOrEmpty(externalApiOptions.News.ApiKey))
            {
                client.DefaultRequestHeaders.Add("X-API-Key", externalApiOptions.News.ApiKey);
            }
        })
        .AddPolicyHandler(GetNewsPolicyWrap(resilienceOptions));

        // Configurar HttpClient para ResilienceTestService
        services.AddHttpClient<IResilienceTestService, ResilienceTestService>(client =>
        {
            client.BaseAddress = new Uri("https://httpbin.org/"); // Servicio de prueba
            client.Timeout = TimeSpan.FromSeconds(externalApiOptions.Weather.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("User-Agent", "ResilientClient-Test/1.0");
        })
        .AddPolicyHandler(GetTestServicePolicyWrap(resilienceOptions));

        return services;
    }

    /// <summary>
    /// Política combinada para el servicio de clima con fallback
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetWeatherPolicyWrap(ResilienceOptions options)
    {
        var fallbackPolicy = GetWeatherFallbackPolicy();
        var retryPolicy = GetRetryPolicy(options, "Weather");
        var circuitBreakerPolicy = GetCircuitBreakerPolicy(options, "Weather");
        var timeoutPolicy = GetTimeoutPolicy(options);

        // Orden de políticas: Fallback -> CircuitBreaker -> Retry -> Timeout
        return Policy.WrapAsync(fallbackPolicy, circuitBreakerPolicy, retryPolicy, timeoutPolicy);
    }

    /// <summary>
    /// Política combinada para el servicio de noticias con fallback
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetNewsPolicyWrap(ResilienceOptions options)
    {
        var fallbackPolicy = GetNewsFallbackPolicy();
        var retryPolicy = GetRetryPolicy(options, "News");
        var circuitBreakerPolicy = GetCircuitBreakerPolicy(options, "News");
        var timeoutPolicy = GetTimeoutPolicy(options);

        // Orden de políticas: Fallback -> CircuitBreaker -> Retry -> Timeout
        return Policy.WrapAsync(fallbackPolicy, circuitBreakerPolicy, retryPolicy, timeoutPolicy);
    }

    /// <summary>
    /// Política de fallback para el servicio de clima
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetWeatherFallbackPolicy()
    {
        return Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .Or<TaskCanceledException>()
            .FallbackAsync(
                fallbackValue: new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("""
                    {
                        "name": "Ciudad Desconocida",
                        "main": {
                            "temp": 20.0,
                            "humidity": 50.0
                        },
                        "weather": [{
                            "description": "Datos no disponibles - usando fallback"
                        }],
                        "wind": {
                            "speed": 5.0
                        }
                    }
                    """, System.Text.Encoding.UTF8, "application/json")
                },
                onFallbackAsync: async (result, context) =>
                {
                    var logger = context.GetLogger();
                    if (result.Exception != null)
                    {
                        logger?.LogWarning("Ejecutando fallback para Weather debido a excepción: {Exception}", 
                            result.Exception.Message);
                    }
                    else
                    {
                        logger?.LogWarning("Ejecutando fallback para Weather debido a status code: {StatusCode}", 
                            result.Result?.StatusCode);
                    }
                    await Task.CompletedTask;
                });
    }

    /// <summary>
    /// Política de fallback para el servicio de noticias
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetNewsFallbackPolicy()
    {
        return Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .Or<TaskCanceledException>()
            .FallbackAsync(
                fallbackValue: new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("""
                    {
                        "status": "ok",
                        "totalResults": 2,
                        "articles": [
                            {
                                "title": "Servicio de noticias temporalmente no disponible",
                                "description": "Los datos de noticias no están disponibles en este momento. Este es contenido de fallback.",
                                "author": "Sistema",
                                "url": "https://example.com/fallback",
                                "publishedAt": "2024-01-01T00:00:00Z",
                                "source": {
                                    "name": "Fallback News"
                                }
                            },
                            {
                                "title": "Patrones de resiliencia en acción",
                                "description": "Este mensaje demuestra cómo los patrones de fallback proporcionan una experiencia degradada pero funcional.",
                                "author": "Polly",
                                "url": "https://example.com/resilience",
                                "publishedAt": "2024-01-01T00:00:00Z",
                                "source": {
                                    "name": "Resilience News"
                                }
                            }
                        ]
                    }
                    """, System.Text.Encoding.UTF8, "application/json")
                },
                onFallbackAsync: async (result, context) =>
                {
                    var logger = context.GetLogger();
                    if (result.Exception != null)
                    {
                        logger?.LogWarning("Ejecutando fallback para News debido a excepción: {Exception}", 
                            result.Exception.Message);
                    }
                    else
                    {
                        logger?.LogWarning("Ejecutando fallback para News debido a status code: {StatusCode}", 
                            result.Result?.StatusCode);
                    }
                    await Task.CompletedTask;
                });
    }

    /// <summary>
    /// Política de reintentos con backoff exponencial y jitter
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(ResilienceOptions options, string serviceName)
    {
        var random = new Random();
        
        return HttpPolicyExtensions
            .HandleTransientHttpError() // Maneja HttpRequestException y 5XX, 408 status codes
            .Or<TaskCanceledException>() // Maneja timeouts
            .WaitAndRetryAsync(
                retryCount: options.Retry.MaxRetries,
                sleepDurationProvider: retryAttempt =>
                {
                    // Backoff exponencial con jitter para evitar thundering herd
                    var baseDelay = options.Retry.BaseDelaySeconds * Math.Pow(2, retryAttempt - 1);
                    var jitter = random.NextDouble() * 0.1 * baseDelay; // 10% de jitter
                    var totalDelay = Math.Min(baseDelay + jitter, options.Retry.MaxDelaySeconds);
                    
                    return TimeSpan.FromSeconds(totalDelay);
                },
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    var logger = context.GetLogger();
                    context.Add($"{serviceName}_RetryCount", retryCount);
                    
                    if (outcome.Exception != null)
                    {
                        logger?.LogWarning("[{ServiceName}] Reintento {RetryCount}/{MaxRetries} después de {Delay}ms debido a: {Exception}",
                            serviceName, retryCount, options.Retry.MaxRetries, timespan.TotalMilliseconds, outcome.Exception.Message);
                    }
                    else
                    {
                        logger?.LogWarning("[{ServiceName}] Reintento {RetryCount}/{MaxRetries} después de {Delay}ms debido a status code: {StatusCode}",
                            serviceName, retryCount, options.Retry.MaxRetries, timespan.TotalMilliseconds, outcome.Result?.StatusCode);
                    }
                });
    }

    /// <summary>
    /// Política de circuit breaker avanzada
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(ResilienceOptions options, string serviceName)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TaskCanceledException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: options.CircuitBreaker.FailureThreshold,
                durationOfBreak: TimeSpan.FromSeconds(options.CircuitBreaker.DurationOfBreakSeconds),
                onBreak: (exception, duration) =>
                {
                    if (exception.Exception != null)
                    {
                        Console.WriteLine($"[{serviceName}] Circuit breaker ABIERTO por {duration.TotalSeconds}s debido a: {exception.Exception.Message}");
                    }
                    else
                    {
                        Console.WriteLine($"[{serviceName}] Circuit breaker ABIERTO por {duration.TotalSeconds}s debido a status code: {exception.Result?.StatusCode}");
                    }
                },
                onReset: () =>
                {
                    Console.WriteLine($"[{serviceName}] Circuit breaker CERRADO - las llamadas se reanudan normalmente");
                },
                onHalfOpen: () =>
                {
                    Console.WriteLine($"[{serviceName}] Circuit breaker SEMI-ABIERTO - probando llamadas");
                });
    }

    /// <summary>
    /// Política de timeout optimista
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(ResilienceOptions options)
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(
            timeout: TimeSpan.FromSeconds(options.Timeout.TimeoutSeconds),
            timeoutStrategy: Polly.Timeout.TimeoutStrategy.Optimistic,
            onTimeoutAsync: async (context, timeout, task) =>
            {
                var logger = context.GetLogger();
                logger?.LogWarning("Timeout de {TimeoutSeconds}s alcanzado para la operación", timeout.TotalSeconds);
                await Task.CompletedTask;
            });
    }

    /// <summary>
    /// Política de bulkhead para limitar concurrencia
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetBulkheadPolicy(ResilienceOptions options, string serviceName)
    {
        return Policy.BulkheadAsync<HttpResponseMessage>(
            maxParallelization: options.Bulkhead?.MaxParallelization ?? 10,
            maxQueuingActions: options.Bulkhead?.MaxQueuingActions ?? 20,
            onBulkheadRejectedAsync: async (context) =>
            {
                var logger = context.GetLogger();
                logger?.LogWarning("[{ServiceName}] Bulkhead rechazó la request - demasiadas operaciones concurrentes", serviceName);
                await Task.CompletedTask;
            });
    }

    /// <summary>
    /// Política combinada para el servicio de test de resiliencia
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetTestServicePolicyWrap(ResilienceOptions options)
    {
        var fallbackPolicy = GetTestServiceFallbackPolicy();
        var retryPolicy = GetRetryPolicy(options, "ResilienceTest");
        var circuitBreakerPolicy = GetCircuitBreakerPolicy(options, "ResilienceTest");
        var timeoutPolicy = GetTimeoutPolicy(options);

        // Orden de políticas: Fallback -> CircuitBreaker -> Retry -> Timeout
        return Policy.WrapAsync(fallbackPolicy, circuitBreakerPolicy, retryPolicy, timeoutPolicy);
    }

    /// <summary>
    /// Política de fallback para el servicio de test
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetTestServiceFallbackPolicy()
    {
        return Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .Or<TaskCanceledException>()
            .FallbackAsync(
                fallbackValue: new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("""
                    {
                        "message": "Fallback ejecutado para test de resiliencia",
                        "timestamp": "2024-01-01T00:00:00Z",
                        "source": "fallback",
                        "note": "Esta respuesta demuestra el patrón de fallback en acción"
                    }
                    """, System.Text.Encoding.UTF8, "application/json")
                },
                onFallbackAsync: async (result, context) =>
                {
                    var logger = context.GetLogger();
                    if (result.Exception != null)
                    {
                        logger?.LogWarning("Ejecutando fallback para ResilienceTest debido a excepción: {Exception}", 
                            result.Exception.Message);
                    }
                    else
                    {
                        logger?.LogWarning("Ejecutando fallback para ResilienceTest debido a status code: {StatusCode}", 
                            result.Result?.StatusCode);
                    }
                    await Task.CompletedTask;
                });
    }

    /// <summary>
    /// Extensión para obtener el logger del contexto de Polly
    /// </summary>
    private static ILogger? GetLogger(this Context context)
    {
        if (context.TryGetValue("logger", out var logger) && logger is ILogger loggerInstance)
        {
            return loggerInstance;
        }
        return null;
    }
}