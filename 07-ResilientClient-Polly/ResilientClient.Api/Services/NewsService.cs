using System.Text.Json;
using ResilientClient.Api.Models;

namespace ResilientClient.Api.Services;

/// <summary>
/// Implementación del servicio de noticias con patrones de resiliencia
/// </summary>
public class NewsService : INewsService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NewsService> _logger;

    public NewsService(HttpClient httpClient, ILogger<NewsService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<NewsResponse>> GetLatestNewsAsync(string? category = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Obteniendo noticias para categoría: {Category}", category ?? "general");

            var endpoint = string.IsNullOrEmpty(category) 
                ? "/news/top-headlines" 
                : $"/news/top-headlines?category={category}";

            var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var externalResponse = JsonSerializer.Deserialize<ExternalNewsResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (externalResponse?.Articles != null)
                {
                    var newsResponses = externalResponse.Articles
                        .Select(MapToNewsResponse)
                        .ToList();

                    _logger.LogInformation("Se obtuvieron {Count} noticias", newsResponses.Count);
                    return newsResponses;
                }
            }

            _logger.LogWarning("No se pudieron obtener noticias. Status: {StatusCode}", response.StatusCode);
            return Enumerable.Empty<NewsResponse>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de red al obtener noticias");
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout al obtener noticias");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener noticias");
            throw;
        }
    }

    public async Task<IEnumerable<NewsResponse>> GetNewsWithRetryAsync(string? category = null, CancellationToken cancellationToken = default)
    {
        // Este método utilizará las políticas de Polly configuradas en el HttpClient
        // Los reintentos se manejan automáticamente por las políticas
        return await GetLatestNewsAsync(category, cancellationToken);
    }

    public async Task<IEnumerable<NewsResponse>> GetNewsWithAdvancedResilienceAsync(string? category = null, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        var requestId = Guid.NewGuid().ToString("N")[..8];
        
        try
        {
            _logger.LogInformation("[{RequestId}] Iniciando request de noticias para categoría: {Category} con patrones de resiliencia avanzados", 
                requestId, category ?? "general");

            var endpoint = string.IsNullOrEmpty(category) 
                ? "/news/top-headlines?country=us" 
                : $"/news/top-headlines?country=us&category={category}";

            // El HttpClient ya tiene configuradas las políticas de Polly
            var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            
            var duration = DateTime.UtcNow - startTime;
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var externalResponse = JsonSerializer.Deserialize<ExternalNewsResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (externalResponse?.Articles != null)
                {
                    var newsResponses = externalResponse.Articles
                        .Where(a => !string.IsNullOrEmpty(a.Title)) // Filtrar artículos sin título
                        .Select(MapToNewsResponse)
                        .ToList();

                    _logger.LogInformation("[{RequestId}] Se obtuvieron {Count} noticias en {Duration}ms", 
                        requestId, newsResponses.Count, duration.TotalMilliseconds);
                    return newsResponses;
                }
            }

            _logger.LogWarning("[{RequestId}] Respuesta no exitosa. Status: {StatusCode}, Duration: {Duration}ms", 
                requestId, response.StatusCode, duration.TotalMilliseconds);
            
            // Retornar noticias de fallback
            return await GetFallbackNewsData(category);
        }
        catch (HttpRequestException ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "[{RequestId}] Error de red al obtener noticias después de {Duration}ms", 
                requestId, duration.TotalMilliseconds);
            return await GetFallbackNewsData(category);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "[{RequestId}] Timeout al obtener noticias después de {Duration}ms", 
                requestId, duration.TotalMilliseconds);
            return await GetFallbackNewsData(category);
        }
        catch (TaskCanceledException ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "[{RequestId}] Operación cancelada después de {Duration}ms", 
                requestId, duration.TotalMilliseconds);
            throw; // Re-throw cancellation
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "[{RequestId}] Error inesperado al obtener noticias después de {Duration}ms", 
                requestId, duration.TotalMilliseconds);
            return await GetFallbackNewsData(category);
        }
    }

    private async Task<IEnumerable<NewsResponse>> GetFallbackNewsData(string? category)
    {
        // Simular latencia de fuente alternativa
        await Task.Delay(50);
        
        _logger.LogInformation("Usando noticias de fallback para categoría: {Category}", category ?? "general");
        
        var fallbackNews = new List<NewsResponse>
        {
            new()
            {
                Title = $"Servicio de noticias temporalmente no disponible - {category ?? "general"}",
                Description = "Los datos de noticias no están disponibles en este momento. Este es contenido de fallback que demuestra patrones de resiliencia.",
                Author = "Sistema de Fallback",
                Url = "https://example.com/fallback-news",
                PublishedAt = DateTime.UtcNow.AddMinutes(-30),
                Source = "Resilience News"
            },
            new()
            {
                Title = "Patrones de resiliencia con Polly en acción",
                Description = "Este mensaje demuestra cómo los patrones de fallback proporcionan una experiencia degradada pero funcional cuando los servicios externos fallan.",
                Author = "Polly Framework",
                Url = "https://github.com/App-vNext/Polly",
                PublishedAt = DateTime.UtcNow.AddHours(-1),
                Source = "Tech News"
            }
        };

        // Agregar noticias específicas por categoría
        if (!string.IsNullOrEmpty(category))
        {
            fallbackNews.Add(new NewsResponse
            {
                Title = $"Noticias de {category} - Modo de recuperación",
                Description = $"Contenido específico de fallback para la categoría {category}. Los patrones de resiliencia aseguran que la aplicación siga funcionando.",
                Author = "Fallback System",
                Url = $"https://example.com/fallback/{category}",
                PublishedAt = DateTime.UtcNow.AddMinutes(-15),
                Source = $"{category} Fallback"
            });
        }

        return fallbackNews;
    }

    private static NewsResponse MapToNewsResponse(Article article)
    {
        return new NewsResponse
        {
            Title = article.Title,
            Description = article.Description,
            Author = article.Author,
            Url = article.Url,
            PublishedAt = article.PublishedAt,
            Source = article.Source?.Name
        };
    }
}