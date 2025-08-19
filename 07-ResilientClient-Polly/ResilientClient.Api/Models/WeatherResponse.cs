namespace ResilientClient.Api.Models;

/// <summary>
/// Respuesta del servicio de clima
/// </summary>
public class WeatherResponse
{
    public string? Location { get; set; }
    public double Temperature { get; set; }
    public string? Description { get; set; }
    public double Humidity { get; set; }
    public double WindSpeed { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Respuesta de la API externa de clima (simulada)
/// </summary>
public class ExternalWeatherResponse
{
    public string? Name { get; set; }
    public Main? Main { get; set; }
    public Weather[]? Weather { get; set; }
    public Wind? Wind { get; set; }
}

public class Main
{
    public double Temp { get; set; }
    public double Humidity { get; set; }
}

public class Weather
{
    public string? Description { get; set; }
}

public class Wind
{
    public double Speed { get; set; }
}