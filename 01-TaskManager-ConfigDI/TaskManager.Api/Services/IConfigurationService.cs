using TaskManager.Api.Configuration;

namespace TaskManager.Api.Services;

/// <summary>
/// Interfaz para el servicio de configuración
/// Demuestra el acceso a configuración a través de servicios
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Obtiene las opciones de TaskManager
    /// </summary>
    TaskManagerOptions GetTaskManagerOptions();

    /// <summary>
    /// Obtiene las opciones de la API
    /// </summary>
    ApiOptions GetApiOptions();

    /// <summary>
    /// Obtiene las opciones de base de datos
    /// </summary>
    DatabaseOptions GetDatabaseOptions();

    /// <summary>
    /// Verifica si una funcionalidad está habilitada
    /// </summary>
    /// <param name="featureName">Nombre de la funcionalidad</param>
    /// <returns>True si está habilitada</returns>
    bool IsFeatureEnabled(string featureName);

    /// <summary>
    /// Obtiene un valor de configuración por clave
    /// </summary>
    /// <typeparam name="T">Tipo del valor</typeparam>
    /// <param name="key">Clave de configuración</param>
    /// <param name="defaultValue">Valor por defecto</param>
    /// <returns>Valor de configuración</returns>
    T GetValue<T>(string key, T defaultValue = default!);

    /// <summary>
    /// Obtiene información del ambiente actual
    /// </summary>
    /// <returns>Información del ambiente</returns>
    EnvironmentInfo GetEnvironmentInfo();
}

/// <summary>
/// Información del ambiente de ejecución
/// </summary>
public class EnvironmentInfo
{
    /// <summary>
    /// Nombre del ambiente (Development, Staging, Production)
    /// </summary>
    public string EnvironmentName { get; set; } = string.Empty;

    /// <summary>
    /// Indica si es ambiente de desarrollo
    /// </summary>
    public bool IsDevelopment { get; set; }

    /// <summary>
    /// Indica si es ambiente de producción
    /// </summary>
    public bool IsProduction { get; set; }

    /// <summary>
    /// Nombre de la aplicación
    /// </summary>
    public string ApplicationName { get; set; } = string.Empty;

    /// <summary>
    /// Versión de la aplicación
    /// </summary>
    public string Version { get; set; } = string.Empty;
}