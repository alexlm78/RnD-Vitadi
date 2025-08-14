# Conceptos Fundamentales: Dependency Injection y Configuración en .NET 8

## Dependency Injection (DI)

### ¿Qué es Dependency Injection?

Dependency Injection es un patrón de diseño que implementa **Inversión de Control (IoC)** para resolver dependencias entre objetos. En lugar de que un objeto cree sus propias dependencias, estas se "inyectan" desde el exterior.

### Beneficios del DI

1. **Desacoplamiento**: Las clases no dependen de implementaciones concretas
2. **Testabilidad**: Fácil creación de mocks y stubs para pruebas
3. **Mantenibilidad**: Cambios en implementaciones no afectan a los consumidores
4. **Flexibilidad**: Intercambio de implementaciones sin modificar código

### Ciclos de Vida de Servicios

#### 1. Transient
```csharp
builder.Services.AddTransient<IService, Service>();
```
- **Nueva instancia** en cada solicitud
- **Uso**: Servicios ligeros, sin estado
- **Ejemplo**: Calculadoras, validadores simples

#### 2. Scoped
```csharp
builder.Services.AddScoped<ITaskService, TaskService>();
```
- **Una instancia por request HTTP**
- **Uso**: Servicios que mantienen estado durante un request
- **Ejemplo**: Servicios de negocio, repositorios

#### 3. Singleton
```csharp
builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
```
- **Una instancia para toda la aplicación**
- **Uso**: Servicios costosos de crear, configuración
- **Ejemplo**: Servicios de configuración, cache, logging

### Implementación en TaskManager

```csharp
// Program.cs - Registro de servicios
builder.Services.AddScoped<ITaskService, TaskService>();           // Scoped
builder.Services.AddScoped<IValidationService, ValidationService>(); // Scoped  
builder.Services.AddSingleton<IConfigurationService, ConfigurationService>(); // Singleton

// TasksController.cs - Inyección múltiple
public TasksController(
    ITaskService taskService,
    IValidationService validationService, 
    IConfigurationService configurationService,
    ILogger<TasksController> logger)
{
    _taskService = taskService;
    _validationService = validationService;
    _configurationService = configurationService;
    _logger = logger;
}
```

### Patrones de Inyección

#### 1. Constructor Injection (Recomendado)
```csharp
public class TaskService : ITaskService
{
    private readonly ILogger<TaskService> _logger;
    private readonly IConfigurationService _configurationService;

    public TaskService(ILogger<TaskService> logger, IConfigurationService configurationService)
    {
        _logger = logger;
        _configurationService = configurationService;
    }
}
```

#### 2. Property Injection (No recomendado en .NET Core)
```csharp
public class TaskService : ITaskService
{
    public ILogger<TaskService> Logger { get; set; }
}
```

#### 3. Method Injection (Casos específicos)
```csharp
public void ProcessTask(IValidator validator, TaskItem task)
{
    // validator se inyecta como parámetro
}
```

## Configuración en .NET 8

### Jerarquía de Configuración

.NET 8 utiliza un sistema de configuración jerárquico que combina múltiples fuentes:

1. **appsettings.json** (base)
2. **appsettings.{Environment}.json** (específico del ambiente)
3. **Variables de entorno**
4. **Argumentos de línea de comandos**
5. **Azure Key Vault** (en producción)

### Options Pattern

El **Options Pattern** es la forma recomendada de acceder a configuración en .NET:

#### 1. Definir Clase de Opciones
```csharp
public class TaskManagerOptions
{
    public const string SectionName = "TaskManager";
    
    public bool SeedData { get; set; } = true;
    public int MaxTasksPerUser { get; set; } = 100;
    public FeatureOptions Features { get; set; } = new();
}

public class FeatureOptions
{
    public bool EnableAdvancedFiltering { get; set; } = true;
    public bool EnableTaskHistory { get; set; } = false;
}
```

#### 2. Configurar en appsettings.json
```json
{
  "TaskManager": {
    "SeedData": true,
    "MaxTasksPerUser": 100,
    "Features": {
      "EnableAdvancedFiltering": true,
      "EnableTaskHistory": false
    }
  }
}
```

#### 3. Registrar en DI Container
```csharp
// Program.cs
builder.Services.Configure<TaskManagerOptions>(
    builder.Configuration.GetSection(TaskManagerOptions.SectionName));
```

