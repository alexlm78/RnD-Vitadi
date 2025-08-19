using ResilientClient.Api.Models;

namespace ResilientClient.Api.Services;

/// <summary>
/// Servicio para demostrar y probar patrones de resiliencia
/// </summary>
public interface IResilienceTestService
{
    /// <summary>
    /// Simula diferentes tipos de errores transitorios
    /// </summary>
    /// <param name="errorType">Tipo de error a simular</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado del test de resiliencia</returns>
    Task<ResilienceTestResult> SimulateTransientErrorAsync(string errorType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Demuestra el comportamiento del circuit breaker
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Estado del circuit breaker</returns>
    Task<CircuitBreakerStatus> GetCircuitBreakerStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Ejecuta múltiples requests para demostrar patrones de resiliencia
    /// </summary>
    /// <param name="requestCount">Número de requests a ejecutar</param>
    /// <param name="errorRate">Porcentaje de errores (0-100)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Estadísticas de las requests</returns>
    Task<ResilienceStatistics> ExecuteBulkRequestsAsync(int requestCount, int errorRate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Demuestra estrategias de fallback
    /// </summary>
    /// <param name="fallbackType">Tipo de fallback a demostrar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado con datos de fallback</returns>
    Task<FallbackResult> DemonstrateFallbackAsync(string fallbackType, CancellationToken cancellationToken = default);
}