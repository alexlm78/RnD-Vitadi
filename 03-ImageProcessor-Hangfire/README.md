# ImageProcessor - Background Jobs con Hangfire

Esta mini-aplicación demuestra el uso completo de **Hangfire** para procesamiento de imágenes en segundo plano en .NET Core 8, utilizando **ImageSharp** para manipulación profesional de imágenes. Es un ejemplo práctico de cómo implementar diferentes tipos de trabajos en segundo plano en aplicaciones web modernas.

## 📋 Conceptos Demostrados

### Hangfire - Sistema de Jobs Completo
- **Hangfire.Core**: Motor principal para trabajos en segundo plano
- **Hangfire.SqlServer**: Almacenamiento persistente con SQL Server
- **Hangfire.AspNetCore**: Dashboard web interactivo para monitoreo
- **Jobs Fire-and-Forget**: Procesamiento inmediato de imágenes
- **Jobs Recurrentes**: Limpieza automática y mantenimiento del sistema
- **Jobs Programados**: Procesamiento diferido en el tiempo
- **Jobs Encadenados**: Secuencias de procesamiento dependientes
- **Colas Múltiples**: Organización de trabajos por prioridad y tipo

### Procesamiento de Imágenes Avanzado
- **SixLabors.ImageSharp**: Biblioteca moderna para manipulación de imágenes
- **Redimensionado Inteligente**: Cambio de tamaño con preservación de aspecto
- **Filtros Profesionales**: Aplicación de efectos visuales avanzados
- **Thumbnails Optimizados**: Generación de miniaturas para web
- **Procesamiento en Lotes**: Múltiples imágenes simultáneamente
- **Validación de Archivos**: Control de tipos y tamaños permitidos

### Patrones de Background Processing
- **Async/Await**: Procesamiento no bloqueante
- **Dependency Injection**: Servicios inyectados en jobs
- **Error Handling**: Manejo robusto de errores y reintentos
- **Monitoring**: Seguimiento en tiempo real de trabajos
- **Queue Management**: Gestión de colas por prioridad

## 🏗️ Arquitectura

```
ImageProcessor.Api/
├── Controllers/
│   └── ImageController.cs              # API endpoints para procesamiento
├── Services/
│   ├── IImageProcessingService.cs      # Interfaz del servicio
│   └── ImageProcessingService.cs       # Implementación con ImageSharp
├── Models/
│   └── ImageProcessingModels.cs        # Modelos de request/response
├── uploads/                            # Directorio de imágenes subidas
├── processed/                          # Directorio de imágenes procesadas
├── HangfireAuthorizationFilter.cs      # Filtro de autorización
├── Program.cs                          # Configuración de Hangfire y DI
└── appsettings.json                   # Configuración de paths y conexiones
```

## 🚀 Configuración

### 1. Paquetes NuGet Instalados

```xml
<PackageReference Include="Hangfire.Core" Version="1.8.21" />
<PackageReference Include="Hangfire.SqlServer" Version="1.8.21" />
<PackageReference Include="Hangfire.AspNetCore" Version="1.8.21" />
<PackageReference Include="SixLabors.ImageSharp" Version="3.1.6" />
<PackageReference Include="SixLabors.ImageSharp.Web" Version="3.1.3" />
```

### 2. Configuración en appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ImageProcessorHangfire;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "ImageProcessing": {
    "UploadPath": "uploads",
    "ProcessedPath": "processed",
    "MaxFileSize": 10485760,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif", ".bmp"],
    "CleanupIntervalDays": 7
  }
}
```

### 3. Registro de Servicios

```csharp
// Registrar servicio de procesamiento de imágenes
builder.Services.AddScoped<IImageProcessingService, ImageProcessingService>();

// Configurar job recurrente de limpieza
RecurringJob.AddOrUpdate<IImageProcessingService>(
    "cleanup-old-images",
    service => service.CleanupOldImagesAsync(),
    Cron.Daily);
