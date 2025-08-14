using Microsoft.Extensions.Options;
using TaskManager.Api.Configuration;

namespace TaskManager.Api.Services;

/// <summary>
/// Implementación del servicio de configuración
/// Demuestra el uso del patrón Options y acceso a configuración
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly IOptionsMonitor<TaskManagerOptions> _taskManagerOptions;
    private readonly IOptionsMonitor<ApiOptions> _apiOptions;
    private readonly IOptionsMonitor<DatabaseOptions> _databaseOptions;
    private readonly ILogger<ConfigurationService> _logger;

    public ConfigurationService(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        IOptionsMonitor<TaskManagerOptions> taskManagerOptions,
        IOptionsMonitor<ApiOptions> apiOptions,
        IOptionsMonitor<DatabaseOptions> databaseOptions,
        ILogger<ConfigurationService> logger)
    {
        _configuration = configuration;
        _environment = environment;
        _taskManagerOptions = taskManagerOptions;
        _apiOptions = apiOptions;
        _databaseOptions = databaseOptions;
        _logger = logger;

        _logger.LogInformation("ConfigurationService inicializado para ambiente: {Environment}", 
            _environment.EnvironmentName);
    }

    public TaskManagerOptions GetTaskManagerOptions()
    {
        var options = _taskManagerOptions.CurrentValue;
        _logger.LogDebug("Obteniendo opciones de TaskManager: SeedData={SeedData}, MaxTasks={MaxTasks}", 
            options.SeedData, options.MaxTasksPerUser);
        return options;
    }

    public ApiOptions GetApiOptions()
    {
        var options = _apiOptions.CurrentValue;
        _logger.LogDebug("Obteniendo opciones de API: Version={Version}, Title={Title}", 
            options.Version, options.Title);
        return options;
    }

    public DatabaseOptions GetDatabaseOptions()
    {
        var options = _databaseOptions.CurrentValue;
        _logger.LogDebug("Obteniendo opciones de base de datos: Provider={Provider}", 
            options.Provider);
        return options;
    }

    public bool IsFeatureEnabled(string featureName)
    {
        var taskManagerOptions = GetTaskManagerOptions();
        
        var isEnabled = featureName.ToLowerInvariant() switch
        {
            "advancedfiltering" => taskManagerOptions.Features.EnableAdvancedFiltering,
            "taskhistory" => taskManagerOptions.Features.EnableTaskHistory,
            "emailnotifications" => taskManagerOptions.Features.EnableEmailNotifications,
            "metrics" => taskManagerOptions.Features.EnableMetrics,
            _ => false
        };

        _logger.LogDebug("Verificando funcionalidad {FeatureName}: {IsEnabled}", featureName, isEnabled);
        return isEnabled;
    }

    public T GetValue<T>(string key, T defaultValue = default!)
    {
        try
        {
            var value = _configuration.GetValue<T>(key, defaultValue);
            _logger.LogDebug("Obteniendo valor de configuración {Key}: {Value}", key, value);
            return value ?? defaultValue;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error obteniendo valor de configuración {Key}, usando valor por defecto: {DefaultValue}", 
                key, defaultValue);
            return defaultValue;
        }
    }

    public EnvironmentInfo GetEnvironmentInfo()
    {
        var apiOptions = GetApiOptions();
        
        var environmentInfo = new EnvironmentInfo
        {
            EnvironmentName = _environment.EnvironmentName,
            IsDevelopment = _environment.IsDevelopment(),
            IsProduction = _environment.IsProduction(),
            ApplicationName = GetValue<string>("ApplicationName", "TaskManager API"),
            Version = apiOptions.Version
        };

        _logger.LogDebug("Información del ambiente: {EnvironmentName}, IsDev={IsDevelopment}", 
            environmentInfo.EnvironmentName, environmentInfo.IsDevelopment);

        return environmentInfo;
    }
}