using ResilientClient.Api.Models;

namespace ResilientClient.Api.Services;

/// <summary>
/// Servicio para obtener información del clima
/// </summary>
public interface IWeatherService
{
    /// <summary>
    /// Obtiene el clima actual para una ciudad
    /// </summary>
    /// <param name="city">Nombre de la ciudad</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Información del clima</returns>
    Task<WeatherResponse?> GetCurrentWeatherAsync(string city, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el clima con fallback a datos en caché
    /// </summary>
    /// <param name="city">Nombre de la ciudad</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Información del clima o datos en caché</returns>
    Task<WeatherResponse?> GetWeatherWithFallbackAsync(string city, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene el clima con patrones de resiliencia avanzados
    /// </summary>
    /// <param name="city">Nombre de la ciudad</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Información del clima con manejo avanzado de errores</returns>
    Task<WeatherResponse?> GetWeatherWithAdvancedResilienceAsync(string city, CancellationToken cancellationToken = default);
}