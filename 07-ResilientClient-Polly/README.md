# ResilientClient - Patrones de Resiliencia con Polly

Este proyecto demuestra cómo implementar patrones de resiliencia en aplicaciones .NET Core 8 usando **Polly** para consumir APIs externas de manera robusta y confiable.

## 🎯 Objetivos de Aprendizaje

- Implementar patrones de resiliencia con Polly
- Configurar políticas de retry, circuit breaker y timeout
- Manejar fallos transitorios en llamadas HTTP
- Implementar estrategias de fallback
- Configurar HttpClient con políticas de resiliencia
- Logging y monitoreo de patrones de resiliencia

## 🏗️ Arquitectura

```
ResilientClient.Api/
├── Controllers/           # Controllers de API
├── Services/             # Servicios de negocio
├── Models/               # Modelos de datos
├── Configuration/        # Opciones de configuración
├── Extensions/           # Extensiones de servicios
└── Properties/           # Configuración de launch
```

## 📦 Librerías Utilizadas

### Principales
- **Polly** (8.2.0) - Librería principal de resiliencia
- **Polly.Extensions.Http** (3.0.0) - Extensiones para HttpClient
- **Microsoft.Extensions.Http.Polly** (8.0.0) - Integración con DI

### Complementarias
- **Serilog.AspNetCore** - Logging estructurado
- **Swashbuckle.AspNetCore** - Documentación API

## 🔧 Configuración

### appsettings.json

```json
{
  "Resilience": {
    "Retry": {
      "MaxRetries": 3,
      "BaseDelaySeconds": 1.0,
      "MaxDelaySeconds": 30.0
    },
    "CircuitBreaker": {
      "FailureThreshold": 5,
      "DurationOfBreakSeconds": 30.0,
      "SamplingDuration": 10
    },
    "Timeout": {
      "TimeoutSeconds": 30.0
    }
  },
  "ExternalApis": {
    "Weather": {
      "BaseUrl": "https://api.openweathermap.org/data/2.5",
      "ApiKey": "your-api-key",
      "TimeoutSeconds": 30
    },
    "News": {
      "BaseUrl": "https://newsapi.org/v2",
      "ApiKey": "your-api-key",
      "TimeoutSeconds": 30
    }
  }
}
```

## 🛡️ Patrones de Resiliencia Implementados

### 1. Retry Policy (Política de Reintentos)

```csharp
HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(
            Math.Min(1.0 * Math.Pow(2, retryAttempt - 1), 30.0)
        )
    );
```

**Características:**
- Reintentos automáticos en errores transitorios
- Backoff exponencial con jitter
- Logging detallado de reintentos

### 2. Circuit Breaker (Cortocircuito)

```csharp
HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 5,
        durationOfBreak: TimeSpan.FromSeconds(30)
    );
```

**Estados:**
- **Cerrado**: Llamadas normales
- **Abierto**: Bloquea llamadas por tiempo determinado
- **Semi-abierto**: Permite llamadas de prueba

### 3. Timeout Policy (Política de Timeout)

```csharp
Policy.TimeoutAsync<HttpResponseMessage>(
    timeout: TimeSpan.FromSeconds(30),
    timeoutStrategy: TimeoutStrategy.Optimistic
);
```

**Características:**
- Timeout configurable por operación
- Estrategia optimista para mejor performance
- Cancelación automática de operaciones lentas

### 4. Fallback Strategy (Estrategia de Respaldo)

```csharp
// Fallback a nivel de política HTTP
Policy
    .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
    .Or<HttpRequestException>()
    .FallbackAsync(
        fallbackValue: new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(fallbackJson, Encoding.UTF8, "application/json")
        }
    );

// Fallback a nivel de servicio
public async Task<WeatherResponse?> GetWeatherWithFallbackAsync(string city)
{
    try
    {
        return await GetCurrentWeatherAsync(city);
    }
    catch (Exception)
    {
        // Fallback a datos en caché
        if (_cache.TryGetValue(city, out var cached))
            return cached;
            
        // Fallback a datos por defecto
        return GetDefaultWeatherData(city);
    }
}
```

### 5. Policy Wrapping (Combinación de Políticas)

```csharp
// Orden correcto: Fallback -> CircuitBreaker -> Retry -> Timeout
var policyWrap = Policy.WrapAsync(
    fallbackPolicy,
    circuitBreakerPolicy, 
    retryPolicy,
    timeoutPolicy
);
```

### 6. Jitter en Retry (Anti-Thundering Herd)

```csharp
sleepDurationProvider: retryAttempt =>
{
    var baseDelay = options.Retry.BaseDelaySeconds * Math.Pow(2, retryAttempt - 1);
    var jitter = random.NextDouble() * 0.1 * baseDelay; // 10% jitter
    var totalDelay = Math.Min(baseDelay + jitter, options.Retry.MaxDelaySeconds);
    return TimeSpan.FromSeconds(totalDelay);
}
```

