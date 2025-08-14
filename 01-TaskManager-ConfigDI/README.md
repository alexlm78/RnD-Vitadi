# TaskManager API - Configuración y Dependency Injection

## Descripción

Esta mini-aplicación demuestra los conceptos fundamentales de **configuración** y **dependency injection** en .NET 8. Es una API REST simple para gestionar tareas que utiliza las mejores prácticas de desarrollo en ASP.NET Core.

## Conceptos Demostrados

### 1. Dependency Injection (DI)
- **Registro de servicios**: Configuración de servicios en `Program.cs`
- **Inyección de dependencias**: Uso de interfaces para desacoplar componentes
- **Ciclos de vida de servicios**: 
  - `Scoped`: ITaskService, IValidationService (una instancia por request)
  - `Singleton`: IConfigurationService (una instancia para toda la app)
- **Constructor injection**: Inyección a través del constructor
- **Múltiples dependencias**: Inyección de varios servicios en un controlador

### 2. Configuración Avanzada
- **Options Pattern**: Configuración fuertemente tipada con clases
- **IOptionsMonitor**: Monitoreo de cambios en configuración en tiempo real
- **Configuración por ambiente**: Development, Production con diferentes settings
- **Validación de configuración**: Validación automática al inicio de la aplicación
- **Configuración jerárquica**: Secciones anidadas y configuración compleja

### 3. Servicios de Aplicación
- **Separación de responsabilidades**: Servicios especializados para diferentes tareas
- **Validación de negocio**: Servicio dedicado para validaciones complejas
- **Configuración como servicio**: Acceso centralizado a configuración
- **Logging estructurado**: Logging con contexto y diferentes niveles

### 4. Documentación Avanzada de API
- **Swagger/OpenAPI**: Generación automática de documentación
- **XML Comments**: Documentación enriquecida con comentarios
- **Swagger Annotations**: Anotaciones detalladas con SwaggerOperation
- **Swagger UI**: Interfaz interactiva para probar la API
- **Configuración dinámica**: Swagger configurado desde appsettings
- **Ejemplos automáticos**: Filtros personalizados para generar ejemplos
- **Agrupación por tags**: Organización de endpoints por funcionalidad
- **Documentación de parámetros**: Descripciones detalladas de parámetros

## Estructura del Proyecto

```
TaskManager.Api/
├── Configuration/                  # Clases de configuración fuertemente tipadas
│   ├── TaskManagerOptions.cs     # Opciones específicas de TaskManager
│   ├── ApiOptions.cs              # Opciones de configuración de API
│   └── DatabaseOptions.cs        # Opciones de base de datos
├── Controllers/
│   └── TasksController.cs         # Controlador principal con múltiples servicios
├── DTOs/
│   ├── CreateTaskDto.cs          # DTO para crear tareas
│   ├── UpdateTaskDto.cs          # DTO para actualizar tareas
│   └── TaskResponseDto.cs        # DTO para respuestas
├── Models/
│   └── TaskItem.cs               # Modelo de dominio
├── Services/
│   ├── ITaskService.cs           # Interfaz del servicio de tareas
│   ├── TaskService.cs            # Implementación del servicio de tareas
│   ├── IValidationService.cs     # Interfaz del servicio de validación
│   ├── ValidationService.cs      # Implementación de validación de negocio
│   ├── IConfigurationService.cs  # Interfaz del servicio de configuración
│   └── ConfigurationService.cs   # Servicio centralizado de configuración
├── Program.cs                    # Configuración avanzada de DI y pipeline
├── appsettings.json             # Configuración base con múltiples secciones
├── appsettings.Development.json # Configuración de desarrollo
└── appsettings.Production.json  # Configuración de producción
```

## Funcionalidades

### Endpoints Disponibles

**Gestión de Tareas:**
- `GET /api/tasks` - Obtener todas las tareas
- `GET /api/tasks/{id}` - Obtener una tarea específica
- `POST /api/tasks` - Crear una nueva tarea (con validación de negocio)
- `PUT /api/tasks/{id}` - Actualizar una tarea existente (con validación)
- `DELETE /api/tasks/{id}` - Eliminar una tarea (con validación)
- `GET /api/tasks/paged` - Obtener tareas con paginación

