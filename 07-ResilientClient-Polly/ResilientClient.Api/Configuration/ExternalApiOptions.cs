namespace ResilientClient.Api.Configuration;

/// <summary>
/// Configuración para APIs externas
/// </summary>
public class ExternalApiOptions
{
    public const string SectionName = "ExternalApis";

    /// <summary>
    /// Configuración del servicio de clima
    /// </summary>
    public WeatherApiOptions Weather { get; set; } = new();

    /// <summary>
    /// Configuración del servicio de noticias
    /// </summary>
    public NewsApiOptions News { get; set; } = new();
}

public class WeatherApiOptions
{
    /// <summary>
    /// URL base de la API de clima
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.openweathermap.org/data/2.5";

    /// <summary>
    /// API Key para el servicio de clima
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Timeout en segundos para requests
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}

public class NewsApiOptions
{
    /// <summary>
    /// URL base de la API de noticias
    /// </summary>
    public string BaseUrl { get; set; } = "https://newsapi.org/v2";

    /// <summary>
    /// API Key para el servicio de noticias
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Timeout en segundos para requests
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}