## 🚀 Endpoints Disponibles

### Weather Controller

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/weather/current/{city}` | Clima actual (sin fallback) |
| GET | `/api/weather/resilient/{city}` | Clima con fallback simple |
| GET | `/api/weather/advanced/{city}` | Clima con resiliencia avanzada |
| GET | `/api/weather/multiple?cities=...` | Múltiples ciudades |
| GET | `/api/weather/test/{scenario}` | Probar escenarios de fallo |

### News Controller

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/news/latest` | Últimas noticias |
| GET | `/api/news/resilient` | Noticias con reintentos |
| GET | `/api/news/advanced` | Noticias con resiliencia avanzada |
| GET | `/api/news/by-categories` | Por categorías |
| GET | `/api/news/categories` | Categorías disponibles |
| GET | `/api/news/test/{scenario}` | Probar escenarios de fallo |

### Resilience Test Controller

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/resilience/transient-error/{errorType}` | Simular errores transitorios |
| GET | `/api/resilience/circuit-breaker/status` | Estado del circuit breaker |
| POST | `/api/resilience/bulk-test` | Test masivo con estadísticas |
| GET | `/api/resilience/fallback/{fallbackType}` | Demostrar estrategias de fallback |
| GET | `/api/resilience/patterns` | Información sobre patrones implementados |

### **🆕 External API Controller - Consolidado de Patrones de Resiliencia**

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/external/dashboard` | Dashboard consolidado con múltiples APIs |
| POST | `/api/external/retry-demo` | Demostración interactiva del patrón Retry |
| POST | `/api/external/circuit-breaker-demo` | Demostración del patrón Circuit Breaker |
| GET | `/api/external/fallback-demo/{strategy}` | Demostración de estrategias de Fallback |
| GET | `/api/external/resilience-metrics` | Métricas consolidadas de resiliencia |
| POST | `/api/external/comprehensive-test` | Test completo de todos los patrones |

## 🔍 Monitoreo y Logging

### Logging Estructurado con Serilog

```csharp
_logger.LogWarning("Reintento {RetryCount} después de {Delay}ms debido a: {Exception}",
    retryCount, timespan.TotalMilliseconds, outcome.Exception.Message);
```

### Métricas de Resiliencia

- Número de reintentos por endpoint
- Estado del circuit breaker
- Tiempos de respuesta
- Tasa de éxito/fallo

## 🧪 Pruebas y Desarrollo

### Ejecutar la Aplicación

```bash
cd 07-ResilientClient-Polly/ResilientClient.Api
dotnet run
```

### Probar Endpoints Básicos

```bash
# Clima resiliente básico
curl https://localhost:7070/api/weather/resilient/Madrid

# Clima con resiliencia avanzada
curl https://localhost:7070/api/weather/advanced/Madrid

# Noticias con reintentos
curl https://localhost:7070/api/news/resilient?category=technology

# Noticias con resiliencia avanzada
curl https://localhost:7070/api/news/advanced?category=technology

# Health check
curl https://localhost:7070/health
```

### **🆕 Probar Patrones de Resiliencia Consolidados**

```bash
# Dashboard consolidado con múltiples APIs
curl "https://localhost:7070/api/external/dashboard?city=Madrid&newsCategory=technology"

# Demostración interactiva del patrón Retry
curl -X POST "https://localhost:7070/api/external/retry-demo?maxRetries=5&baseDelayMs=500&errorType=intermittent"

# Demostración del patrón Circuit Breaker
curl -X POST "https://localhost:7070/api/external/circuit-breaker-demo?requestCount=25&failureRate=60"

# Demostración de estrategias de Fallback
curl "https://localhost:7070/api/external/fallback-demo/cache?forceError=true"
curl "https://localhost:7070/api/external/fallback-demo/default?forceError=true"
curl "https://localhost:7070/api/external/fallback-demo/alternative?forceError=true"
curl "https://localhost:7070/api/external/fallback-demo/hybrid?forceError=true"

# Métricas consolidadas de resiliencia
curl https://localhost:7070/api/external/resilience-metrics

# Test completo de todos los patrones (30 segundos, 3 req/s)
curl -X POST "https://localhost:7070/api/external/comprehensive-test?testDurationSeconds=30&requestsPerSecond=3"
```

### Probar Patrones de Resiliencia Individuales

