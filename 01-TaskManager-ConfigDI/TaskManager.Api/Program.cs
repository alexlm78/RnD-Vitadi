using System.Reflection;
using TaskManager.Api.Configuration;
using TaskManager.Api.Services;
using TaskManager.Api.Swagger;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// CONFIGURACIÓN DE OPCIONES (Options Pattern)
// ========================================

// Configurar opciones fuertemente tipadas usando el patrón Options
builder.Services.Configure<TaskManagerOptions>(
    builder.Configuration.GetSection(TaskManagerOptions.SectionName));

builder.Services.Configure<ApiOptions>(
    builder.Configuration.GetSection(ApiOptions.SectionName));

builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection(DatabaseOptions.SectionName));

// Validar configuración al inicio (opcional)
builder.Services.AddOptions<TaskManagerOptions>()
    .Bind(builder.Configuration.GetSection(TaskManagerOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// ========================================
// REGISTRO DE SERVICIOS (Dependency Injection)
// ========================================

// Servicios de aplicación con diferentes ciclos de vida
builder.Services.AddScoped<ITaskService, TaskService>();           // Scoped: una instancia por request
builder.Services.AddScoped<IValidationService, ValidationService>(); // Scoped: para servicios que dependen de otros scoped
builder.Services.AddSingleton<IConfigurationService, ConfigurationService>(); // Singleton: una instancia para toda la app

// Servicios de infraestructura
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ========================================
// CONFIGURACIÓN DE SWAGGER/OPENAPI
// ========================================

builder.Services.AddSwaggerGen(c =>
{
    // Obtener configuración de API desde appsettings
    var apiOptions = builder.Configuration.GetSection(ApiOptions.SectionName).Get<ApiOptions>() ?? new ApiOptions();
    
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = apiOptions.Title,
        Version = apiOptions.Version,
        Description = apiOptions.Description,
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = apiOptions.Contact.Name,
            Email = apiOptions.Contact.Email,
            Url = new Uri(apiOptions.Contact.Url)
        }
    });

    // Incluir comentarios XML para documentación
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Habilitar anotaciones de Swagger para documentación enriquecida
    c.EnableAnnotations();
    
    // Configurar esquemas
    c.DescribeAllParametersInCamelCase();
    
    // Agregar ejemplos para los DTOs
    c.SchemaFilter<ExampleSchemaFilter>();
});

// ========================================
// CONFIGURACIÓN DE CORS
// ========================================

builder.Services.AddCors(options =>
{
    var corsOptions = builder.Configuration.GetSection($"{ApiOptions.SectionName}:Cors").Get<CorsOptions>() ?? new CorsOptions();
    
    options.AddPolicy("ConfiguredPolicy", policy =>
    {
        if (corsOptions.AllowedOrigins.Contains("*"))
        {
            policy.AllowAnyOrigin();
        }
        else
        {
            policy.WithOrigins(corsOptions.AllowedOrigins);
        }

        if (corsOptions.AllowedMethods.Contains("*"))
        {
            policy.AllowAnyMethod();
        }
        else
        {
            policy.WithMethods(corsOptions.AllowedMethods);
        }

        if (corsOptions.AllowedHeaders.Contains("*"))
        {
            policy.AllowAnyHeader();
        }
        else
        {
            policy.WithHeaders(corsOptions.AllowedHeaders);
        }

        if (corsOptions.AllowCredentials)
        {
            policy.AllowCredentials();
        }
    });
});

// ========================================
// CONFIGURACIÓN DE LOGGING
// ========================================

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configurar niveles de logging desde appsettings
var logLevel = builder.Configuration.GetValue<string>("Logging:LogLevel:Default", "Information");
if (Enum.TryParse<LogLevel>(logLevel, out var level))
{
    builder.Logging.SetMinimumLevel(level);
}

// ========================================
// SERVICIOS ADICIONALES
// ========================================

// Configurar cache en memoria si está habilitado
var taskManagerOptions = builder.Configuration.GetSection(TaskManagerOptions.SectionName).Get<TaskManagerOptions>();
if (taskManagerOptions?.Cache.EnableMemoryCache == true)
{
    builder.Services.AddMemoryCache(options =>
    {
        options.SizeLimit = taskManagerOptions.Cache.MaxCacheSize;
    });
}

// ========================================
// CONSTRUCCIÓN DE LA APLICACIÓN
// ========================================

var app = builder.Build();

// Obtener servicios para logging inicial
var logger = app.Services.GetRequiredService<ILogger<Program>>();
var configService = app.Services.GetRequiredService<IConfigurationService>();
var environmentInfo = configService.GetEnvironmentInfo();

logger.LogInformation("Iniciando TaskManager API v{Version} en ambiente {Environment}", 
    environmentInfo.Version, environmentInfo.EnvironmentName);

// ========================================
// CONFIGURACIÓN DEL PIPELINE HTTP
// ========================================

// Configurar Swagger para todos los ambientes (con restricciones en producción)
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        var apiOptions = configService.GetApiOptions();
        c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{apiOptions.Title} v{apiOptions.Version}");
        c.RoutePrefix = string.Empty; // Swagger UI en la raíz
        c.DocumentTitle = $"{apiOptions.Title} - Documentación";
        c.DefaultModelsExpandDepth(-1); // Colapsar modelos por defecto
        c.DisplayRequestDuration();
        c.EnableTryItOutByDefault();
    });
}

// Configurar CORS usando la política configurada
app.UseCors("ConfiguredPolicy");

app.UseHttpsRedirection();

// ========================================
// MIDDLEWARE PERSONALIZADO
// ========================================

// Middleware para logging de requests (solo en desarrollo y staging)
if (!app.Environment.IsProduction())
{
    app.Use(async (context, next) =>
    {
        var requestLogger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        var startTime = DateTime.UtcNow;
        
        requestLogger.LogDebug("Iniciando request: {Method} {Path} at {Time}", 
            context.Request.Method, 
            context.Request.Path, 
            startTime);
        
        await next();
        
        var duration = DateTime.UtcNow - startTime;
        requestLogger.LogDebug("Request completado: {Method} {Path} - {StatusCode} en {Duration}ms", 
            context.Request.Method, 
            context.Request.Path,
            context.Response.StatusCode,
            duration.TotalMilliseconds);
    });
}

// Middleware para manejo de errores global
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var errorLogger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        var exceptionFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        
        if (exceptionFeature != null)
        {
            errorLogger.LogError(exceptionFeature.Error, "Error no manejado en {Path}", context.Request.Path);
        }

        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            Error = "Error interno del servidor",
            RequestId = context.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    });
});

app.UseAuthorization();

app.MapControllers();

// ========================================
// ENDPOINTS ADICIONALES
// ========================================

// Endpoint de salud de la aplicación con información detallada
app.MapGet("/health", (IConfigurationService configService) =>
{
    var environmentInfo = configService.GetEnvironmentInfo();
    var taskManagerOptions = configService.GetTaskManagerOptions();
    
    return Results.Ok(new
    {
        Status = "Healthy",
        Application = environmentInfo.ApplicationName,
        Environment = environmentInfo.EnvironmentName,
        Version = environmentInfo.Version,
        Timestamp = DateTime.UtcNow,
        Configuration = new
        {
            SeedData = taskManagerOptions.SeedData,
            MaxTasksPerUser = taskManagerOptions.MaxTasksPerUser,
            DefaultPriority = taskManagerOptions.DefaultPriority,
            Features = taskManagerOptions.Features
        }
    });
})
.WithName("HealthCheck")
.WithTags("Health")
.WithOpenApi()
.WithSummary("Verificar el estado de salud de la aplicación")
.WithDescription("Retorna información sobre el estado y configuración de la aplicación");

// Endpoint de información de configuración (solo en desarrollo y staging)
if (!app.Environment.IsProduction())
{
    app.MapGet("/config", (IConfigurationService configService) =>
    {
        var environmentInfo = configService.GetEnvironmentInfo();
        var taskManagerOptions = configService.GetTaskManagerOptions();
        var apiOptions = configService.GetApiOptions();
        var databaseOptions = configService.GetDatabaseOptions();
        
        return Results.Ok(new
        {
            Environment = environmentInfo,
            TaskManager = taskManagerOptions,
            Api = apiOptions,
            Database = new
            {
                databaseOptions.Provider,
                databaseOptions.CommandTimeout,
                databaseOptions.EnableSqlLogging,
                databaseOptions.Retry
                // No incluir ConnectionString por seguridad
            }
        });
    })
    .WithName("GetConfiguration")
    .WithTags("Development")
    .WithOpenApi()
    .WithSummary("Obtener configuración de la aplicación")
    .WithDescription("Retorna la configuración actual de la aplicación (solo disponible en desarrollo)");

    // Endpoint para probar dependency injection
    app.MapGet("/di-test", (
        ITaskService taskService,
        IValidationService validationService,
        IConfigurationService configService,
        ILogger<Program> logger) =>
    {
        logger.LogInformation("Probando dependency injection");
        
        return Results.Ok(new
        {
            Message = "Dependency Injection funcionando correctamente",
            Services = new
            {
                TaskService = taskService.GetType().Name,
                ValidationService = validationService.GetType().Name,
                ConfigurationService = configService.GetType().Name
            },
            Environment = configService.GetEnvironmentInfo(),
            Timestamp = DateTime.UtcNow
        });
    })
    .WithName("TestDependencyInjection")
    .WithTags("Development")
    .WithOpenApi()
    .WithSummary("Probar dependency injection")
    .WithDescription("Endpoint para verificar que todos los servicios se inyectan correctamente");
}

// ========================================
// INICIAR LA APLICACIÓN
// ========================================

logger.LogInformation("TaskManager API configurada correctamente. Iniciando servidor...");

try
{
    app.Run();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Error crítico al iniciar la aplicación");
    throw;
}
finally
{
    logger.LogInformation("TaskManager API detenida");
}
