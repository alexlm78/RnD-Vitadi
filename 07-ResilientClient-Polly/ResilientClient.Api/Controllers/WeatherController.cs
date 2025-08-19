using Microsoft.AspNetCore.Mvc;
using ResilientClient.Api.Models;
using ResilientClient.Api.Services;

namespace ResilientClient.Api.Controllers;

/// <summary>
/// Controller para operaciones relacionadas con el clima
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;
    private readonly ILogger<WeatherController> _logger;

    public WeatherController(IWeatherService weatherService, ILogger<WeatherController> logger)
    {
        _weatherService = weatherService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el clima actual para una ciudad
    /// </summary>
    /// <param name="city">Nombre de la ciudad</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Información del clima actual</returns>
    /// <response code="200">Clima obtenido exitosamente</response>
    /// <response code="404">Ciudad no encontrada</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("current/{city}")]
    [ProducesResponseType(typeof(WeatherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WeatherResponse>> GetCurrentWeather(
        string city, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Solicitando clima para {City}", city);

            var weather = await _weatherService.GetCurrentWeatherAsync(city, cancellationToken);
            
            if (weather == null)
            {
                _logger.LogWarning("No se encontró información del clima para {City}", city);
                return NotFound($"No se pudo obtener información del clima para {city}");
            }

            return Ok(weather);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de conectividad al obtener clima para {City}", city);
            return StatusCode(503, "Servicio de clima temporalmente no disponible");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout al obtener clima para {City}", city);
            return StatusCode(408, "Timeout al obtener información del clima");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener clima para {City}", city);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene el clima con estrategia de fallback
    /// </summary>
    /// <param name="city">Nombre de la ciudad</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Información del clima con fallback a datos en caché</returns>
    /// <response code="200">Clima obtenido exitosamente (puede incluir datos en caché)</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("resilient/{city}")]
    [ProducesResponseType(typeof(WeatherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WeatherResponse>> GetResilientWeather(
        string city, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Solicitando clima resiliente para {City}", city);

            var weather = await _weatherService.GetWeatherWithFallbackAsync(city, cancellationToken);
            
            return Ok(weather);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico al obtener clima resiliente para {City}", city);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtiene el clima para múltiples ciudades
    /// </summary>
    /// <param name="cities">Lista de ciudades separadas por coma</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Información del clima para múltiples ciudades</returns>
    /// <response code="200">Clima obtenido para las ciudades disponibles</response>
    /// <response code="400">Lista de ciudades inválida</response>
    [HttpGet("multiple")]
    [ProducesResponseType(typeof(IEnumerable<WeatherResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<WeatherResponse>>> GetMultipleCitiesWeather(
        [FromQuery] string cities, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cities))
        {
            return BadRequest("Debe proporcionar al menos una ciudad");
        }

        var cityList = cities.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Trim())
            .Where(c => !string.IsNullOrEmpty(c))
            .ToList();

        if (!cityList.Any())
        {
            return BadRequest("Lista de ciudades inválida");
        }

        _logger.LogInformation("Solicitando clima para {Count} ciudades: {Cities}", 
            cityList.Count, string.Join(", ", cityList));

        var weatherTasks = cityList.Select(city => 
            _weatherService.GetWeatherWithFallbackAsync(city, cancellationToken));

        var results = await Task.WhenAll(weatherTasks);
        var validResults = results.Where(r => r != null).ToList();

        _logger.LogInformation("Se obtuvo clima para {ValidCount} de {TotalCount} ciudades", 
            validResults.Count, cityList.Count);

        return Ok(validResults);
    }

    /// <summary>
    /// Obtiene el clima con patrones de resiliencia avanzados (retry, circuit breaker, timeout, fallback)
    /// </summary>
    /// <param name="city">Nombre de la ciudad</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Información del clima con manejo avanzado de errores</returns>
    /// <response code="200">Clima obtenido con patrones de resiliencia</response>
    /// <response code="400">Parámetros inválidos</response>
    /// <response code="499">Request cancelada por el cliente</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("advanced/{city}")]
    [ProducesResponseType(typeof(WeatherResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WeatherResponse>> GetWeatherWithAdvancedResilience(
        string city, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return BadRequest("El nombre de la ciudad es requerido");
        }

        try
        {
            _logger.LogInformation("Solicitando clima con resiliencia avanzada para {City}", city);

            var weather = await _weatherService.GetWeatherWithAdvancedResilienceAsync(city, cancellationToken);
            
            // Con los patrones de resiliencia, siempre deberíamos obtener algún resultado
            // (ya sea datos frescos, de caché, o de fallback)
            return Ok(weather);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Request cancelada por el cliente para {City}", city);
            return StatusCode(499, "Request cancelada por el cliente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener clima para {City}", city);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Endpoint para probar diferentes escenarios de fallo y patrones de resiliencia
    /// </summary>
    /// <param name="scenario">Escenario de prueba: timeout, error, slow, intermittent</param>
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
    public async Task<ActionResult> TestResilienceScenario(
        string scenario, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Probando escenario de resiliencia: {Scenario}", scenario);

        try
        {
            switch (scenario.ToLowerInvariant())
            {
                case "timeout":
                    // Simular timeout (más que el timeout configurado)
                    _logger.LogInformation("Simulando timeout de 35 segundos...");
                    await Task.Delay(35000, cancellationToken);
                    return Ok(new { Message = "Este mensaje no debería aparecer debido al timeout" });

                case "error":
                    // Simular error HTTP
                    _logger.LogInformation("Simulando error de red...");
                    throw new HttpRequestException("Error simulado de red para demostrar retry policy");

                case "slow":
                    // Simular respuesta lenta pero exitosa
                    _logger.LogInformation("Simulando respuesta lenta de 2 segundos...");
                    await Task.Delay(2000, cancellationToken);
                    return Ok(new { 
                        Message = "Respuesta lenta pero exitosa", 
                        Scenario = scenario,
                        Timestamp = DateTime.UtcNow,
                        Duration = "2000ms"
                    });

                case "intermittent":
                    // Simular fallo intermitente (33% de probabilidad de éxito)
                    var random = new Random();
                    var success = random.Next(1, 4) == 1;
                    _logger.LogInformation("Simulando fallo intermitente - Éxito: {Success}", success);
                    
                    if (success)
                    {
                        return Ok(new { 
                            Message = "Éxito intermitente - esta vez funcionó", 
                            Scenario = scenario,
                            Timestamp = DateTime.UtcNow,
                            Attempt = "Exitoso"
                        });
                    }
                    throw new HttpRequestException("Fallo intermitente simulado - reintente para ver retry policy");

                case "circuitbreaker":
                    // Simular múltiples fallos para activar circuit breaker
                    _logger.LogInformation("Simulando fallo para activar circuit breaker...");
                    throw new HttpRequestException("Fallo simulado para activar circuit breaker - haga múltiples requests para ver el patrón");

                default:
                    return BadRequest(new { 
                        Error = $"Escenario desconocido: {scenario}", 
                        AvailableScenarios = new[] { "timeout", "error", "slow", "intermittent", "circuitbreaker" },
                        Description = "Use estos escenarios para probar diferentes patrones de resiliencia"
                    });
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Test de escenario {Scenario} cancelado", scenario);
            return StatusCode(408, new { 
                Message = "Request timeout", 
                Scenario = scenario,
                Note = "Este timeout puede ser del cliente o del servidor"
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error simulado en escenario {Scenario}", scenario);
            return StatusCode(500, new { 
                Message = $"Error simulado: {ex.Message}", 
                Scenario = scenario,
                Note = "Este error activará las políticas de retry y circuit breaker configuradas"
            });
        }
    }
}