```

## 🎨 Operaciones de Procesamiento

### 1. Redimensionado de Imágenes
- Cambio de tamaño a dimensiones específicas
- Opción de mantener proporción de aspecto
- Salida en formato JPEG de alta calidad

### 2. Aplicación de Filtros
Filtros disponibles:
- **Grayscale**: Conversión a escala de grises
- **Sepia**: Efecto vintage sepia
- **Blur**: Desenfoque gaussiano
- **Sharpen**: Aumento de nitidez
- **Brightness**: Ajuste de brillo
- **Contrast**: Modificación de contraste
- **Invert**: Inversión de colores

### 3. Generación de Thumbnails
- Miniaturas cuadradas con recorte automático
- Tamaño configurable
- Optimización para web

### 4. Procesamiento en Lotes
- Procesamiento paralelo de múltiples imágenes
- Soporte para todas las operaciones
- Mejor rendimiento para grandes volúmenes

## 🔧 Ejecución

### Prerrequisitos

1. **.NET 8 SDK** instalado
2. **SQL Server LocalDB** o SQL Server disponible
3. **Visual Studio 2022** o **VS Code** (opcional)

### Pasos para ejecutar

1. **Clonar y navegar al proyecto:**
   ```bash
   cd 03-ImageProcessor-Hangfire/ImageProcessor.Api
   ```

2. **Restaurar paquetes:**
   ```bash
   dotnet restore
   ```

3. **Ejecutar la aplicación:**
   ```bash
   dotnet run
   ```

4. **La aplicación creará automáticamente los directorios necesarios:**
   - `uploads/` - Para imágenes subidas
   - `processed/` - Para imágenes procesadas

## 📡 API Endpoints Completos

### 🔄 Subida y Procesamiento Básico
```http
POST /api/Image/upload
Content-Type: multipart/form-data
# Subir imagen individual con procesamiento automático

POST /api/Image/upload-batch
Content-Type: multipart/form-data
# Subir múltiples imágenes para procesamiento en lote
```

### ⚡ Operaciones de Procesamiento Directo
```http
POST /api/Image/resize          # Redimensionar imagen (Fire-and-Forget)
POST /api/Image/apply-filters   # Aplicar filtros visuales
POST /api/Image/thumbnail       # Generar thumbnail optimizado
POST /api/Image/batch          # Procesamiento en lotes
```

### ⏰ Jobs Programados y Avanzados
```http
POST /api/Image/schedule-delayed    # Job programado con retraso
POST /api/Image/chain-processing    # Jobs encadenados (resize → thumbnail)
POST /api/Image/recurring-job/{id}  # Crear job recurrente personalizado
DELETE /api/Image/recurring-job/{id} # Eliminar job recurrente
```

### 📊 Monitoreo y Gestión
```http
POST /api/Image/cleanup         # Limpieza manual de archivos
GET /api/Image/status/{jobId}   # Estado detallado del job
GET /api/Image/job-stats        # Estadísticas completas del sistema
GET /api/Image/health          # Health check del servicio
```

## 💻 Ejemplos de Uso Completos

### 📤 Subir y Procesar Imagen Individual
```bash
# Subir imagen con redimensionado automático
curl -X POST "http://localhost:5084/api/Image/upload" \
  -H "Content-Type: multipart/form-data" \
  -F "file=@imagen.jpg" \
  -F "operation=Resize" \
  -F "parameters[width]=800" \
  -F "parameters[height]=600" \
  -F "parameters[maintainAspectRatio]=true"

# Subir imagen con filtros
curl -X POST "http://localhost:5084/api/Image/upload" \
  -H "Content-Type: multipart/form-data" \
  -F "file=@imagen.jpg" \
  -F "operation=ApplyFilters" \
  -F "parameters[filters]=Grayscale,Sepia"
```

### 📦 Procesamiento en Lotes
```bash
# Subir múltiples imágenes para procesamiento en lote
curl -X POST "http://localhost:5084/api/Image/upload-batch" \
  -H "Content-Type: multipart/form-data" \
  -F "files=@imagen1.jpg" \
  -F "files=@imagen2.jpg" \
  -F "files=@imagen3.jpg" \
  -F "operation=GenerateThumbnails" \
  -F "parameters={\"size\":200}"
```

### ⚡ Operaciones Directas (Fire-and-Forget)
```bash
# Redimensionar imagen existente
curl -X POST "http://localhost:5084/api/Image/resize" \
  -H "Content-Type: application/json" \
  -d '{
    "imagePath": "uploads/sample.jpg",
    "outputPath": "processed/resized_sample.jpg",
    "width": 1200,
    "height": 800,
    "maintainAspectRatio": true
  }'

# Aplicar múltiples filtros
curl -X POST "http://localhost:5084/api/Image/apply-filters" \
  -H "Content-Type: application/json" \
  -d '{
    "imagePath": "uploads/sample.jpg",
    "outputPath": "processed/filtered_sample.jpg",
    "filters": ["Grayscale", "Contrast", "Sharpen"]
  }'
