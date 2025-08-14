namespace TaskManager.Api.Configuration;

/// <summary>
/// Opciones de configuración para la API
/// Demuestra configuración de aspectos transversales de la aplicación
/// </summary>
public class ApiOptions
{
    /// <summary>
    /// Sección de configuración en appsettings.json
    /// </summary>
    public const string SectionName = "Api";

    /// <summary>
    /// Versión de la API
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Título de la API para documentación
    /// </summary>
    public string Title { get; set; } = "TaskManager API";

    /// <summary>
    /// Descripción de la API
    /// </summary>
    public string Description { get; set; } = "API para gestión de tareas";

    /// <summary>
    /// Información de contacto
    /// </summary>
    public ContactInfo Contact { get; set; } = new();

    /// <summary>
    /// Configuración de CORS
    /// </summary>
    public CorsOptions Cors { get; set; } = new();

    /// <summary>
    /// Configuración de rate limiting
    /// </summary>
    public RateLimitOptions RateLimit { get; set; } = new();

    /// <summary>
    /// Configuración de paginación
    /// </summary>
    public PaginationOptions Pagination { get; set; } = new();
}

/// <summary>
/// Información de contacto para la documentación de la API
/// </summary>
public class ContactInfo
{
    /// <summary>
    /// Nombre del contacto
    /// </summary>
    public string Name { get; set; } = "Equipo de Desarrollo";

    /// <summary>
    /// Email de contacto
    /// </summary>
    public string Email { get; set; } = "dev@taskmanager.com";

    /// <summary>
    /// URL del sitio web
    /// </summary>
    public string Url { get; set; } = "https://taskmanager.com";
}

/// <summary>
/// Opciones de configuración de CORS
/// </summary>
public class CorsOptions
{
    /// <summary>
    /// Orígenes permitidos
    /// </summary>
    public string[] AllowedOrigins { get; set; } = { "*" };

    /// <summary>
    /// Métodos HTTP permitidos
    /// </summary>
    public string[] AllowedMethods { get; set; } = { "GET", "POST", "PUT", "DELETE", "OPTIONS" };

    /// <summary>
    /// Headers permitidos
    /// </summary>
    public string[] AllowedHeaders { get; set; } = { "*" };

    /// <summary>
    /// Permite credenciales
    /// </summary>
    public bool AllowCredentials { get; set; } = false;
}

/// <summary>
/// Opciones de configuración de rate limiting
/// </summary>
public class RateLimitOptions
{
    /// <summary>
    /// Habilita rate limiting
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Número máximo de requests por ventana de tiempo
    /// </summary>
    public int MaxRequests { get; set; } = 100;

    /// <summary>
    /// Ventana de tiempo en minutos
    /// </summary>
    public int WindowMinutes { get; set; } = 1;
}

/// <summary>
/// Opciones de configuración de paginación
/// </summary>
public class PaginationOptions
{
    /// <summary>
    /// Tamaño de página por defecto
    /// </summary>
    public int DefaultPageSize { get; set; } = 10;

    /// <summary>
    /// Tamaño máximo de página
    /// </summary>
    public int MaxPageSize { get; set; } = 100;

    /// <summary>
    /// Habilita paginación automática
    /// </summary>
    public bool EnableAutoPagination { get; set; } = true;
}