using System.Text.Json;
using ResilientClient.Api.Models;

namespace ResilientClient.Api.Services;

/// <summary>
/// Implementación del servicio de clima con patrones de resiliencia
/// </summary>
public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WeatherService> _logger;
    private readonly Dictionary<string, WeatherResponse> _cache = new();

    public WeatherService(HttpClient httpClient, ILogger<WeatherService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<WeatherResponse?> GetCurrentWeatherAsync(string city, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Obteniendo clima para {City}", city);

            // Simulamos una API externa que puede fallar
            var response = await _httpClient.GetAsync($"/weather?q={city}", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var externalResponse = JsonSerializer.Deserialize<ExternalWeatherResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (externalResponse != null)
                {
                    var weatherResponse = MapToWeatherResponse(externalResponse);
                    
                    // Guardamos en caché para fallback
                    _cache[city.ToLowerInvariant()] = weatherResponse;
                    
                    _logger.LogInformation("Clima obtenido exitosamente para {City}", city);
                    return weatherResponse;
                }
            }

            _logger.LogWarning("No se pudo obtener clima para {City}. Status: {StatusCode}", city, response.StatusCode);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de red al obtener clima para {City}", city);
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout al obtener clima para {City}", city);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener clima para {City}", city);
            throw;
        }
    }

    public async Task<WeatherResponse?> GetWeatherWithFallbackAsync(string city, CancellationToken cancellationToken = default)
    {
        try
        {
            // Intentamos obtener datos frescos
            return await GetCurrentWeatherAsync(city, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Fallo al obtener clima fresco para {City}, usando fallback", city);
            
            // Fallback a datos en caché
            if (_cache.TryGetValue(city.ToLowerInvariant(), out var cachedWeather))
            {
                _logger.LogInformation("Usando datos en caché para {City}", city);
                return cachedWeather;
            }

            // Fallback a datos por defecto
            _logger.LogInformation("Usando datos por defecto para {City}", city);
            return new WeatherResponse
            {
                Location = city,
                Temperature = 20.0,
                Description = "Datos no disponibles",
                Humidity = 50.0,
                WindSpeed = 5.0,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    public async Task<WeatherResponse?> GetWeatherWithAdvancedResilienceAsync(string city, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            _logger.LogInformation("Iniciando request de clima para {City} con patrones de resiliencia avanzados", city);

            // El HttpClient ya tiene configuradas las políticas de Polly
            // Retry, Circuit Breaker, Timeout y Fallback se aplicarán automáticamente
            var response = await _httpClient.GetAsync($"/weather?q={city}&appid=demo&units=metric", cancellationToken);
            
            var duration = DateTime.UtcNow - startTime;
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var externalResponse = JsonSerializer.Deserialize<ExternalWeatherResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (externalResponse != null)
                {
                    var weatherResponse = MapToWeatherResponse(externalResponse);
                    
                    // Guardamos en caché para fallback futuro
                    _cache[city.ToLowerInvariant()] = weatherResponse;
                    
                    _logger.LogInformation("Clima obtenido exitosamente para {City} en {Duration}ms", 
                        city, duration.TotalMilliseconds);
                    return weatherResponse;
                }
            }

            _logger.LogWarning("Respuesta no exitosa para {City}. Status: {StatusCode}, Duration: {Duration}ms", 
                city, response.StatusCode, duration.TotalMilliseconds);
            return null;
        }
        catch (HttpRequestException ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "Error de red al obtener clima para {City} después de {Duration}ms", city, duration.TotalMilliseconds);
            
            // Las políticas de Polly ya manejaron los reintentos
            // Si llegamos aquí, todos los reintentos fallaron
            return await GetFallbackWeatherData(city);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "Timeout al obtener clima para {City} después de {Duration}ms", city, duration.TotalMilliseconds);
            return await GetFallbackWeatherData(city);
        }
        catch (TaskCanceledException ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "Operación cancelada para {City} después de {Duration}ms", city, duration.TotalMilliseconds);
            throw; // Re-throw cancellation
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "Error inesperado al obtener clima para {City} después de {Duration}ms", city, duration.TotalMilliseconds);
            return await GetFallbackWeatherData(city);
        }
    }

    private async Task<WeatherResponse> GetFallbackWeatherData(string city)
    {
        // Intentar obtener datos de caché primero
        if (_cache.TryGetValue(city.ToLowerInvariant(), out var cachedWeather))
        {
            _logger.LogInformation("Usando datos en caché como fallback para {City}", city);
            cachedWeather.Description += " (desde caché)";
            return cachedWeather;
        }

        // Simular obtención de datos de una fuente alternativa
        await Task.Delay(100); // Simular latencia de fuente alternativa
        
        _logger.LogInformation("Usando datos por defecto como fallback para {City}", city);
        return new WeatherResponse
        {
            Location = city,
            Temperature = GetDefaultTemperatureForCity(city),
            Description = "Datos no disponibles - usando fallback",
            Humidity = 50.0,
            WindSpeed = 5.0,
            Timestamp = DateTime.UtcNow
        };
    }

    private static double GetDefaultTemperatureForCity(string city)
    {
        // Simular temperaturas por defecto basadas en la ciudad
        return city.ToLowerInvariant() switch
        {
            var c when c.Contains("madrid") => 15.0,
            var c when c.Contains("barcelona") => 18.0,
            var c when c.Contains("sevilla") => 22.0,
            var c when c.Contains("bilbao") => 12.0,
            var c when c.Contains("valencia") => 20.0,
            _ => 16.0 // Temperatura promedio para España
        };
    }

    private static WeatherResponse MapToWeatherResponse(ExternalWeatherResponse external)
    {
        return new WeatherResponse
        {
            Location = external.Name,
            Temperature = external.Main?.Temp ?? 0,
            Description = external.Weather?.FirstOrDefault()?.Description ?? "N/A",
            Humidity = external.Main?.Humidity ?? 0,
            WindSpeed = external.Wind?.Speed ?? 0,
            Timestamp = DateTime.UtcNow
        };
    }
}