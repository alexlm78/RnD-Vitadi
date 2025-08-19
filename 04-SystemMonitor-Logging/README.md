# SystemMonitor - Logging Estructurado y Telemetría

Esta mini-aplicación demuestra el uso completo de **logging estructurado** y **telemetría** en .NET Core 8, incluyendo Serilog, Application Insights, Prometheus y monitoreo avanzado del sistema.

## Características Implementadas

### 1. Logging Estructurado con Serilog
- **Serilog.AspNetCore**: Integración principal con ASP.NET Core
- **Múltiples Sinks configurados**:
  - **Console**: Para desarrollo y debugging con formato personalizado
  - **File**: Para persistencia local con rotación diaria y retención
  - **Application Insights**: Para telemetría en la nube con conversión automática
- **Structured Properties**: Uso de propiedades estructuradas en lugar de interpolación de strings
- **Scoped Logging**: Contexto enriquecido por operación con BeginScope

### 2. Monitoreo de Sistema Avanzado
- **Métricas de CPU**: Monitoreo cross-platform (Windows con PerformanceCounter, Linux/macOS con estimación)
- **Métricas de Memoria**: Total, disponible, usada, específica del proceso
- **Métricas de Disco**: Uso por drive, espacio libre, información del sistema de archivos
- **Información del Proceso**: Threads, handles, memoria virtual, tiempos de CPU
- **Health Checks**: Estado de la aplicación, uptime, garbage collection

### 3. Telemetría y Métricas con Prometheus
- **prometheus-net.AspNetCore**: Exportación de métricas en formato Prometheus
- **Métricas HTTP**: Duración de requests, códigos de respuesta, throughput
- **Métricas de Sistema**: CPU, memoria, disco con labels por drive
- **Métricas Personalizadas**: Contadores y histogramas para operaciones de negocio
- **Endpoint /metrics**: Exposición estándar para scraping de Prometheus

### 4. Application Insights Integration
- **Microsoft.ApplicationInsights.AspNetCore**: Telemetría automática
- **Dependency Tracking**: Seguimiento de llamadas externas
- **Performance Counters**: Métricas del sistema operativo
- **Custom Events**: Eventos personalizados de negocio

## Estructura del Proyecto

```
04-SystemMonitor-Logging/
├── SystemMonitor.Api/
│   ├── Controllers/
│   │   └── SystemController.cs     # Endpoints de monitoreo con logging estructurado
│   ├── Models/
│   │   └── SystemMetrics.cs        # Modelos de datos para métricas
│   ├── Services/
│   │   ├── ISystemMetricsService.cs    # Interface del servicio de métricas
│   │   ├── SystemMetricsService.cs     # Implementación con Prometheus integration
│   │   └── MetricsCollectionService.cs # Background service para colección continua
│   ├── Program.cs                  # Configuración de Serilog, Prometheus y Application Insights
│   ├── appsettings.json           # Configuración de producción
│   ├── appsettings.Development.json # Configuración de desarrollo
│   └── logs/                      # Directorio de logs generados
└── README.md                      # Esta documentación
```

## Paquetes NuGet Utilizados

```xml
<!-- Logging Estructurado -->
<PackageReference Include="Serilog.AspNetCore" Version="9.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />
<PackageReference Include="Serilog.Sinks.File" Version="7.0.0" />
<PackageReference Include="Serilog.Sinks.ApplicationInsights" Version="4.0.0" />
<PackageReference Include="Serilog.Enrichers.Environment" Version="3.0.1" />
<PackageReference Include="Serilog.Enrichers.Thread" Version="4.0.0" />

<!-- Telemetría y Métricas -->
<PackageReference Include="Microsoft.ApplicationInsights.AspNetCore" Version="2.23.0" />
<PackageReference Include="prometheus-net.AspNetCore" Version="8.2.1" />

<!-- API Documentation -->
<PackageReference Include="Swashbuckle.AspNetCore" Version="7.2.0" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.0" />
```

## Configuración de Logging Estructurado

### Bootstrap Logger en Program.cs
```csharp
// Bootstrap logger para capturar logs de inicio de la aplicación
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting SystemMonitor API");
    // ... configuración de la aplicación
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush(); // Importante: asegurar que todos los logs se escriban
}
```

### Configuración Completa de Serilog
```csharp
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(outputTemplate: 
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/systemmonitor-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: 
            "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
        retainedFileCountLimit: 7)
    .WriteTo.ApplicationInsights(
        context.Configuration.GetConnectionString("ApplicationInsights") ?? "",
        TelemetryConverter.Traces));
```