#### 4. Inyectar en Servicios
```csharp
public class ConfigurationService : IConfigurationService
{
    private readonly IOptionsMonitor<TaskManagerOptions> _taskManagerOptions;

    public ConfigurationService(IOptionsMonitor<TaskManagerOptions> taskManagerOptions)
    {
        _taskManagerOptions = taskManagerOptions;
    }

    public TaskManagerOptions GetTaskManagerOptions()
    {
        return _taskManagerOptions.CurrentValue; // Valor actual (reactivo)
    }
}
```

### Interfaces de Options

#### IOptions<T>
```csharp
private readonly IOptions<TaskManagerOptions> _options;
// _options.Value - Valor al momento de la inyección (no reactivo)
```

#### IOptionsSnapshot<T>
```csharp
private readonly IOptionsSnapshot<TaskManagerOptions> _options;
// _options.Value - Valor actual por request (reactivo por request)
```

#### IOptionsMonitor<T>
```csharp
private readonly IOptionsMonitor<TaskManagerOptions> _options;
// _options.CurrentValue - Valor actual (reactivo en tiempo real)
// _options.OnChange() - Notificación de cambios
```

### Configuración por Ambiente

#### Development (appsettings.Development.json)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "TaskManager": "Debug"
    }
  },
  "TaskManager": {
    "SeedData": true,
    "MaxTasksPerUser": 50
  }
}
```

#### Production (appsettings.Production.json)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "TaskManager": "Information"
    }
  },
  "TaskManager": {
    "SeedData": false,
    "MaxTasksPerUser": 1000
  }
}
```

### Validación de Configuración

```csharp
// Validación automática al inicio
builder.Services.AddOptions<TaskManagerOptions>()
    .Bind(builder.Configuration.GetSection(TaskManagerOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Clase con validaciones
public class TaskManagerOptions
{
    [Required]
    [Range(1, 10000)]
    public int MaxTasksPerUser { get; set; } = 100;
    
    [Required]
    public FeatureOptions Features { get; set; } = new();
}
```

## Mejores Prácticas

### Dependency Injection

1. **Usar interfaces**: Siempre definir contratos claros
2. **Constructor injection**: Preferir sobre otros tipos
3. **Evitar Service Locator**: No usar `GetService()` directamente
4. **Ciclo de vida apropiado**: Elegir el correcto según el uso
5. **Evitar dependencias circulares**: Diseñar arquitectura limpia

### Configuración

1. **Options Pattern**: Usar siempre para configuración tipada
2. **Validación**: Validar configuración al inicio
3. **Separación por ambiente**: Diferentes configs para cada ambiente
4. **Secretos seguros**: Usar Azure Key Vault en producción
5. **Configuración reactiva**: Usar `IOptionsMonitor` cuando sea necesario

## Ejemplos Prácticos en TaskManager

### 1. Servicio con Múltiples Dependencias
```csharp
public class ValidationService : IValidationService
{
    private readonly ITaskService _taskService;           // Scoped
    private readonly IConfigurationService _configService; // Singleton
    private readonly ILogger<ValidationService> _logger;   // Singleton

    public ValidationService(
        ITaskService taskService,
        IConfigurationService configurationService,
        ILogger<ValidationService> logger)
    {
        _taskService = taskService;
        _configurationService = configurationService;
        _logger = logger;
    }
}
```

### 2. Configuración Condicional
```csharp
public bool IsFeatureEnabled(string featureName)
{
    var options = _taskManagerOptions.CurrentValue;
    
    return featureName.ToLowerInvariant() switch
    {
        "advancedfiltering" => options.Features.EnableAdvancedFiltering,
        "taskhistory" => options.Features.EnableTaskHistory,
        "metrics" => options.Features.EnableMetrics,
        _ => false
    };
}
```

### 3. Configuración Reactiva
```csharp
public class ConfigurationService : IConfigurationService
{
    public ConfigurationService(IOptionsMonitor<TaskManagerOptions> options)
    {
        _options = options;
        
        // Reaccionar a cambios de configuración
        _options.OnChange(newOptions =>
        {
            _logger.LogInformation("Configuración actualizada: MaxTasks={MaxTasks}", 
                newOptions.MaxTasksPerUser);
        });
    }
}
```

## Conclusión

La combinación de **Dependency Injection** y **Configuración** en .NET 8 proporciona:

- **Flexibilidad**: Fácil intercambio de implementaciones
- **Testabilidad**: Inyección de mocks para pruebas
- **Mantenibilidad**: Código desacoplado y configurable
- **Escalabilidad**: Configuración por ambiente
- **Robustez**: Validación automática de configuración

Estos patrones son fundamentales para crear aplicaciones .NET modernas, mantenibles y escalables.