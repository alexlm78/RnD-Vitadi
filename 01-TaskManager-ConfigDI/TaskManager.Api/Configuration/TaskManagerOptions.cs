namespace TaskManager.Api.Configuration;

/// <summary>
/// Opciones de configuración para TaskManager
/// Demuestra el patrón Options para configuración fuertemente tipada
/// </summary>
public class TaskManagerOptions
{
    /// <summary>
    /// Sección de configuración en appsettings.json
    /// </summary>
    public const string SectionName = "TaskManager";

    /// <summary>
    /// Indica si se deben inicializar datos de ejemplo
    /// </summary>
    public bool SeedData { get; set; } = true;

    /// <summary>
    /// Número máximo de tareas por usuario
    /// </summary>
    public int MaxTasksPerUser { get; set; } = 100;

    /// <summary>
    /// Prioridad por defecto para nuevas tareas (1=Baja, 2=Media, 3=Alta)
    /// </summary>
    public int DefaultPriority { get; set; } = 2;

    /// <summary>
    /// Configuración de funcionalidades
    /// </summary>
    public FeatureOptions Features { get; set; } = new();

    /// <summary>
    /// Configuración de cache
    /// </summary>
    public CacheOptions Cache { get; set; } = new();
}

/// <summary>
/// Opciones de funcionalidades que se pueden habilitar/deshabilitar
/// </summary>
public class FeatureOptions
{
    /// <summary>
    /// Habilita filtros avanzados en la API
    /// </summary>
    public bool EnableAdvancedFiltering { get; set; } = true;

    /// <summary>
    /// Habilita el historial de cambios en tareas
    /// </summary>
    public bool EnableTaskHistory { get; set; } = false;

    /// <summary>
    /// Habilita notificaciones por email
    /// </summary>
    public bool EnableEmailNotifications { get; set; } = false;

    /// <summary>
    /// Habilita métricas y telemetría
    /// </summary>
    public bool EnableMetrics { get; set; } = true;
}

/// <summary>
/// Opciones de configuración de cache
/// </summary>
public class CacheOptions
{
    /// <summary>
    /// Tiempo de expiración del cache en minutos
    /// </summary>
    public int ExpirationMinutes { get; set; } = 30;

    /// <summary>
    /// Habilita el cache en memoria
    /// </summary>
    public bool EnableMemoryCache { get; set; } = true;

    /// <summary>
    /// Tamaño máximo del cache
    /// </summary>
    public int MaxCacheSize { get; set; } = 1000;
}