### Logging Estructurado en Controllers
```csharp
public async Task<ActionResult<SystemMetrics>> GetSystemMetrics()
{
    // Crear scope con contexto estructurado
    using var activity = _logger.BeginScope(new Dictionary<string, object>
    {
        ["Operation"] = "GetSystemMetrics",
        ["RequestId"] = HttpContext.TraceIdentifier,
        ["UserAgent"] = HttpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown"
    });

    try
    {
        _logger.LogInformation("System metrics collection started");
        var startTime = DateTime.UtcNow;
        
        var metrics = await _systemMetricsService.GetSystemMetricsAsync();
        
        var duration = DateTime.UtcNow - startTime;
        // Logging estructurado con propiedades tipadas
        _logger.LogInformation("System metrics collected successfully in {Duration}ms. CPU: {CpuUsage:F1}%, Memory: {MemoryUsage:F1}%, Disks: {DiskCount}",
            duration.TotalMilliseconds,
            metrics.CpuUsagePercent,
            metrics.Memory.UsagePercent,
            metrics.Disks.Count);
        
        return Ok(metrics);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to retrieve system metrics");
        return StatusCode(500, new { error = "Failed to retrieve system metrics", details = ex.Message });
    }
}
```

### En appsettings.json
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/systemmonitor-.txt",
          "rollingInterval": "Day",
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
          "retainedFileCountLimit": 7
        }
      }
    ],
    "Enrich": [
      "FromLogContext",
      "WithMachineName",
      "WithThreadId"
    ],
    "Properties": {
      "Application": "SystemMonitor.Api"
    }
  }
}
```

## Middleware de Request Logging Avanzado

### Configuración de Request Logging
```csharp
app.UseSerilogRequestLogging(options =>
{
    // Template personalizado para requests HTTP
    options.MessageTemplate = "Handled {RequestPath} in {Elapsed:0.0000} ms";
    
    // Nivel de log dinámico basado en duración y errores
    options.GetLevel = (httpContext, elapsed, ex) => ex != null
        ? LogEventLevel.Error
        : elapsed > 1000
            ? LogEventLevel.Warning
            : LogEventLevel.Information;
    
    // Enriquecimiento del contexto diagnóstico
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown");
        diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString());
        diagnosticContext.Set("RequestSize", httpContext.Request.ContentLength ?? 0);
        diagnosticContext.Set("ResponseSize", httpContext.Response.ContentLength ?? 0);
    };
});
```

## Integración con Prometheus

### Configuración de Métricas
```csharp
// En Program.cs
app.UseRouting();
app.UseHttpMetrics(); // Colecta métricas HTTP automáticamente
app.MapMetrics(); // Expone endpoint /metrics para Prometheus

// En SystemMetricsService.cs
private static readonly Gauge CpuUsageGauge = Metrics
    .CreateGauge("system_cpu_usage_percent", "Current CPU usage percentage");

private static readonly Gauge MemoryUsageGauge = Metrics
    .CreateGauge("system_memory_usage_percent", "Current memory usage percentage");

private static readonly Gauge DiskUsageGauge = Metrics
    .CreateGauge("system_disk_usage_percent", "Disk usage percentage", "drive");
```

### Métricas Expuestas
- `http_requests_total{method, endpoint, status}` - Total de requests HTTP
- `http_request_duration_seconds{method, endpoint}` - Duración de requests
- `system_cpu_usage_percent` - Uso de CPU en porcentaje
- `system_memory_usage_percent` - Uso de memoria en porcentaje
- `system_memory_used_bytes` - Memoria usada en bytes
- `system_disk_usage_percent{drive}` - Uso de disco por drive

## Cómo Ejecutar

1. **Navegar al directorio del proyecto**:
   ```bash
   cd 04-SystemMonitor-Logging/SystemMonitor.Api
   ```

2. **Restaurar dependencias**:
   ```bash
   dotnet restore
   ```

3. **Ejecutar la aplicación**:
   ```bash
   dotnet run
   ```

4. **Acceder a los endpoints**:
   - **Swagger UI**: `https://localhost:5001` o `http://localhost:5000`
   - **Métricas del Sistema**: `https://localhost:5001/api/system/metrics`
   - **Prometheus Metrics**: `https://localhost:5001/metrics`
   - **Health Status**: `https://localhost:5001/api/system/health`

5. **Verificar logs y métricas**:
   - **Console**: Ver logs estructurados en tiempo real
   - **File**: Revisar archivos en `logs/systemmonitor-YYYYMMDD.txt`
   - **Prometheus**: Scraping desde `http://localhost:5001/metrics`

## Endpoints de Monitoreo Disponibles

### Métricas del Sistema
- `GET /api/system/metrics` - Métricas completas del sistema
- `GET /api/system/cpu` - Uso de CPU únicamente
- `GET /api/system/memory` - Información de memoria
- `GET /api/system/disk` - Uso de disco por drive
- `GET /api/system/info` - Información básica del sistema

### Monitoreo de Aplicación
- `GET /api/system/health` - Estado de salud y uptime
- `GET /api/system/process` - Información detallada del proceso
- `GET /api/system/environment` - Variables de entorno y configuración
- `POST /api/system/gc` - Trigger garbage collection y estadísticas

### Telemetría
- `GET /metrics` - Métricas en formato Prometheus

## Conceptos Clave de Logging Estructurado y Telemetría

### 1. **Logging Estructurado Avanzado**
- **Propiedades Tipadas**: Uso de `{PropertyName}` en lugar de interpolación de strings
- **Scoped Context**: `BeginScope()` para agregar contexto a múltiples logs
- **Structured Properties**: Objetos complejos serializados automáticamente como JSON
- **Correlation IDs**: Seguimiento de requests a través de `TraceIdentifier`