```bash
# Simular diferentes tipos de errores transitorios
curl https://localhost:7070/api/resilience/transient-error/http500
curl https://localhost:7070/api/resilience/transient-error/timeout
curl https://localhost:7070/api/resilience/transient-error/intermittent

# Ver estado del circuit breaker
curl https://localhost:7070/api/resilience/circuit-breaker/status

# Test masivo con estadísticas
curl -X POST "https://localhost:7070/api/resilience/bulk-test?requestCount=20&errorRate=40"

# Probar diferentes estrategias de fallback
curl https://localhost:7070/api/resilience/fallback/cache
curl https://localhost:7070/api/resilience/fallback/default
curl https://localhost:7070/api/resilience/fallback/alternative

# Ver información sobre patrones implementados
curl https://localhost:7070/api/resilience/patterns
```

### Probar Escenarios Específicos por Servicio

```bash
# Escenarios de clima
curl https://localhost:7070/api/weather/test/timeout
curl https://localhost:7070/api/weather/test/intermittent
curl https://localhost:7070/api/weather/test/circuitbreaker

# Escenarios de noticias
curl https://localhost:7070/api/news/test/error?category=tech
curl https://localhost:7070/api/news/test/slow?category=sports
curl https://localhost:7070/api/news/test/fallback?category=business
```

## 📚 Conceptos Clave

### 1. Errores Transitorios

Errores temporales que pueden resolverse con reintentos:
- Timeouts de red
- Errores HTTP 5xx
- Errores de conectividad

### 2. Combinación de Políticas

```csharp
services.AddHttpClient<IWeatherService, WeatherService>()
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy())
    .AddPolicyHandler(GetTimeoutPolicy());
```

### 3. Contexto de Polly

```csharp
onRetry: (outcome, timespan, retryCount, context) =>
{
    var logger = context.GetLogger();
    logger?.LogWarning("Reintento {RetryCount}", retryCount);
}
```

## 🔧 Ejemplos de Configuración de Políticas

### 1. Política de Retry Avanzada con Backoff Exponencial

```csharp
public static IAsyncPolicy<HttpResponseMessage> GetAdvancedRetryPolicy()
{
    var random = new Random();
    
    return HttpPolicyExtensions
        .HandleTransientHttpError() // HTTP 5XX, 408, HttpRequestException
        .Or<TaskCanceledException>() // Timeouts
        .Or<SocketException>() // Errores de red
        .WaitAndRetryAsync(
            retryCount: 4, // Máximo 4 reintentos
            sleepDurationProvider: retryAttempt =>
            {
                // Backoff exponencial: 1s, 2s, 4s, 8s
                var baseDelay = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1));
                
                // Agregar jitter (±25%) para evitar thundering herd
                var jitter = TimeSpan.FromMilliseconds(
                    random.Next(-250, 250) * retryAttempt);
                
                // Limitar el delay máximo a 30 segundos
                var totalDelay = baseDelay + jitter;
                return totalDelay > TimeSpan.FromSeconds(30) 
                    ? TimeSpan.FromSeconds(30) 
                    : totalDelay;
            },
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                var logger = context.GetLogger();
                var serviceName = context.GetValueOrDefault("ServiceName", "Unknown");
                
                if (outcome.Exception != null)
                {
                    logger?.LogWarning(
                        "[{ServiceName}] Reintento {RetryCount}/4 en {Delay}ms. Error: {Error}",
                        serviceName, retryCount, timespan.TotalMilliseconds, 
                        outcome.Exception.Message);
                }
                else
                {
                    logger?.LogWarning(
                        "[{ServiceName}] Reintento {RetryCount}/4 en {Delay}ms. Status: {StatusCode}",
                        serviceName, retryCount, timespan.TotalMilliseconds, 
                        outcome.Result?.StatusCode);
                }
            });
}
```

### 2. Circuit Breaker con Estados Personalizados

```csharp
public static IAsyncPolicy<HttpResponseMessage> GetAdvancedCircuitBreakerPolicy(string serviceName)
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .Or<TaskCanceledException>()
        .AdvancedCircuitBreakerAsync(
            // Configuración avanzada basada en porcentaje de fallos
            failureThreshold: 0.5, // 50% de fallos
            samplingDuration: TimeSpan.FromSeconds(10), // Ventana de muestreo
            minimumThroughput: 3, // Mínimo 3 requests para evaluar
            durationOfBreak: TimeSpan.FromSeconds(30), // Tiempo abierto
            
            onBreak: (exception, duration) =>
            {
                Console.WriteLine($"🔴 [{serviceName}] Circuit Breaker ABIERTO por {duration.TotalSeconds}s");
                Console.WriteLine($"    Razón: {exception.Exception?.Message ?? exception.Result?.StatusCode.ToString()}");
            },
            onReset: () =>
            {
                Console.WriteLine($"🟢 [{serviceName}] Circuit Breaker CERRADO - Operación normal restablecida");
            },
            onHalfOpen: () =>
            {
                Console.WriteLine($"🟡 [{serviceName}] Circuit Breaker SEMI-ABIERTO - Probando requests");
            });
}
```