**Filtros y Búsqueda:**
- `GET /api/tasks/status/{completed}` - Filtrar por estado
- `GET /api/tasks/priority/{priority}` - Filtrar por prioridad (con validación)
- `GET /api/tasks/search` - Buscar tareas por texto

**Estadísticas y Monitoreo:**
- `GET /api/tasks/stats` - Estadísticas de tareas (funcionalidad configurable)
- `GET /api/tasks/health` - Health check del controlador con configuración

**Configuración y Desarrollo:**
- `GET /health` - Health check de la aplicación con información detallada
- `GET /config` - Ver configuración completa (solo desarrollo/staging)
- `GET /di-test` - Probar dependency injection (solo desarrollo/staging)

### Modelo de Tarea

```json
{
  "id": 1,
  "title": "Título de la tarea",
  "description": "Descripción opcional",
  "isCompleted": false,
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": "2024-01-01T10:00:00Z",
  "priority": 2,
  "priorityText": "Media"
}
```

## Cómo Ejecutar

### Prerrequisitos
- .NET 8 SDK
- Editor de código (Visual Studio, VS Code, etc.)

### Pasos

1. **Navegar al directorio del proyecto**:
   ```bash
   cd 01-TaskManager-ConfigDI/TaskManager.Api
   ```

2. **Restaurar dependencias**:
   ```bash
   dotnet restore
   ```

3. **Ejecutar la aplicación**:
   ```bash
   dotnet run
   ```

4. **Acceder a la documentación**:
   - Swagger UI: `https://localhost:7xxx` (puerto asignado automáticamente)
   - API: `https://localhost:7xxx/api/tasks`

## Configuración Avanzada

### Estructura de Configuración

La aplicación utiliza el **Options Pattern** para configuración fuertemente tipada:

```json
{
  "TaskManager": {
    "SeedData": true,
    "MaxTasksPerUser": 100,
    "DefaultPriority": 2,
    "Features": {
      "EnableAdvancedFiltering": true,
      "EnableTaskHistory": false,
      "EnableEmailNotifications": false,
      "EnableMetrics": true
    },
    "Cache": {
      "ExpirationMinutes": 30,
      "EnableMemoryCache": true,
      "MaxCacheSize": 1000
    }
  },
  "Api": {
    "Version": "1.0.0",
    "Title": "TaskManager API",
    "Description": "API para gestión de tareas",
    "Contact": {
      "Name": "Equipo de Desarrollo",
      "Email": "dev@taskmanager.com",
      "Url": "https://taskmanager.com"
    },
    "Cors": {
      "AllowedOrigins": ["*"],
      "AllowedMethods": ["GET", "POST", "PUT", "DELETE"],
      "AllowCredentials": false
    },
    "RateLimit": {
      "Enabled": false,
      "MaxRequests": 100,
      "WindowMinutes": 1
    }
  },
  "Database": {
    "Provider": "InMemory",
    "CommandTimeout": 30,
    "EnableSqlLogging": false,
    "Retry": {
      "MaxRetryCount": 3,
      "InitialDelay": 1000,
      "BackoffMultiplier": 2.0
    }
  }
}
```

### Configuración por Ambiente

- **Development**: Logging detallado, datos de ejemplo, CORS permisivo
- **Production**: Logging mínimo, sin datos de ejemplo, CORS restrictivo
- **Staging**: Configuración intermedia para pruebas

### Clases de Configuración

- **TaskManagerOptions**: Configuración específica de la aplicación
- **ApiOptions**: Configuración de la API (CORS, rate limiting, etc.)
- **DatabaseOptions**: Configuración de base de datos y reintentos

## Dependency Injection en Acción

### Registro de Servicios con Diferentes Ciclos de Vida

