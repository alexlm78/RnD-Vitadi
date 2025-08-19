using ResilientClient.Api.Models;

namespace ResilientClient.Api.Services;

/// <summary>
/// Servicio para obtener noticias
/// </summary>
public interface INewsService
{
    /// <summary>
    /// Obtiene las últimas noticias
    /// </summary>
    /// <param name="category">Categoría de noticias (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de noticias</returns>
    Task<IEnumerable<NewsResponse>> GetLatestNewsAsync(string? category = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene noticias con reintentos automáticos
    /// </summary>
    /// <param name="category">Categoría de noticias (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de noticias</returns>
    Task<IEnumerable<NewsResponse>> GetNewsWithRetryAsync(string? category = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene noticias con patrones de resiliencia avanzados
    /// </summary>
    /// <param name="category">Categoría de noticias (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de noticias con manejo avanzado de errores</returns>
    Task<IEnumerable<NewsResponse>> GetNewsWithAdvancedResilienceAsync(string? category = null, CancellationToken cancellationToken = default);
}