### 3. Política de Timeout con Estrategias Diferentes

```csharp
// Timeout Optimista (recomendado para la mayoría de casos)
public static IAsyncPolicy<HttpResponseMessage> GetOptimisticTimeoutPolicy(int timeoutSeconds = 30)
{
    return Policy.TimeoutAsync<HttpResponseMessage>(
        timeout: TimeSpan.FromSeconds(timeoutSeconds),
        timeoutStrategy: TimeoutStrategy.Optimistic, // No cancela threads
        onTimeoutAsync: async (context, timeout, task) =>
        {
            var logger = context.GetLogger();
            var serviceName = context.GetValueOrDefault("ServiceName", "Unknown");
            
            logger?.LogWarning(
                "[{ServiceName}] ⏰ Timeout de {TimeoutSeconds}s alcanzado",
                serviceName, timeout.TotalSeconds);
            
            await Task.CompletedTask;
        });
}

// Timeout Pesimista (para operaciones que deben cancelarse)
public static IAsyncPolicy<HttpResponseMessage> GetPessimisticTimeoutPolicy(int timeoutSeconds = 10)
{
    return Policy.TimeoutAsync<HttpResponseMessage>(
        timeout: TimeSpan.FromSeconds(timeoutSeconds),
        timeoutStrategy: TimeoutStrategy.Pessimistic, // Cancela threads activamente
        onTimeoutAsync: async (context, timeout, task) =>
        {
            var logger = context.GetLogger();
            logger?.LogError("⏰ Timeout pesimista - operación cancelada forzosamente");
            await Task.CompletedTask;
        });
}
```

### 4. Política de Fallback Multinivel

```csharp
public static IAsyncPolicy<HttpResponseMessage> GetMultiLevelFallbackPolicy()
{
    // Nivel 1: Fallback a caché
    var cacheFallback = Policy
        .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .Or<HttpRequestException>()
        .FallbackAsync(
            fallbackAction: async (context, ct) =>
            {
                var cacheService = context.GetValueOrDefault("CacheService") as ICacheService;
                var cacheKey = context.GetValueOrDefault("CacheKey") as string;
                
                if (cacheService != null && cacheKey != null)
                {
                    var cachedData = await cacheService.GetAsync(cacheKey, ct);
                    if (cachedData != null)
                    {
                        return new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent(cachedData, Encoding.UTF8, "application/json")
                        };
                    }
                }
                
                throw new InvalidOperationException("Cache fallback failed");
            },
            onFallbackAsync: async (result, context) =>
            {
                var logger = context.GetLogger();
                logger?.LogInformation("📦 Usando datos de caché como fallback");
                await Task.CompletedTask;
            });
    
    // Nivel 2: Fallback a servicio alternativo
    var alternativeServiceFallback = Policy
        .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .Or<HttpRequestException>()
        .Or<InvalidOperationException>() // Del fallback anterior
        .FallbackAsync(
            fallbackAction: async (context, ct) =>
            {
                var alternativeService = context.GetValueOrDefault("AlternativeService") as IAlternativeService;
                if (alternativeService != null)
                {
                    var alternativeData = await alternativeService.GetDataAsync(ct);
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(alternativeData, Encoding.UTF8, "application/json")
                    };
                }
                
                throw new InvalidOperationException("Alternative service fallback failed");
            },
            onFallbackAsync: async (result, context) =>
            {
                var logger = context.GetLogger();
                logger?.LogInformation("🔄 Usando servicio alternativo como fallback");
                await Task.CompletedTask;
            });
    
    // Nivel 3: Fallback a valores por defecto
    var defaultValuesFallback = Policy
        .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .Or<HttpRequestException>()
        .Or<InvalidOperationException>()
        .FallbackAsync(
            fallbackValue: new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                {
                    "message": "Servicio temporalmente no disponible",
                    "fallbackLevel": "default",
                    "timestamp": "2024-01-01T00:00:00Z"
                }
                """, Encoding.UTF8, "application/json")
            },
            onFallbackAsync: async (result, context) =>
            {
                var logger = context.GetLogger();
                logger?.LogWarning("⚠️ Usando valores por defecto - todos los fallbacks anteriores fallaron");
                await Task.CompletedTask;
            });
    
    // Combinar todos los niveles de fallback
    return Policy.WrapAsync(defaultValuesFallback, alternativeServiceFallback, cacheFallback);
}
```

### 5. Política de Bulkhead para Aislamiento de Recursos