```

### ⏰ Jobs Programados y Encadenados
```bash
# Programar procesamiento con retraso de 10 minutos
curl -X POST "http://localhost:5084/api/Image/schedule-delayed?delayMinutes=10" \
  -H "Content-Type: application/json" \
  -d '{
    "imagePath": "uploads/sample.jpg",
    "outputPath": "processed/delayed_sample.jpg",
    "width": 800,
    "height": 600
  }'

# Crear cadena de procesamiento (resize → thumbnail)
curl -X POST "http://localhost:5084/api/Image/chain-processing" \
  -H "Content-Type: application/json" \
  -d '{
    "imagePath": "uploads/sample.jpg",
    "outputPath": "processed/chained_sample.jpg",
    "width": 1000,
    "height": 750
  }'
```

### 🔄 Jobs Recurrentes Personalizados
```bash
# Crear job recurrente cada 6 horas
curl -X POST "http://localhost:5084/api/Image/recurring-job/custom-maintenance?cronExpression=0%20*%2F6%20*%20*%20*"

# Eliminar job recurrente
curl -X DELETE "http://localhost:5084/api/Image/recurring-job/custom-maintenance"
```

### 📊 Monitoreo y Estadísticas
```bash
# Obtener estado de un job específico
curl -X GET "http://localhost:5084/api/Image/status/job-id-12345"

# Obtener estadísticas completas del sistema
curl -X GET "http://localhost:5084/api/Image/job-stats"

# Ejecutar limpieza manual
curl -X POST "http://localhost:5084/api/Image/cleanup"
```

## 📊 Dashboard de Hangfire - Monitoreo Completo

### 🔐 Acceso al Dashboard
- **URL**: `http://localhost:5084/hangfire`
- **Autenticación**: 
  - **Desarrollo**: Acceso libre desde localhost
  - **Producción**: Usuario: `admin`, Contraseña: `hangfire123`

### 📈 Funcionalidades del Dashboard

#### 🏠 **Vista Principal (Home)**
- Resumen de estadísticas en tiempo real
- Gráficos de rendimiento de jobs
- Estado general del sistema

#### 💼 **Jobs (Trabajos)**
- **Enqueued**: Jobs en cola esperando procesamiento
- **Processing**: Jobs ejecutándose actualmente
- **Succeeded**: Jobs completados exitosamente
- **Failed**: Jobs que fallaron con detalles de error
- **Scheduled**: Jobs programados para ejecución futura
- **Deleted**: Jobs eliminados del sistema

#### 🔄 **Queues (Colas)**
- **default**: Jobs generales del sistema
- **images**: Jobs de procesamiento de imágenes
- **cleanup**: Jobs de limpieza y mantenimiento
- Estadísticas por cola y tiempo de procesamiento

#### 🖥️ **Servers (Servidores)**
- Información de servidores Hangfire activos
- Workers configurados por servidor
- Colas procesadas por cada servidor
- Estado de conexión y heartbeat

#### ⏰ **Recurring Jobs (Jobs Recurrentes)**
- **cleanup-old-images**: Limpieza diaria automática
- **system-health-check**: Verificación de salud cada hora
- Jobs personalizados creados via API
- Próximas ejecuciones programadas

#### 🔄 **Retries (Reintentos)**
- Jobs que fallaron y están programados para reintento
- Configuración automática de reintentos
- Historial de intentos fallidos

## 🔄 Tipos de Jobs en Segundo Plano

### 1. 🚀 **Fire-and-Forget Jobs**
Jobs que se ejecutan inmediatamente en segundo plano:
```csharp
// Ejemplo: Procesamiento inmediato de imagen
BackgroundJob.Enqueue<IImageProcessingService>(
    "images", // Cola específica
    service => service.ResizeImageAsync(request));
```
**Características:**
- Ejecución inmediata cuando hay workers disponibles
- Reintento automático en caso de fallo (hasta 10 intentos)
- Seguimiento completo via dashboard
- Ideal para: Procesamiento de imágenes, envío de emails, operaciones I/O