```csharp
// ❌ Logging tradicional (no estructurado)
_logger.LogInformation($"User {userId} performed action {action}");

// ✅ Logging estructurado
_logger.LogInformation("User {UserId} performed {Action}", userId, action);

// ✅ Scoped logging con contexto
using var scope = _logger.BeginScope(new Dictionary<string, object>
{
    ["Operation"] = "GetSystemMetrics",
    ["RequestId"] = HttpContext.TraceIdentifier
});
```

### 2. **Telemetría Multi-Dimensional**
- **Application Insights**: Telemetría automática de requests, dependencies, exceptions
- **Prometheus Metrics**: Métricas numéricas con labels para dimensiones
- **Custom Events**: Eventos de negocio específicos de la aplicación
- **Performance Counters**: Métricas del sistema operativo

### 3. **Observabilidad Completa**
- **Logs**: Eventos discretos con contexto estructurado
- **Metrics**: Valores numéricos agregables en el tiempo
- **Traces**: Seguimiento de requests a través de servicios
- **Health Checks**: Estado de componentes críticos

### 4. **Configuración Dinámica**
- **Niveles por Namespace**: Diferentes niveles para Microsoft, System, etc.
- **Sinks Condicionales**: Diferentes destinos según el ambiente
- **Enrichers**: Agregado automático de propiedades contextuales
- **Filtering**: Filtrado de logs sensibles o innecesarios

### 5. **Patrones de Monitoreo**
- **RED Method**: Rate, Errors, Duration para servicios
- **USE Method**: Utilization, Saturation, Errors para recursos
- **Golden Signals**: Latency, Traffic, Errors, Saturation
- **SLI/SLO**: Service Level Indicators y Objectives

## Ejemplos de Uso Avanzado

### Consultas de Logs Estructurados
```bash
# Buscar logs por operación específica
grep "GetSystemMetrics" logs/systemmonitor-*.txt

# Filtrar por nivel de error
grep "\"Level\":\"Error\"" logs/systemmonitor-*.txt

# Buscar requests lentos (>1000ms)
grep "\"Elapsed\":[1-9][0-9][0-9][0-9]" logs/systemmonitor-*.txt
```

### Configuración de Prometheus Scraping
```yaml
# prometheus.yml
scrape_configs:
  - job_name: 'systemmonitor'
    static_configs:
      - targets: ['localhost:5001']
    metrics_path: '/metrics'
    scrape_interval: 15s
```

### Queries de Prometheus
```promql
# CPU usage promedio en los últimos 5 minutos
avg_over_time(system_cpu_usage_percent[5m])

# Rate de requests HTTP por minuto
rate(http_requests_total[1m])

# Percentil 95 de duración de requests
histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))
```

## Mejores Prácticas Implementadas

### Logging
1. **Bootstrap Logger**: Captura logs críticos durante el startup
2. **Structured Logging**: Propiedades tipadas en lugar de string interpolation
3. **Scoped Context**: Contexto enriquecido por operación
4. **Exception Handling**: Logging detallado de errores con contexto
5. **Performance Logging**: Medición de duración de operaciones
6. **Sensitive Data**: Filtrado de información sensible en logs

### Telemetría
1. **Multi-Sink Strategy**: Console para desarrollo, File para persistencia, AI para producción
2. **Correlation IDs**: Seguimiento de requests end-to-end
3. **Custom Metrics**: Métricas específicas del dominio de negocio
4. **Health Monitoring**: Endpoints dedicados para health checks
5. **Resource Monitoring**: CPU, memoria, disco con alertas
6. **Garbage Collection**: Monitoreo y optimización de memoria

### Configuración
1. **Environment-Specific**: Configuración diferenciada por ambiente
2. **External Configuration**: Toda la configuración en archivos JSON
3. **Hot Reload**: Cambios de configuración sin reinicio
4. **Secrets Management**: Manejo seguro de connection strings
5. **Feature Flags**: Habilitación/deshabilitación de características

## Integración con Sistemas de Monitoreo

### Application Insights
- Telemetría automática de requests, dependencies, exceptions
- Custom events y metrics
- Live metrics stream
- Application map y dependency tracking

### ELK Stack (Elasticsearch, Logstash, Kibana)
- Ingesta de logs estructurados
- Dashboards personalizados
- Alertas basadas en patrones de logs
- Análisis de tendencias y anomalías

### Grafana + Prometheus
- Dashboards de métricas en tiempo real
- Alerting basado en thresholds
- Correlación de métricas y logs
- SLI/SLO monitoring

## Próximos Pasos y Extensiones

1. **Distributed Tracing**: Implementar OpenTelemetry para tracing distribuido
2. **Custom Metrics**: Agregar métricas específicas del dominio de negocio
3. **Alerting**: Configurar alertas basadas en métricas y logs
4. **Dashboards**: Crear dashboards de observabilidad con Grafana
5. **Log Analysis**: Implementar análisis automático de logs con ML
6. **Chaos Engineering**: Inyección de fallos para testing de observabilidad