```csharp
public static IAsyncPolicy<HttpResponseMessage> GetBulkheadPolicy(string serviceName)
{
    return Policy.BulkheadAsync<HttpResponseMessage>(
        maxParallelization: 5, // Máximo 5 operaciones paralelas
        maxQueuingActions: 10, // Máximo 10 operaciones en cola
        onBulkheadRejectedAsync: async (context) =>
        {
            var logger = context.GetLogger();
            logger?.LogWarning(
                "🚧 [{ServiceName}] Bulkhead rechazó request - límite de concurrencia alcanzado",
                serviceName);
            
            // Opcional: registrar métricas de rechazo
            var metricsService = context.GetValueOrDefault("MetricsService") as IMetricsService;
            metricsService?.IncrementCounter($"{serviceName}.bulkhead.rejected");
            
            await Task.CompletedTask;
        });
}
```

### 6. Política Combinada Completa (Policy Wrap)

```csharp
public static IAsyncPolicy<HttpResponseMessage> GetComprehensivePolicy(string serviceName)
{
    // Definir todas las políticas individuales
    var fallbackPolicy = GetMultiLevelFallbackPolicy();
    var circuitBreakerPolicy = GetAdvancedCircuitBreakerPolicy(serviceName);
    var retryPolicy = GetAdvancedRetryPolicy();
    var timeoutPolicy = GetOptimisticTimeoutPolicy(30);
    var bulkheadPolicy = GetBulkheadPolicy(serviceName);
    
    // Orden crítico de políticas (de exterior a interior):
    // 1. Bulkhead (limita concurrencia)
    // 2. Fallback (maneja fallos finales)
    // 3. Circuit Breaker (previene llamadas a servicios fallidos)
    // 4. Retry (reintenta operaciones fallidas)
    // 5. Timeout (limita tiempo de operación)
    
    return Policy.WrapAsync(
        bulkheadPolicy,
        fallbackPolicy,
        circuitBreakerPolicy,
        retryPolicy,
        timeoutPolicy
    );
}
```

### 7. Configuración Dinámica de Políticas

```csharp
public class DynamicPolicyConfiguration
{
    public static IAsyncPolicy<HttpResponseMessage> CreatePolicy(
        string serviceName, 
        IConfiguration configuration,
        IServiceProvider serviceProvider)
    {
        var section = configuration.GetSection($"Resilience:Services:{serviceName}");
        
        // Leer configuración específica del servicio
        var retryConfig = section.GetSection("Retry");
        var circuitBreakerConfig = section.GetSection("CircuitBreaker");
        var timeoutConfig = section.GetSection("Timeout");
        
        var policies = new List<IAsyncPolicy<HttpResponseMessage>>();
        
        // Agregar políticas según configuración
        if (retryConfig.GetValue<bool>("Enabled", true))
        {
            policies.Add(CreateRetryPolicy(retryConfig, serviceName));
        }
        
        if (circuitBreakerConfig.GetValue<bool>("Enabled", true))
        {
            policies.Add(CreateCircuitBreakerPolicy(circuitBreakerConfig, serviceName));
        }
        
        if (timeoutConfig.GetValue<bool>("Enabled", true))
        {
            policies.Add(CreateTimeoutPolicy(timeoutConfig));
        }
        
        // Combinar políticas dinámicamente
        return policies.Count switch
        {
            0 => Policy.NoOpAsync<HttpResponseMessage>(),
            1 => policies[0],
            _ => Policy.WrapAsync(policies.ToArray())
        };
    }
    
    private static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy(
        IConfigurationSection config, string serviceName)
    {
        var maxRetries = config.GetValue<int>("MaxRetries", 3);
        var baseDelayMs = config.GetValue<int>("BaseDelayMs", 1000);
        var maxDelayMs = config.GetValue<int>("MaxDelayMs", 30000);
        
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: maxRetries,
                sleepDurationProvider: retryAttempt =>
                {
                    var delay = Math.Min(
                        baseDelayMs * Math.Pow(2, retryAttempt - 1),
                        maxDelayMs);
                    return TimeSpan.FromMilliseconds(delay);
                },
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    Console.WriteLine($"[{serviceName}] Retry {retryCount}/{maxRetries}");
                });
    }
    
    // Métodos similares para otras políticas...
}
```

### 8. Configuración en appsettings.json para Políticas Dinámicas