### 2. ⏰ **Scheduled Jobs (Jobs Programados)**
Jobs que se ejecutan en un momento específico en el futuro:
```csharp
// Ejemplo: Procesamiento diferido por 10 minutos
BackgroundJob.Schedule<IImageProcessingService>(
    service => service.ResizeImageAsync(request),
    TimeSpan.FromMinutes(10));
```
**Características:**
- Ejecución diferida en el tiempo
- Útil para procesamiento no urgente
- Reducción de carga en horas pico
- Ideal para: Reportes programados, notificaciones diferidas

### 3. 🔗 **Continuation Jobs (Jobs Encadenados)**
Jobs que se ejecutan después de que otro job se complete exitosamente:
```csharp
// Ejemplo: Thumbnail después de resize
var resizeJobId = BackgroundJob.Enqueue<IImageProcessingService>(
    service => service.ResizeImageAsync(request));

BackgroundJob.ContinueJobWith<IImageProcessingService>(
    resizeJobId,
    service => service.GenerateThumbnailAsync(thumbnailRequest));
```
**Características:**
- Dependencia entre jobs
- Ejecución secuencial garantizada
- Manejo de errores en cadena
- Ideal para: Pipelines de procesamiento, workflows complejos

### 4. 🔄 **Recurring Jobs (Jobs Recurrentes)**
Jobs que se ejecutan automáticamente según un cronograma:
```csharp
// Ejemplo: Limpieza diaria automática
RecurringJob.AddOrUpdate<IImageProcessingService>(
    "cleanup-old-images",
    service => service.CleanupOldImagesAsync(),
    Cron.Daily, // Expresión cron
    new RecurringJobOptions { Queue = "cleanup" });
```
**Características:**
- Ejecución automática basada en cron expressions
- Gestión de estado persistente
- Configuración flexible de horarios
- Ideal para: Mantenimiento, backups, reportes periódicos

### 5. 📦 **Batch Jobs (Jobs en Lote)**
Procesamiento de múltiples elementos de forma eficiente:
```csharp
// Ejemplo: Procesamiento paralelo de múltiples imágenes
BackgroundJob.Enqueue<IImageProcessingService>(
    "images",
    service => service.ProcessBatchAsync(batchRequest));
```
**Características:**
- Procesamiento paralelo optimizado
- Mejor rendimiento para grandes volúmenes
- Control de concurrencia
- Ideal para: Migración de datos, procesamiento masivo

## 🎯 Puntos Clave de Aprendizaje

### 🔧 **Conceptos Fundamentales de Hangfire**
1. **Arquitectura de Jobs**: Comprensión de los diferentes tipos de trabajos
2. **Persistencia**: Almacenamiento robusto con SQL Server
3. **Colas Múltiples**: Organización y priorización de trabajos
4. **Monitoreo en Tiempo Real**: Dashboard interactivo y estadísticas
5. **Manejo de Errores**: Reintentos automáticos y recuperación elegante
6. **Escalabilidad**: Múltiples servidores y workers
7. **Seguridad**: Autenticación y autorización del dashboard

### 🖼️ **Procesamiento Avanzado de Imágenes**
1. **ImageSharp Moderno**: Biblioteca de última generación para .NET
2. **Operaciones Asíncronas**: Procesamiento no bloqueante y eficiente
3. **Validación Robusta**: Control de tipos, tamaños y formatos
4. **Gestión de Memoria**: Uso correcto de disposable patterns
5. **Procesamiento Paralelo**: Optimización para múltiples imágenes
6. **Control de Calidad**: Configuración de compresión y formatos
7. **Organización de Archivos**: Estructura de directorios y naming

### 🏗️ **Patrones de Background Processing**
1. **Dependency Injection**: Servicios inyectados en jobs de forma segura
2. **Separation of Concerns**: Interfaces claras entre capas
3. **Configuration Management**: Settings flexibles y por ambiente
4. **Structured Logging**: Registro completo con contexto
5. **Error Handling**: Estrategias de recuperación y notificación
6. **Resource Management**: Gestión eficiente de archivos y memoria
7. **Testing Strategies**: Pruebas de jobs y servicios asincrónicos

### 📊 **Monitoreo y Observabilidad**
1. **Dashboard Analytics**: Interpretación de métricas de rendimiento
2. **Job Lifecycle**: Comprensión del ciclo de vida completo
3. **Queue Management**: Gestión de colas y balanceo de carga
4. **Performance Tuning**: Optimización de workers y timeouts
5. **Alerting**: Configuración de notificaciones de fallos
6. **Capacity Planning**: Dimensionamiento de recursos

