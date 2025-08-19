using Microsoft.AspNetCore.Mvc;
using ResilientClient.Api.Models;
using ResilientClient.Api.Services;

namespace ResilientClient.Api.Controllers;

/// <summary>
/// Controller para operaciones relacionadas con noticias
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class NewsController : ControllerBase
{
    private readonly INewsService _newsService;
    private readonly ILogger<NewsController> _logger;

    public NewsController(INewsService newsService, ILogger<NewsController> logger)
    {
        _newsService = newsService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene las últimas noticias
    /// </summary>
    /// <param name="category">Categoría de noticias (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de noticias</returns>
    /// <response code="200">Noticias obtenidas exitosamente</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("latest")]
    [ProducesResponseType(typeof(IEnumerable<NewsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<NewsResponse>>> GetLatestNews(
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Solicitando noticias para categoría: {Category}", category ?? "general");

            var news = await _newsService.GetLatestNewsAsync(category, cancellationToken);
            
            _logger.LogInformation("Se obtuvieron {Count} noticias", news.Count());
            return Ok(news);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de conectividad al obtener noticias");
            return StatusCode(503, "Servicio de noticias temporalmente no disponible");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout al obtener noticias");
            return StatusCode(408, "Timeout al obtener noticias");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener noticias");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene noticias con reintentos automáticos
    /// </summary>
    /// <param name="category">Categoría de noticias (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de noticias con reintentos automáticos</returns>
    /// <response code="200">Noticias obtenidas exitosamente</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("resilient")]
    [ProducesResponseType(typeof(IEnumerable<NewsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<NewsResponse>>> GetResilientNews(
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Solicitando noticias resilientes para categoría: {Category}", category ?? "general");

            var news = await _newsService.GetNewsWithRetryAsync(category, cancellationToken);
            
            _logger.LogInformation("Se obtuvieron {Count} noticias con resiliencia", news.Count());
            return Ok(news);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico al obtener noticias resilientes");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene noticias por categorías específicas
    /// </summary>
    /// <param name="categories">Lista de categorías separadas por coma</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Noticias agrupadas por categoría</returns>
    /// <response code="200">Noticias obtenidas por categorías</response>
    /// <response code="400">Lista de categorías inválida</response>
    [HttpGet("by-categories")]
    [ProducesResponseType(typeof(Dictionary<string, IEnumerable<NewsResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Dictionary<string, IEnumerable<NewsResponse>>>> GetNewsByCategories(
        [FromQuery] string categories,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(categories))
        {
            return BadRequest("Debe proporcionar al menos una categoría");
        }

        var categoryList = categories.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Trim().ToLowerInvariant())
            .Where(c => !string.IsNullOrEmpty(c))
            .Distinct()
            .ToList();

        if (!categoryList.Any())
        {
            return BadRequest("Lista de categorías inválida");
        }

        _logger.LogInformation("Solicitando noticias para {Count} categorías: {Categories}", 
            categoryList.Count, string.Join(", ", categoryList));

        var result = new Dictionary<string, IEnumerable<NewsResponse>>();

        foreach (var category in categoryList)
        {
            try
            {
                var news = await _newsService.GetNewsWithRetryAsync(category, cancellationToken);
                result[category] = news;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudieron obtener noticias para categoría: {Category}", category);
                result[category] = Enumerable.Empty<NewsResponse>();
            }
        }

        var totalNews = result.Values.Sum(news => news.Count());
        _logger.LogInformation("Se obtuvieron {TotalNews} noticias en total para {Categories} categorías", 
            totalNews, categoryList.Count);

        return Ok(result);
    }

    /// <summary>
    /// Obtiene las categorías disponibles
    /// </summary>
    /// <returns>Lista de categorías disponibles</returns>
    /// <response code="200">Categorías disponibles</response>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<string>> GetAvailableCategories()
    {
        var categories = new[]
        {
            "general",
            "business",
            "entertainment",
            "health",
            "science",
            "sports",
            "technology"
        };

        return Ok(categories);
    }

    /// <summary>
    /// Obtiene noticias con patrones de resiliencia avanzados (retry, circuit breaker, timeout, fallback)
    /// </summary>
    /// <param name="category">Categoría de noticias (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de noticias con manejo avanzado de errores</returns>
    /// <response code="200">Noticias obtenidas con patrones de resiliencia</response>
    /// <response code="400">Parámetros inválidos</response>
    /// <response code="499">Request cancelada por el cliente</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("advanced")]
    [ProducesResponseType(typeof(IEnumerable<NewsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<NewsResponse>>> GetNewsWithAdvancedResilience(
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Solicitando noticias con resiliencia avanzada para categoría: {Category}", category ?? "general");

            var news = await _newsService.GetNewsWithAdvancedResilienceAsync(category, cancellationToken);
            
            // Con los patrones de resiliencia, siempre deberíamos obtener algún resultado
            // (ya sea datos frescos o de fallback)
            _logger.LogInformation("Se obtuvieron {Count} noticias con resiliencia avanzada", news.Count());
            return Ok(news);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Request de noticias cancelada por el cliente para categoría: {Category}", category);
            return StatusCode(499, "Request cancelada por el cliente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener noticias avanzadas para categoría: {Category}", category);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Endpoint para probar diferentes escenarios de fallo en el servicio de noticias
    /// </summary>
    /// <param name="scenario">Escenario de prueba: timeout, error, slow, intermittent, circuitbreaker</param>
    /// <param name="category">Categoría de noticias para el test (opcional)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Respuesta según el escenario para demostrar patrones de resiliencia</returns>
    /// <response code="200">Escenario ejecutado exitosamente</response>
    /// <response code="400">Escenario desconocido</response>
    /// <response code="408">Request timeout</response>
    /// <response code="500">Error simulado</response>
    [HttpGet("test/{scenario}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status408RequestTimeout)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> TestNewsResilienceScenario(
        string scenario,
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Probando escenario de resiliencia de noticias: {Scenario} para categoría: {Category}", 
            scenario, category ?? "general");

        try
        {
            switch (scenario.ToLowerInvariant())
            {
                case "timeout":
                    // Simular timeout (más que el timeout configurado)
                    _logger.LogInformation("Simulando timeout de 35 segundos para noticias...");
                    await Task.Delay(35000, cancellationToken);
                    return Ok(new { 
                        Message = "Este mensaje no debería aparecer debido al timeout",
                        Category = category ?? "general"
                    });

                case "error":
                    // Simular error HTTP
                    _logger.LogInformation("Simulando error de red para noticias...");
                    throw new HttpRequestException("Error simulado de API de noticias para demostrar retry policy");

                case "slow":
                    // Simular respuesta lenta pero exitosa
                    _logger.LogInformation("Simulando respuesta lenta de 3 segundos para noticias...");
                    await Task.Delay(3000, cancellationToken);
                    return Ok(new { 
                        Message = "Respuesta lenta pero exitosa del servicio de noticias", 
                        Scenario = scenario,
                        Category = category ?? "general",
                        Timestamp = DateTime.UtcNow,
                        Duration = "3000ms"
                    });

                case "intermittent":
                    // Simular fallo intermitente (25% de probabilidad de éxito)
                    var random = new Random();
                    var success = random.Next(1, 5) == 1;
                    _logger.LogInformation("Simulando fallo intermitente de noticias - Éxito: {Success}", success);
                    
                    if (success)
                    {
                        return Ok(new { 
                            Message = "Éxito intermitente en servicio de noticias - esta vez funcionó", 
                            Scenario = scenario,
                            Category = category ?? "general",
                            Timestamp = DateTime.UtcNow,
                            Attempt = "Exitoso"
                        });
                    }
                    throw new HttpRequestException("Fallo intermitente simulado en API de noticias - reintente para ver retry policy");

                case "circuitbreaker":
                    // Simular múltiples fallos para activar circuit breaker
                    _logger.LogInformation("Simulando fallo para activar circuit breaker de noticias...");
                    throw new HttpRequestException("Fallo simulado para activar circuit breaker de noticias - haga múltiples requests para ver el patrón");

                case "fallback":
                    // Simular escenario que active fallback
                    _logger.LogInformation("Simulando escenario de fallback para noticias...");
                    throw new HttpRequestException("Error que activará fallback - debería ver noticias de fallback");

                default:
                    return BadRequest(new { 
                        Error = $"Escenario desconocido: {scenario}", 
                        AvailableScenarios = new[] { "timeout", "error", "slow", "intermittent", "circuitbreaker", "fallback" },
                        Description = "Use estos escenarios para probar diferentes patrones de resiliencia en el servicio de noticias",
                        Note = "Combine con el parámetro 'category' para probar escenarios específicos"
                    });
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Test de escenario de noticias {Scenario} cancelado", scenario);
            return StatusCode(408, new { 
                Message = "Request timeout en servicio de noticias", 
                Scenario = scenario,
                Category = category ?? "general",
                Note = "Este timeout puede ser del cliente o del servidor"
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error simulado en escenario de noticias {Scenario}", scenario);
            return StatusCode(500, new { 
                Message = $"Error simulado en servicio de noticias: {ex.Message}", 
                Scenario = scenario,
                Category = category ?? "general",
                Note = "Este error activará las políticas de retry, circuit breaker y fallback configuradas"
            });
        }
    }
}