```json
{
  "Resilience": {
    "Services": {
      "WeatherService": {
        "Retry": {
          "Enabled": true,
          "MaxRetries": 3,
          "BaseDelayMs": 1000,
          "MaxDelayMs": 30000
        },
        "CircuitBreaker": {
          "Enabled": true,
          "FailureThreshold": 0.5,
          "SamplingDurationSeconds": 10,
          "MinimumThroughput": 3,
          "DurationOfBreakSeconds": 30
        },
        "Timeout": {
          "Enabled": true,
          "TimeoutSeconds": 30,
          "Strategy": "Optimistic"
        },
        "Bulkhead": {
          "Enabled": false,
          "MaxParallelization": 5,
          "MaxQueuingActions": 10
        }
      },
      "NewsService": {
        "Retry": {
          "Enabled": true,
          "MaxRetries": 5,
          "BaseDelayMs": 500,
          "MaxDelayMs": 15000
        },
        "CircuitBreaker": {
          "Enabled": true,
          "FailureThreshold": 0.6,
          "SamplingDurationSeconds": 15,
          "MinimumThroughput": 5,
          "DurationOfBreakSeconds": 45
        },
        "Timeout": {
          "Enabled": true,
          "TimeoutSeconds": 25,
          "Strategy": "Optimistic"
        }
      }
    }
  }
}
```

## 🎓 Ejercicios Prácticos

### **Ejercicio 1: Configuración Básica de Políticas**
1. **Objetivo:** Entender cómo configurar políticas básicas
2. **Pasos:**
   - Modifica los valores de retry en `appsettings.json`
   - Cambia `MaxRetries` de 3 a 5 y `BaseDelaySeconds` de 1.0 a 0.5
   - Ejecuta `curl https://localhost:7070/api/resilience/transient-error/intermittent` varias veces
   - Observa los logs para ver el comportamiento de retry
3. **Resultado esperado:** Más reintentos con delays más cortos

### **Ejercicio 2: Demostración de Circuit Breaker**
1. **Objetivo:** Ver el circuit breaker en acción
2. **Pasos:**
   - Ejecuta múltiples requests que fallen: `curl https://localhost:7070/api/resilience/transient-error/http500`
   - Después de 5 fallos, el circuit breaker se abrirá
   - Verifica el estado: `curl https://localhost:7070/api/resilience/circuit-breaker/status`
   - Espera 30 segundos y vuelve a verificar el estado
3. **Resultado esperado:** Estados "Closed" → "Open" → "HalfOpen" → "Closed"

### **Ejercicio 3: Comparación de Estrategias de Fallback**
1. **Objetivo:** Comparar diferentes estrategias de fallback
2. **Pasos:**
   ```bash
   # Probar cada estrategia con error forzado
   curl "https://localhost:7070/api/external/fallback-demo/cache?forceError=true"
   curl "https://localhost:7070/api/external/fallback-demo/default?forceError=true"
   curl "https://localhost:7070/api/external/fallback-demo/alternative?forceError=true"
   curl "https://localhost:7070/api/external/fallback-demo/hybrid?forceError=true"
   ```
3. **Resultado esperado:** Diferentes fuentes de datos de fallback

### **Ejercicio 4: Test de Carga con Patrones de Resiliencia**
1. **Objetivo:** Evaluar comportamiento bajo carga
2. **Pasos:**
   ```bash
   # Test de 60 segundos con 5 requests por segundo
   curl -X POST "https://localhost:7070/api/external/comprehensive-test?testDurationSeconds=60&requestsPerSecond=5"
   ```
3. **Análisis:** Revisar métricas de éxito, tiempos de respuesta y activación de políticas

### **Ejercicio 5: Configuración Personalizada de Políticas**
1. **Objetivo:** Crear políticas personalizadas
2. **Pasos:**
   - Crea una nueva configuración en `appsettings.json`:
   ```json
   {
     "Resilience": {
       "CustomService": {
         "Retry": {
           "MaxRetries": 2,
           "BaseDelaySeconds": 2.0,
           "MaxDelaySeconds": 10.0
         },
         "CircuitBreaker": {
           "FailureThreshold": 3,
           "DurationOfBreakSeconds": 60.0
         }
       }
     }
   }
   ```
   - Modifica el código para usar esta configuración
   - Prueba el comportamiento

### **Ejercicio 6: Implementar Fallback Avanzado**
1. **Objetivo:** Crear un sistema de fallback multinivel
2. **Pasos:**
   - Implementa un servicio de caché simple
   - Crea un servicio alternativo mock
   - Implementa la lógica de fallback: API → Caché → Alternativo → Default
   - Prueba cada nivel de fallback

### **Ejercicio 7: Monitoreo y Métricas**
1. **Objetivo:** Implementar monitoreo avanzado
2. **Pasos:**
   - Agrega contadores para cada tipo de política activada
   - Implementa métricas de tiempo de respuesta
   - Crea un endpoint de health check que incluya estado de circuit breakers
   - Visualiza las métricas en tiempo real

### **Ejercicio 8: Escenarios de Fallo Realistas**
1. **Objetivo:** Simular escenarios del mundo real
2. **Pasos:**
   - Simula degradación gradual del servicio (aumentar latencia progresivamente)
   - Simula fallos intermitentes con patrones específicos
   - Simula recuperación gradual del servicio
   - Analiza cómo responden las políticas a cada escenario