### 🔒 **Consideraciones de Producción**
1. **Security**: Autenticación robusta para dashboard
2. **Scalability**: Configuración multi-servidor
3. **Reliability**: Estrategias de backup y recuperación
4. **Monitoring**: Integración con sistemas de monitoreo
5. **Maintenance**: Limpieza automática y gestión de datos
6. **Performance**: Optimización de queries y storage

## 🎨 Ejemplos Prácticos de Diferentes Tipos de Jobs

### 🚀 **Fire-and-Forget: Procesamiento Inmediato**
```csharp
// En el Controller
[HttpPost("process-now")]
public IActionResult ProcessImageNow([FromBody] ImageRequest request)
{
    // Job se ejecuta inmediatamente en background
    var jobId = BackgroundJob.Enqueue<IImageProcessingService>(
        "images", // Cola específica para imágenes
        service => service.ResizeImageAsync(request));
    
    return Ok(new { JobId = jobId, Status = "Enqueued" });
}

// En el Service
public async Task ResizeImageAsync(ImageRequest request)
{
    _logger.LogInformation("Starting resize for {ImagePath}", request.ImagePath);
    
    using var image = await Image.LoadAsync(request.ImagePath);
    image.Mutate(x => x.Resize(request.Width, request.Height));
    await image.SaveAsync(request.OutputPath);
    
    _logger.LogInformation("Resize completed for {ImagePath}", request.ImagePath);
}
```

### ⏰ **Scheduled: Procesamiento Diferido**
```csharp
// Procesar imagen en horario de menor carga (2 AM)
[HttpPost("schedule-night-processing")]
public IActionResult ScheduleNightProcessing([FromBody] ImageRequest request)
{
    var tonight2AM = DateTime.Today.AddDays(1).AddHours(2);
    
    var jobId = BackgroundJob.Schedule<IImageProcessingService>(
        service => service.ProcessLargeImageAsync(request),
        tonight2AM);
    
    return Ok(new { JobId = jobId, ScheduledFor = tonight2AM });
}
```

### 🔗 **Continuation: Jobs Encadenados**
```csharp
[HttpPost("full-processing-pipeline")]
public IActionResult CreateProcessingPipeline([FromBody] ImageRequest request)
{
    // 1. Redimensionar imagen
    var resizeJobId = BackgroundJob.Enqueue<IImageProcessingService>(
        "images",
        service => service.ResizeImageAsync(request));
    
    // 2. Aplicar filtros después del resize
    var filterJobId = BackgroundJob.ContinueJobWith<IImageProcessingService>(
        resizeJobId,
        "images",
        service => service.ApplyFiltersAsync(CreateFilterRequest(request)));
    
    // 3. Generar thumbnail después de los filtros
    var thumbnailJobId = BackgroundJob.ContinueJobWith<IImageProcessingService>(
        filterJobId,
        "images",
        service => service.GenerateThumbnailAsync(CreateThumbnailRequest(request)));
    
    return Ok(new { 
        Pipeline = new { resizeJobId, filterJobId, thumbnailJobId },
        Status = "Pipeline Created"
    });
}
```

### 🔄 **Recurring: Mantenimiento Automático**
```csharp
// En Program.cs - Configuración de jobs recurrentes
public static void ConfigureRecurringJobs()
{
    // Limpieza diaria a las 3 AM
    RecurringJob.AddOrUpdate<IImageProcessingService>(
        "daily-cleanup",
        service => service.CleanupOldImagesAsync(),
        "0 3 * * *", // Cron: 3 AM todos los días
        new RecurringJobOptions { Queue = "cleanup" });
    
    // Backup semanal los domingos a las 2 AM
    RecurringJob.AddOrUpdate<IBackupService>(
        "weekly-backup",
        service => service.BackupProcessedImagesAsync(),
        "0 2 * * 0", // Cron: 2 AM todos los domingos
        new RecurringJobOptions { Queue = "backup" });
    
    // Reporte mensual el primer día del mes
    RecurringJob.AddOrUpdate<IReportService>(
        "monthly-report",
        service => service.GenerateMonthlyReportAsync(),
        "0 6 1 * *", // Cron: 6 AM el día 1 de cada mes
        new RecurringJobOptions { Queue = "reports" });
}
```

