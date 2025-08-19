namespace ResilientClient.Api.Configuration;

/// <summary>
/// Opciones de configuración para políticas de resiliencia
/// </summary>
public class ResilienceOptions
{
    public const string SectionName = "Resilience";

    /// <summary>
    /// Configuración de reintentos
    /// </summary>
    public RetryOptions Retry { get; set; } = new();

    /// <summary>
    /// Configuración de circuit breaker
    /// </summary>
    public CircuitBreakerOptions CircuitBreaker { get; set; } = new();

    /// <summary>
    /// Configuración de timeout
    /// </summary>
    public TimeoutOptions Timeout { get; set; } = new();

    /// <summary>
    /// Configuración de bulkhead (aislamiento de recursos)
    /// </summary>
    public BulkheadOptions? Bulkhead { get; set; } = new();
}

public class RetryOptions
{
    /// <summary>
    /// Número máximo de reintentos
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Delay base en segundos para backoff exponencial
    /// </summary>
    public double BaseDelaySeconds { get; set; } = 1.0;

    /// <summary>
    /// Delay máximo en segundos
    /// </summary>
    public double MaxDelaySeconds { get; set; } = 30.0;
}

public class CircuitBreakerOptions
{
    /// <summary>
    /// Número de fallos consecutivos antes de abrir el circuito
    /// </summary>
    public int FailureThreshold { get; set; } = 5;

    /// <summary>
    /// Duración en segundos que el circuito permanece abierto
    /// </summary>
    public double DurationOfBreakSeconds { get; set; } = 30.0;

    /// <summary>
    /// Número de llamadas de prueba cuando el circuito está semi-abierto
    /// </summary>
    public int SamplingDuration { get; set; } = 10;
}

public class TimeoutOptions
{
    /// <summary>
    /// Timeout en segundos para operaciones HTTP
    /// </summary>
    public double TimeoutSeconds { get; set; } = 30.0;
}

public class BulkheadOptions
{
    /// <summary>
    /// Número máximo de operaciones paralelas
    /// </summary>
    public int MaxParallelization { get; set; } = 10;

    /// <summary>
    /// Número máximo de operaciones en cola
    /// </summary>
    public int MaxQueuingActions { get; set; } = 20;
}