## 🔬 Escenarios Avanzados de Testing

### **Escenario 1: Thundering Herd Prevention**
```bash
# Ejecutar múltiples requests simultáneas para ver el jitter en acción
for i in {1..10}; do
  curl https://localhost:7070/api/resilience/transient-error/http500 &
done
wait
```

### **Escenario 2: Circuit Breaker bajo Carga**
```bash
# Generar carga que active el circuit breaker
curl -X POST "https://localhost:7070/api/external/circuit-breaker-demo?requestCount=30&failureRate=80"
```

### **Escenario 3: Fallback en Cascada**
```bash
# Probar fallback híbrido que prueba múltiples fuentes
curl "https://localhost:7070/api/external/fallback-demo/hybrid?forceError=true"
```

### **Escenario 4: Recovery Testing**
```bash
# Simular recuperación después de fallos
# 1. Generar fallos para abrir circuit breaker
curl https://localhost:7070/api/resilience/transient-error/http500
curl https://localhost:7070/api/resilience/transient-error/http500
curl https://localhost:7070/api/resilience/transient-error/http500
curl https://localhost:7070/api/resilience/transient-error/http500
curl https://localhost:7070/api/resilience/transient-error/http500

# 2. Verificar estado abierto
curl https://localhost:7070/api/resilience/circuit-breaker/status

# 3. Esperar y probar recuperación
sleep 35
curl https://localhost:7070/api/resilience/transient-error/success
```

## 📊 Análisis de Resultados

### **Métricas Clave a Monitorear**

1. **Tasa de Éxito/Fallo**
   - Porcentaje de requests exitosas vs fallidas
   - Impacto de las políticas de resiliencia en la tasa de éxito

2. **Latencia y Tiempos de Respuesta**
   - P50, P95, P99 de tiempos de respuesta
   - Impacto de reintentos en la latencia total

3. **Activación de Políticas**
   - Frecuencia de activación de retry, circuit breaker, fallback
   - Duración de estados de circuit breaker

4. **Throughput**
   - Requests por segundo procesadas
   - Impacto de bulkhead en el throughput

### **Interpretación de Resultados**

```bash
# Obtener métricas consolidadas
curl https://localhost:7070/api/external/resilience-metrics

# Ejemplo de respuesta:
{
  "timestamp": "2024-01-15T10:30:00Z",
  "circuitBreakerStatus": {
    "state": "Closed",
    "failureCount": 2,
    "isHealthy": true
  },
  "services": {
    "Weather": {
      "isHealthy": true,
      "responseTimeMs": 150,
      "successRate": 95.5,
      "errorRate": 4.5
    }
  },
  "overallHealth": {
    "isHealthy": true,
    "averageResponseTime": 175,
    "overallSuccessRate": 93.9
  }
}
```

## 🔧 Configuración de APIs Externas

### Para usar APIs reales:

1. **OpenWeatherMap API:**
   ```json
   "Weather": {
     "BaseUrl": "https://api.openweathermap.org/data/2.5",
     "ApiKey": "tu-api-key-aqui"
   }
   ```

2. **NewsAPI:**
   ```json
   "News": {
     "BaseUrl": "https://newsapi.org/v2",
     "ApiKey": "tu-api-key-aqui"
   }
   ```

## 🚨 Mejores Prácticas

### **1. Configuración de Políticas**
- **Ajusta valores según SLA:** Configura timeouts y reintentos basándote en el SLA del servicio externo
- **Configuración por ambiente:** Usa valores más agresivos en desarrollo, más conservadores en producción
- **Monitoreo continuo:** Ajusta políticas basándote en métricas reales de producción
- **Evita over-engineering:** No agregues políticas innecesarias que compliquen el sistema

### **2. Logging y Monitoreo**
- **Structured logging:** Usa campos estructurados para facilitar análisis
- **Contexto completo:** Incluye información de request, usuario, y operación
- **Alertas inteligentes:** Configura alertas basadas en tendencias, no eventos únicos
- **Dashboards en tiempo real:** Visualiza métricas de resiliencia continuamente

### **3. Testing y Validación**
- **Chaos Engineering:** Introduce fallos controlados en entornos de prueba
- **Load testing:** Prueba políticas bajo carga real
- **Escenarios realistas:** Simula patrones de fallo del mundo real
- **Validación de fallback:** Asegúrate que los fallbacks proporcionan valor real

### **4. Performance y Optimización**
- **Balance latencia vs resiliencia:** Evita políticas que agreguen latencia innecesaria
- **Timeouts apropiados:** Configura timeouts específicos por operación
- **Jitter en reintentos:** Previene thundering herd con jitter aleatorio
- **Circuit breaker tuning:** Ajusta umbrales basándote en patrones de tráfico