### 📦 **Batch: Procesamiento Masivo**
```csharp
[HttpPost("process-image-gallery")]
public async Task<IActionResult> ProcessImageGallery([FromForm] IFormFileCollection files)
{
    // Subir todos los archivos primero
    var uploadedPaths = await UploadFilesAsync(files);
    
    // Crear job de procesamiento en lote
    var batchRequest = new BatchProcessingRequest
    {
        ImagePaths = uploadedPaths,
        Operation = ProcessingOperation.GenerateThumbnails,
        Parameters = new Dictionary<string, object> { ["size"] = 200 }
    };
    
    var jobId = BackgroundJob.Enqueue<IImageProcessingService>(
        "images",
        service => service.ProcessBatchAsync(batchRequest));
    
    return Ok(new { 
        JobId = jobId, 
        FilesCount = uploadedPaths.Count,
        Status = "Batch Processing Started"
    });
}

// Implementación del procesamiento en lote
public async Task ProcessBatchAsync(BatchProcessingRequest request)
{
    _logger.LogInformation("Starting batch processing of {Count} images", request.ImagePaths.Count);
    
    // Procesar en paralelo con límite de concurrencia
    var semaphore = new SemaphoreSlim(Environment.ProcessorCount);
    var tasks = request.ImagePaths.Select(async imagePath =>
    {
        await semaphore.WaitAsync();
        try
        {
            await ProcessSingleImageAsync(imagePath, request.Operation, request.Parameters);
        }
        finally
        {
            semaphore.Release();
        }
    });
    
    await Task.WhenAll(tasks);
    _logger.LogInformation("Batch processing completed for {Count} images", request.ImagePaths.Count);
}
```

## 🔒 Configuración de Seguridad del Dashboard

### 🛠️ **Para Desarrollo**
```csharp
public class HangfireDevAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // Acceso libre desde localhost en desarrollo
        var httpContext = context.GetHttpContext();
        var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString();
        return remoteIp == "127.0.0.1" || remoteIp == "::1" || remoteIp == null;
    }
}
```

### 🔐 **Para Producción**
```csharp
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly IConfiguration _configuration;

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        
        // Verificar autenticación básica
        var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader?.StartsWith("Basic ") == true)
        {
            var credentials = DecodeBasicAuth(authHeader);
            var configUser = _configuration["Hangfire:Dashboard:Username"];
            var configPass = _configuration["Hangfire:Dashboard:Password"];
            
            return credentials.Username == configUser && credentials.Password == configPass;
        }
        
        // Solicitar autenticación
        httpContext.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Hangfire Dashboard\"";
        httpContext.Response.StatusCode = 401;
        return false;
    }
}
```

### ⚙️ **Configuración en appsettings.json**
```json
{
  "Hangfire": {
    "Dashboard": {
      "Username": "admin",
      "Password": "your-secure-password-here",
      "Title": "ImageProcessor Jobs Dashboard"
    }
  }
}
```

## 📚 Recursos Adicionales