```csharp
// Program.cs - Configuración de servicios

// Options Pattern - Configuración fuertemente tipada
builder.Services.Configure<TaskManagerOptions>(
    builder.Configuration.GetSection(TaskManagerOptions.SectionName));

builder.Services.Configure<ApiOptions>(
    builder.Configuration.GetSection(ApiOptions.SectionName));

// Servicios de aplicación con diferentes ciclos de vida
builder.Services.AddScoped<ITaskService, TaskService>();           // Scoped
builder.Services.AddScoped<IValidationService, ValidationService>(); // Scoped
builder.Services.AddSingleton<IConfigurationService, ConfigurationService>(); // Singleton
```

### Inyección Múltiple en Controller

```csharp
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly IValidationService _validationService;
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<TasksController> _logger;

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
}
```

### Inyección de Configuración Tipada

```csharp
public class ConfigurationService : IConfigurationService
{
    private readonly IOptionsMonitor<TaskManagerOptions> _taskManagerOptions;
    private readonly IOptionsMonitor<ApiOptions> _apiOptions;
    private readonly IWebHostEnvironment _environment;

    public ConfigurationService(
        IOptionsMonitor<TaskManagerOptions> taskManagerOptions,
        IOptionsMonitor<ApiOptions> apiOptions,
        IWebHostEnvironment environment)
    {
        _taskManagerOptions = taskManagerOptions;
        _apiOptions = apiOptions;
        _environment = environment;
    }
}
```

### Validación de Configuración

```csharp
// Validación automática de configuración al inicio
builder.Services.AddOptions<TaskManagerOptions>()
    .Bind(builder.Configuration.GetSection(TaskManagerOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

## Logging

La aplicación utiliza el sistema de logging integrado de .NET:

- **Console**: Logs en la consola durante desarrollo
- **Debug**: Logs en la ventana de debug
- **Structured logging**: Logs con contexto estructurado

### Ejemplos de Logs
```csharp
_logger.LogInformation("Creando nueva tarea: {Title}", createTaskDto.Title);
_logger.LogWarning("Tarea con ID {TaskId} no encontrada", id);
```

## Validación

Utiliza Data Annotations para validación automática:

```csharp
[Required(ErrorMessage = "El título es obligatorio")]
[MaxLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
public string Title { get; set; } = string.Empty;
```

## Documentación Swagger Avanzada

### Características Implementadas

1. **Anotaciones Detalladas**:
   ```csharp
   [SwaggerOperation(
       Summary = "Crear nueva tarea",
       Description = "Crea una nueva tarea en el sistema con validación completa",
       OperationId = "CreateTask",
       Tags = new[] { "Gestión de Tareas" }
   )]
   ```

2. **Agrupación por Tags**:
   - **Gestión de Tareas**: CRUD básico
   - **Filtros y Búsqueda**: Endpoints de filtrado
   - **Estadísticas**: Métricas y reportes
   - **Monitoreo**: Health checks

3. **Documentación de Parámetros**:
   ```csharp
   [SwaggerParameter("ID único de la tarea", Required = true)] int id
   ```

4. **Esquemas Personalizados**:
   ```csharp
   [SwaggerSchema(
       Title = "Crear Tarea",
       Description = "Datos necesarios para crear una nueva tarea"
   )]
   ```

5. **Ejemplos Automáticos**: Filtro personalizado que genera ejemplos para DTOs

6. **Respuestas Documentadas**:
   ```csharp
   [SwaggerResponse(200, "Tarea creada exitosamente", typeof(TaskResponseDto))]
   [SwaggerResponse(400, "Datos inválidos")]
   ```

### Acceso a la Documentación

- **Desarrollo**: `https://localhost:7xxx/` (Swagger UI en la raíz)
- **Staging**: Disponible con autenticación
- **Producción**: Deshabilitado por seguridad

## Próximos Pasos

Este proyecto sirve como base para:
1. Agregar persistencia con Entity Framework
2. Implementar autenticación y autorización
3. Añadir tests unitarios e integración
4. Configurar CI/CD
5. Implementar patrones más avanzados

## Recursos de Aprendizaje

- [Dependency Injection en .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [Configuración en ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [Swagger/OpenAPI](https://docs.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger)
- [Logging en .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging)