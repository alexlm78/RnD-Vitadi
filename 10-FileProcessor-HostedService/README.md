# FileProcessor - Hosted Services y Background Processing

Este ejemplo completo demuestra cómo crear servicios de hosting de larga duración usando **Microsoft.Extensions.Hosting** en .NET 8, incluyendo diferentes tipos de background services y configuración para ejecutar como servicio de Windows/Linux.

## Conceptos Demostrados

### 1. Microsoft.Extensions.Hosting
- Configuración de un host de aplicación para servicios de consola
- Uso de `Host.CreateDefaultBuilder()` para configuración automática
- Gestión del ciclo de vida de la aplicación
- Graceful shutdown y manejo de señales del sistema
- Soporte para Windows Services y Linux systemd

### 2. Tipos de Background Services

#### BackgroundService (FileProcessorService)
Servicio principal que procesa archivos automáticamente:
- Monitoreo de directorios con `FileSystemWatcher`
- Procesamiento asíncrono de archivos
- Manejo de errores y recuperación
- Procesamiento específico por tipo de archivo

#### Timer-Based Service (TimerBasedService)
Servicio que ejecuta trabajo en intervalos regulares:
- Uso de `PeriodicTimer` para ejecución periódica
- Ideal para tareas de limpieza, health checks, sincronización
- Manejo de excepciones sin detener el servicio

#### Queued Background Service (QueuedBackgroundService)
Servicio que procesa elementos de una cola:
- Uso de `System.Threading.Channels` para cola thread-safe
- Patrón productor-consumidor
- Procesamiento de trabajo encolado desde otras partes de la aplicación

#### Lifecycle Service (LifecycleService)
Servicio que demuestra el ciclo de vida completo:
- Implementación de `IHostedService` directamente
- Manejo de eventos de inicio y parada
- Inicialización y limpieza de recursos

#### Application Lifetime Service (ApplicationLifetimeService)
Servicio que responde a eventos del ciclo de vida de la aplicación:
- Uso de `IHostApplicationLifetime`
- Callbacks para eventos de inicio, parada y finalización
- Útil para registro/desregistro de servicios externos

### 3. Configuración y Dependency Injection
- Configuración desde múltiples fuentes (JSON, variables de entorno, línea de comandos)
- Patrón Options para configuración fuertemente tipada
- Registro de servicios en el contenedor de DI
- Configuración específica por ambiente

### 4. Logging Estructurado
- Configuración de múltiples proveedores de logging
- Logging específico para producción (Event Log en Windows)
- Niveles de log configurables
- Logging contextual en servicios

## Estructura del Proyecto

```
FileProcessor.Service/
├── Configuration/
│   └── FileProcessorOptions.cs           # Opciones de configuración
├── Services/
│   ├── FileProcessorService.cs           # Servicio principal de procesamiento
│   ├── TimerBasedService.cs              # Servicio basado en timer
│   ├── QueuedBackgroundService.cs        # Servicio con cola de trabajo
│   ├── LifecycleService.cs               # Servicio de ciclo de vida
│   └── ApplicationLifetimeService.cs     # Servicio de eventos de aplicación
├── Program.cs                            # Configuración del host
├── appsettings.json                      # Configuración base
├── appsettings.Development.json          # Configuración para desarrollo
├── appsettings.Production.json           # Configuración para producción
├── FileProcessor.Service.csproj          # Proyecto con dependencias
├── fileprocessor.service                 # Archivo de servicio systemd
├── install-windows-service.ps1           # Script de instalación Windows
├── uninstall-windows-service.ps1         # Script de desinstalación Windows
├── install-linux-service.sh              # Script de instalación Linux
└── uninstall-linux-service.sh            # Script de desinstalación Linux
```

## Configuración

### appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "FileProcessor.Service": "Debug"
    }
  },
  "FileProcessor": {
    "InputDirectory": "./input",
    "OutputDirectory": "./output",
    "ProcessingIntervalSeconds": 30,
    "SupportedExtensions": [".txt", ".csv", ".json", ".xml"],
    "MaxFileSizeBytes": 10485760
  }
}
```

### Configuración de Servicios en Program.cs
```csharp
services.Configure<FileProcessorOptions>(
    context.Configuration.GetSection(FileProcessorOptions.SectionName));

// Registro de servicios de background
services.AddHostedService<FileProcessorService>();
services.AddHostedService<TimerBasedService>();
services.AddHostedService<QueuedBackgroundService>();
```

## Cómo Ejecutar

### Desarrollo Local
```bash
# Compilar el proyecto
dotnet build

# Ejecutar en modo desarrollo
dotnet run --environment Development

# Ejecutar con configuración personalizada
dotnet run --FileProcessor:ProcessingIntervalSeconds=10

# Ejecutar con logging detallado
dotnet run --Logging:LogLevel:Default=Debug
```

### Publicación para Producción
```bash
# Publicar para Windows (self-contained)
dotnet publish -c Release -r win-x64 --self-contained

# Publicar para Linux (self-contained)
dotnet publish -c Release -r linux-x64 --self-contained

# Publicar framework-dependent
dotnet publish -c Release
```

## Instalación como Servicio

### Windows Service

#### Instalación Automática
```powershell
# Ejecutar como Administrador
.\install-windows-service.ps1

# Con parámetros personalizados
.\install-windows-service.ps1 -ServiceName "MyFileProcessor" -DisplayName "My File Processor"
```

#### Instalación Manual
```powershell
# Crear el servicio
sc.exe create FileProcessorService binPath="C:\path\to\FileProcessor.Service.exe" start=auto

# Iniciar el servicio
sc.exe start FileProcessorService