- [Documentación de Hangfire](https://docs.hangfire.io/)
- [ImageSharp Documentation](https://docs.sixlabors.com/articles/imagesharp/)
- [Background Jobs en .NET](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services)

## 🔧 Configuración Avanzada de Hangfire

### ⚙️ **Configuración de Servidor**
```csharp
// En Program.cs
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = Environment.ProcessorCount * 2; // Workers por CPU
    options.Queues = new[] { "images", "cleanup", "reports", "default" }; // Prioridad de colas
    options.ShutdownTimeout = TimeSpan.FromSeconds(30); // Timeout para shutdown
    options.SchedulePollingInterval = TimeSpan.FromSeconds(1); // Polling de jobs programados
});
```

### 🗄️ **Configuración de Storage**
```csharp
builder.Services.AddHangfire(config => config
    .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero, // Polling inmediato
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true, // Mejor rendimiento
        SchemaName = "hangfire" // Schema personalizado
    }));
```

### 📊 **Monitoreo y Métricas**
```csharp
// Configuración de métricas personalizadas
public class JobMetricsService
{
    private static readonly Counter ProcessedImages = Metrics
        .CreateCounter("hangfire_processed_images_total", "Total processed images");
    
    private static readonly Histogram JobDuration = Metrics
        .CreateHistogram("hangfire_job_duration_seconds", "Job execution duration");
    
    public void RecordImageProcessed() => ProcessedImages.Inc();
    public void RecordJobDuration(double seconds) => JobDuration.Observe(seconds);
}
```

## 🔍 Notas Técnicas Avanzadas

### 🖼️ **Configuración Optimizada de ImageSharp**
```csharp
// Configuración global de ImageSharp
Configuration.Default.ImageFormatsManager.SetEncoder(JpegFormat.Instance, new JpegEncoder
{
    Quality = 90, // Alta calidad para imágenes principales
    Subsample = JpegSubsample.Ratio420 // Optimización de tamaño
});

Configuration.Default.ImageFormatsManager.SetEncoder(PngFormat.Instance, new PngEncoder
{
    CompressionLevel = PngCompressionLevel.BestCompression,
    TransparentColorMode = PngTransparentColorMode.Clear
});
```

### 💾 **Gestión Avanzada de Memoria**
```csharp
public async Task ProcessLargeImageAsync(string imagePath)
{
    // Configuración de memoria para imágenes grandes
    var configuration = Configuration.Default.Clone();
    configuration.MemoryAllocator = ArrayPoolMemoryAllocator.CreateWithMinimalPooling();
    
    using var image = await Image.LoadAsync(configuration, imagePath);
    
    // Procesamiento con liberación inmediata de memoria
    image.Mutate(x => x.Resize(new ResizeOptions
    {
        Size = new Size(1920, 1080),
        Mode = ResizeMode.Max,
        Resampler = KnownResamplers.Lanczos3 // Alta calidad
    }));
    
    await image.SaveAsync(outputPath, new JpegEncoder { Quality = 85 });
    
    // Forzar garbage collection para imágenes muy grandes
    if (new FileInfo(imagePath).Length > 50 * 1024 * 1024) // > 50MB
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}
```

### 🔄 **Patrones de Retry y Error Handling**
```csharp
[AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 60, 120 })]
public async Task ProcessImageWithRetryAsync(ImageRequest request)
{
    try
    {
        await ProcessImageAsync(request);
    }
    catch (OutOfMemoryException ex)
    {
        _logger.LogError(ex, "Out of memory processing {ImagePath}", request.ImagePath);
        // No retry para errores de memoria
        throw new InvalidOperationException("Image too large to process", ex);
    }
    catch (FileNotFoundException ex)
    {
        _logger.LogError(ex, "Image file not found: {ImagePath}", request.ImagePath);
        // No retry para archivos no encontrados
        throw;
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Retryable error processing {ImagePath}", request.ImagePath);
        throw; // Permitir retry automático
    }
}
```

### 📈 **Optimización de Performance**
```csharp
// Configuración de paralelismo para batch processing
public async Task ProcessBatchOptimizedAsync(BatchProcessingRequest request)
{
    var maxConcurrency = Math.Min(Environment.ProcessorCount, request.ImagePaths.Count);
    var semaphore = new SemaphoreSlim(maxConcurrency);
    
    var partitioner = Partitioner.Create(request.ImagePaths, true);
    
    await Task.Run(() =>
        Parallel.ForEach(partitioner, new ParallelOptions
        {
            MaxDegreeOfParallelism = maxConcurrency
        }, async imagePath =>
        {
            await semaphore.WaitAsync();
            try
            {
                await ProcessSingleImageAsync(imagePath, request.Operation);
            }
            finally
            {
                semaphore.Release();
            }
        }));
}
```

## 🚀 Deployment y Producción

### 🐳 **Docker Configuration**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Instalar dependencias para ImageSharp
RUN apt-get update && apt-get install -y \
    libgdiplus \
    libc6-dev \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ImageProcessor.Api.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ImageProcessor.Api.dll"]
```

### 🔧 **Configuración de Producción**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-sql-server;Database=ImageProcessorHangfire;User Id=hangfire_user;Password=secure_password;TrustServerCertificate=true;"
  },
  "Hangfire": {
    "Dashboard": {
      "Username": "admin",
      "Password": "very-secure-production-password"
    },
    "Server": {
      "WorkerCount": 10,
      "Queues": ["critical", "images", "cleanup", "default"]
    }
  },
  "ImageProcessing": {
    "UploadPath": "/app/data/uploads",
    "ProcessedPath": "/app/data/processed",
    "MaxFileSize": 52428800,
    "CleanupIntervalDays": 30
  }
}
```

Esta implementación completa demuestra cómo construir un sistema robusto y escalable de procesamiento de imágenes usando Hangfire para gestión avanzada de trabajos en segundo plano, con todas las mejores prácticas de producción incluidas.