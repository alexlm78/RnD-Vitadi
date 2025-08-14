namespace TaskManager.Api.Configuration;

/// <summary>
/// Opciones de configuración para la base de datos
/// Demuestra configuración para diferentes proveedores de datos
/// </summary>
public class DatabaseOptions
{
    /// <summary>
    /// Sección de configuración en appsettings.json
    /// </summary>
    public const string SectionName = "Database";

    /// <summary>
    /// Proveedor de base de datos (InMemory, SqlServer, Oracle, PostgreSQL)
    /// </summary>
    public string Provider { get; set; } = "InMemory";

    /// <summary>
    /// Cadena de conexión a la base de datos
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Timeout para comandos de base de datos en segundos
    /// </summary>
    public int CommandTimeout { get; set; } = 30;

    /// <summary>
    /// Habilita logging de consultas SQL
    /// </summary>
    public bool EnableSqlLogging { get; set; } = false;

    /// <summary>
    /// Habilita datos sensibles en logs (solo para desarrollo)
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;

    /// <summary>
    /// Configuración de retry para operaciones de base de datos
    /// </summary>
    public RetryOptions Retry { get; set; } = new();
}

/// <summary>
/// Opciones de configuración para reintentos
/// </summary>
public class RetryOptions
{
    /// <summary>
    /// Número máximo de reintentos
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// Delay inicial entre reintentos en milisegundos
    /// </summary>
    public int InitialDelay { get; set; } = 1000;

    /// <summary>
    /// Factor de incremento para el delay
    /// </summary>
    public double BackoffMultiplier { get; set; } = 2.0;
}