### **5. Orden de Políticas (Policy Wrapping)**
```
Orden correcto (exterior → interior):
1. Bulkhead (limita concurrencia)
2. Fallback (maneja fallos finales)  
3. Circuit Breaker (previene llamadas innecesarias)
4. Retry (reintenta fallos transitorios)
5. Timeout (limita duración de operaciones)
```

### **6. Manejo de Errores**
- **Clasifica errores:** Distingue entre errores transitorios y permanentes
- **Fallback apropiado:** Proporciona experiencia degradada pero útil
- **Logging de contexto:** Registra información suficiente para debugging
- **Métricas de negocio:** Mide impacto en métricas de negocio, no solo técnicas

## 🔧 Troubleshooting

### **Problemas Comunes y Soluciones**

#### **1. Circuit Breaker se Abre Demasiado Rápido**
```
Síntomas: Circuit breaker se abre con pocos fallos
Causas: Threshold muy bajo, ventana de muestreo muy pequeña
Solución: Aumentar FailureThreshold o SamplingDuration
```

#### **2. Reintentos Excesivos**
```
Síntomas: Latencia muy alta, logs llenos de reintentos
Causas: MaxRetries muy alto, errores no transitorios
Solución: Reducir MaxRetries, mejorar clasificación de errores
```

#### **3. Fallback No Se Activa**
```
Síntomas: Errores llegan al cliente sin fallback
Causas: Orden incorrecto de políticas, excepciones no manejadas
Solución: Verificar policy wrapping, agregar manejo de excepciones
```

#### **4. Timeouts Inconsistentes**
```
Síntomas: Algunas requests timeout, otras no
Causas: Timeout muy bajo, operaciones variables
Solución: Analizar P95/P99 de latencia, ajustar timeout
```

#### **5. Thundering Herd**
```
Síntomas: Picos de tráfico después de fallos
Causas: Falta de jitter en reintentos
Solución: Implementar jitter aleatorio en delays
```

### **Debugging con Logs**

```csharp
// Ejemplo de logging estructurado para debugging
_logger.LogWarning("Polly policy executed: {PolicyType} for {ServiceName}", 
    "Retry", "WeatherService", new { 
        RetryCount = 2,
        Duration = "1500ms",
        Exception = ex.Message,
        RequestId = context.RequestId
    });
```

### **Métricas de Diagnóstico**

```bash
# Verificar estado de circuit breakers
curl https://localhost:7070/api/resilience/circuit-breaker/status

# Obtener métricas consolidadas
curl https://localhost:7070/api/external/resilience-metrics

# Test de diagnóstico rápido
curl https://localhost:7070/api/resilience/patterns
```

### **Configuración de Desarrollo vs Producción**

```json
// Development - Más agresivo para testing
{
  "Resilience": {
    "Retry": {
      "MaxRetries": 5,
      "BaseDelaySeconds": 0.5
    },
    "CircuitBreaker": {
      "FailureThreshold": 3,
      "DurationOfBreakSeconds": 10
    }
  }
}

// Production - Más conservador
{
  "Resilience": {
    "Retry": {
      "MaxRetries": 3,
      "BaseDelaySeconds": 1.0
    },
    "CircuitBreaker": {
      "FailureThreshold": 5,
      "DurationOfBreakSeconds": 30
    }
  }
}
```

## 📈 Monitoreo en Producción

### **Alertas Recomendadas**

1. **Circuit Breaker Abierto > 5 minutos**
2. **Tasa de error > 10% por 5 minutos**
3. **Latencia P95 > 2x baseline por 10 minutos**
4. **Fallback activado > 50% del tiempo por 15 minutos**

### **Dashboards Clave**

1. **Resiliencia Overview:**
   - Estado de circuit breakers
   - Tasa de activación de políticas
   - Distribución de errores por servicio

2. **Performance Impact:**
   - Latencia con/sin políticas
   - Throughput impact
   - Resource utilization

3. **Business Impact:**
   - Tasa de éxito de operaciones críticas
   - Experiencia de usuario degradada
   - Revenue impact de fallos

## 📖 Recursos Adicionales

- [Documentación oficial de Polly](https://github.com/App-vNext/Polly)
- [Patrones de resiliencia en microservicios](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/)
- [Circuit Breaker Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/circuit-breaker)

## 🤝 Contribución

Este proyecto es parte de una serie de ejemplos educativos. Para contribuir:

1. Fork el repositorio
2. Crea una rama para tu feature
3. Implementa mejoras o correcciones
4. Envía un Pull Request

---

**Nota:** Este ejemplo usa endpoints de prueba en desarrollo. Para producción, configura las APIs reales y ajusta las políticas según tus necesidades específicas.