# Verificar estado
sc.exe query FileProcessorService
```

#### Gestión del Servicio
```powershell
# Iniciar
Start-Service -Name FileProcessorService

# Detener
Stop-Service -Name FileProcessorService

# Ver estado
Get-Service -Name FileProcessorService

# Ver logs (Event Viewer)
Get-EventLog -LogName Application -Source FileProcessorService
```

### Linux systemd Service

#### Instalación Automática
```bash
# Ejecutar como root
sudo ./install-linux-service.sh
```

#### Instalación Manual
```bash
# Copiar archivos del servicio
sudo mkdir -p /opt/fileprocessor
sudo cp FileProcessor.Service /opt/fileprocessor/
sudo cp *.json /opt/fileprocessor/

# Crear usuario del servicio
sudo useradd --system --no-create-home fileprocessor

# Configurar permisos
sudo chown -R fileprocessor:fileprocessor /opt/fileprocessor
sudo chmod +x /opt/fileprocessor/FileProcessor.Service

# Instalar archivo de servicio
sudo cp fileprocessor.service /etc/systemd/system/
sudo systemctl daemon-reload
sudo systemctl enable fileprocessor
sudo systemctl start fileprocessor
```

#### Gestión del Servicio
```bash
# Iniciar
sudo systemctl start fileprocessor

# Detener
sudo systemctl stop fileprocessor

# Reiniciar
sudo systemctl restart fileprocessor

# Ver estado
sudo systemctl status fileprocessor

# Ver logs
sudo journalctl -u fileprocessor -f

# Habilitar inicio automático
sudo systemctl enable fileprocessor
```

## Ejemplos de Uso

### Procesamiento de Archivos
1. Coloca archivos en el directorio `input/`
2. El servicio los procesará automáticamente
3. Los archivos procesados aparecerán en `output/`
4. Los archivos con errores se moverán a `output/errors/`

### Tipos de Archivos Soportados
- **TXT**: Agrega números de línea y metadatos
- **CSV**: Valida formato y agrega información de filas
- **JSON**: Valida sintaxis y envuelve en metadatos
- **XML**: Valida estructura y agrega información de procesamiento

### Cola de Trabajo (QueuedBackgroundService)
```csharp
// Encolar trabajo desde otro servicio
await _taskQueue.QueueBackgroundWorkItemAsync(async token =>
{
    // Tu lógica de trabajo aquí
    await ProcessDataAsync(token);
});
```

## Patrones y Mejores Prácticas

### 1. BackgroundService vs IHostedService
- **BackgroundService**: Para servicios de larga duración con loop principal
- **IHostedService**: Para servicios con lógica de inicio/parada específica

### 2. Manejo de Errores
- Capturar excepciones para evitar que el servicio se detenga
- Logging detallado de errores
- Estrategias de recuperación (reintentos, circuit breaker)

### 3. Graceful Shutdown
- Usar `CancellationToken` para detectar shutdown
- Completar trabajo en progreso antes de terminar
- Liberar recursos apropiadamente

### 4. Configuración
- Usar el patrón Options para configuración tipada
- Soportar múltiples fuentes de configuración
- Validar configuración al inicio

### 5. Logging
- Usar logging estructurado
- Diferentes niveles según el ambiente
- Incluir contexto relevante en los logs

### 6. Monitoreo
- Implementar health checks
- Exponer métricas de rendimiento
- Alertas para errores críticos

## Troubleshooting

### Problemas Comunes

#### El servicio no inicia
- Verificar permisos de archivos y directorios
- Revisar logs de aplicación/sistema
- Validar configuración JSON
- Verificar dependencias de .NET

#### Archivos no se procesan
- Verificar permisos del directorio de entrada
- Revisar extensiones de archivo soportadas
- Verificar tamaño máximo de archivo
- Comprobar logs del servicio

#### Alto uso de CPU/memoria
- Revisar configuración de intervalos
- Verificar que no hay loops infinitos
- Monitorear cola de trabajo
- Ajustar configuración de logging

### Logs Importantes
```bash
# Windows Event Log
Get-EventLog -LogName Application -Source FileProcessorService

# Linux systemd logs
sudo journalctl -u fileprocessor --since "1 hour ago"

# Archivos de log de aplicación
tail -f /opt/fileprocessor/logs/app.log
```

## Conceptos Avanzados

### Host Builder Pattern
```csharp
var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) => {
        // Configuración personalizada
    })
    .ConfigureServices((context, services) => {
        // Registro de servicios
    })
    .ConfigureLogging((context, logging) => {
        // Configuración de logging
    })
    .UseWindowsService() // Soporte para Windows Service
    .UseSystemd()        // Soporte para Linux systemd
    .Build();
```

### Ciclo de Vida del Host
1. **Configuration**: Carga de configuración desde múltiples fuentes
2. **Service Registration**: Registro de servicios en DI container
3. **Host Build**: Construcción del host y validación
4. **Service Start**: Inicio de todos los hosted services
5. **Running**: Aplicación en ejecución, procesando trabajo
6. **Shutdown Signal**: Recepción de señal de parada
7. **Graceful Stop**: Parada ordenada de servicios
8. **Cleanup**: Liberación de recursos

### Integración con Otros Servicios
- **Health Checks**: Monitoreo de salud del servicio
- **Metrics**: Exposición de métricas con Prometheus
- **Distributed Tracing**: Trazabilidad con OpenTelemetry
- **Configuration Providers**: Azure Key Vault, AWS Parameter Store
- **Message Queues**: RabbitMQ, Azure Service Bus, AWS SQS

Este ejemplo proporciona una base sólida para crear servicios de background robustos y escalables en .NET 8, con soporte completo para deployment en Windows y Linux.