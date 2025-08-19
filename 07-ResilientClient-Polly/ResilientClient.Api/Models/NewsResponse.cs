namespace ResilientClient.Api.Models;

/// <summary>
/// Respuesta del servicio de noticias
/// </summary>
public class NewsResponse
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Author { get; set; }
    public string? Url { get; set; }
    public DateTime PublishedAt { get; set; }
    public string? Source { get; set; }
}

/// <summary>
/// Resultado de un test de resiliencia
/// </summary>
public class ResilienceTestResult
{
    public int RequestId { get; set; }
    public string ErrorType { get; set; } = string.Empty;
    public bool Success { get; set; }
    public TimeSpan Duration { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int RetryAttempts { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Estado del circuit breaker
/// </summary>
public class CircuitBreakerStatus
{
    public string State { get; set; } = string.Empty; // Open, Closed, HalfOpen
    public int FailureCount { get; set; }
    public DateTime LastFailureTime { get; set; }
    public DateTime NextAttemptTime { get; set; }
    public bool IsHealthy { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Estadísticas de múltiples requests
/// </summary>
public class ResilienceStatistics
{
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public double ExpectedErrorRate { get; set; }
    public double ActualErrorRate { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public TimeSpan AverageResponseTime { get; set; }
    public TimeSpan MaxResponseTime { get; set; }
    public TimeSpan MinResponseTime { get; set; }
}

/// <summary>
/// Resultado de estrategia de fallback
/// </summary>
public class FallbackResult
{
    public string FallbackType { get; set; } = string.Empty;
    public string OriginalError { get; set; } = string.Empty;
    public object? FallbackData { get; set; }
    public bool FallbackExecuted { get; set; }
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Respuesta de la API externa de noticias (simulada)
/// </summary>
public class ExternalNewsResponse
{
    public string? Status { get; set; }
    public int TotalResults { get; set; }
    public Article[]? Articles { get; set; }
}

public class Article
{
    public Source? Source { get; set; }
    public string? Author { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Url { get; set; }
    public DateTime PublishedAt { get; set; }
}

public class Source
{
    public string? Name { get; set; }
}