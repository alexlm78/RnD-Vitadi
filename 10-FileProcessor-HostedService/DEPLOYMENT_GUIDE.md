# Guía de Deployment - FileProcessor Service

Esta guía detalla cómo desplegar el FileProcessor Service en diferentes entornos de producción.

## Preparación para Deployment

### 1. Compilación para Producción

#### Self-Contained Deployment (Recomendado)
```bash
# Windows x64
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true

# Linux x64
dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true

# macOS x64
dotnet publish -c Release -r osx-x64 --self-contained -p:PublishSingleFile=true
```

#### Framework-Dependent Deployment
```bash
# Requiere .NET 8 Runtime instalado en el servidor
dotnet publish -c Release
```

### 2. Configuración de Producción

Crear `appsettings.Production.json` con configuración específica:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "FileProcessor": {
    "InputDirectory": "/opt/fileprocessor/input",
    "OutputDirectory": "/opt/fileprocessor/output",
    "ProcessingIntervalSeconds": 60,
    "MaxFileSizeBytes": 52428800
  }
}
```

## Deployment en Windows

### Opción 1: Windows Service (Recomendado)

#### Instalación Automática
```powershell
# 1. Copiar archivos al servidor
Copy-Item -Path ".\publish\*" -Destination "C:\Services\FileProcessor\" -Recurse

# 2. Ejecutar script de instalación como Administrador
cd "C:\Services\FileProcessor"
.\install-windows-service.ps1
```

#### Instalación Manual
```powershell
# 1. Crear el servicio
sc.exe create FileProcessorService `
  binPath="C:\Services\FileProcessor\FileProcessor.Service.exe" `
  start=auto `
  DisplayName="File Processor Service" `
  description="Processes files automatically in the background"

# 2. Configurar recuperación en caso de fallo
sc.exe failure FileProcessorService reset=86400 actions=restart/5000/restart/10000/restart/30000

# 3. Iniciar el servicio
sc.exe start FileProcessorService
```

#### Configuración de Seguridad
```powershell
# Crear usuario dedicado para el servicio
net user FileProcessorSvc /add /passwordreq:yes
net localgroup "Log on as a service" FileProcessorSvc /add

# Configurar el servicio para usar el usuario dedicado
sc.exe config FileProcessorService obj=".\FileProcessorSvc" password="SecurePassword123!"
```

### Opción 2: Aplicación de Consola con Task Scheduler

#### Crear Tarea Programada
```powershell
# Crear tarea que se ejecute al inicio del sistema
$action = New-ScheduledTaskAction -Execute "C:\Services\FileProcessor\FileProcessor.Service.exe"
$trigger = New-ScheduledTaskTrigger -AtStartup
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable
$principal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -LogonType ServiceAccount

Register-ScheduledTask -TaskName "FileProcessorService" -Action $action -Trigger $trigger -Settings $settings -Principal $principal
```

### Monitoreo en Windows

#### Event Viewer
- Logs de aplicación: `Windows Logs > Application`
- Filtrar por fuente: `FileProcessorService`

#### Performance Monitor
```powershell
# Crear contador personalizado para monitoreo
Get-Counter "\Process(FileProcessor.Service)\% Processor Time"
Get-Counter "\Process(FileProcessor.Service)\Working Set"
```

## Deployment en Linux

### Opción 1: systemd Service (Recomendado)

#### Instalación Automática
```bash
# 1. Copiar archivos al servidor
sudo mkdir -p /opt/fileprocessor
sudo cp -r ./publish/* /opt/fileprocessor/

# 2. Ejecutar script de instalación
sudo ./install-linux-service.sh
```

#### Instalación Manual
```bash
# 1. Crear usuario del servicio
sudo useradd --system --no-create-home --shell /bin/false fileprocessor

# 2. Configurar directorios y permisos
sudo mkdir -p /opt/fileprocessor/{input,output,logs}
sudo chown -R fileprocessor:fileprocessor /opt/fileprocessor
sudo chmod +x /opt/fileprocessor/FileProcessor.Service

# 3. Crear archivo de servicio systemd
sudo tee /etc/systemd/system/fileprocessor.service > /dev/null <<EOF
[Unit]
Description=File Processor Service
After=network.target

[Service]
Type=notify
ExecStart=/opt/fileprocessor/FileProcessor.Service
WorkingDirectory=/opt/fileprocessor
User=fileprocessor
Group=fileprocessor
Restart=always
RestartSec=10
Environment=DOTNET_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
EOF

# 4. Habilitar e iniciar el servicio
sudo systemctl daemon-reload
sudo systemctl enable fileprocessor
sudo systemctl start fileprocessor
```

#### Configuración de Seguridad
```bash
# Configurar SELinux (si está habilitado)
sudo setsebool -P allow_execstack on
sudo restorecon -R /opt/fileprocessor

# Configurar firewall (si es necesario)
sudo firewall-cmd --permanent --add-port=8080/tcp
sudo firewall-cmd --reload

# Configurar límites de recursos
sudo tee /etc/systemd/system/fileprocessor.service.d/limits.conf > /dev/null <<EOF
[Service]
LimitNOFILE=65536
LimitNPROC=4096
MemoryMax=1G
CPUQuota=200%
EOF
```

### Opción 2: Docker Container

#### Dockerfile
```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine

# Crear usuario no-root
RUN addgroup -g 1001 fileprocessor && \
    adduser -D -s /bin/false -G fileprocessor -u 1001 fileprocessor

# Crear directorios de trabajo
WORKDIR /app
RUN mkdir -p input output logs && \
    chown -R fileprocessor:fileprocessor /app

# Copiar aplicación
COPY --chown=fileprocessor:fileprocessor ./publish .

# Cambiar a usuario no-root
USER fileprocessor

# Configurar variables de entorno
ENV DOTNET_ENVIRONMENT=Production
ENV FileProcessor__InputDirectory=/app/input
ENV FileProcessor__OutputDirectory=/app/output

# Exponer volúmenes
VOLUME ["/app/input", "/app/output", "/app/logs"]

# Comando de inicio
ENTRYPOINT ["./FileProcessor.Service"]
```

#### Docker Compose
```yaml
version: '3.8'

services:
  fileprocessor:
    build: .
    container_name: fileprocessor
    restart: unless-stopped
    environment:
      - DOTNET_ENVIRONMENT=Production
      - FileProcessor__ProcessingIntervalSeconds=30
    volumes:
      - ./data/input:/app/input
      - ./data/output:/app/output
      - ./data/logs:/app/logs
    logging:
      driver: "json-file"
      options:
        max-size: "10m"
        max-file: "3"
```

### Monitoreo en Linux

#### systemd Logs
```bash
# Ver logs en tiempo real
sudo journalctl -u fileprocessor -f

# Ver logs de las últimas 24 horas
sudo journalctl -u fileprocessor --since "24 hours ago"

# Ver logs con filtros
sudo journalctl -u fileprocessor --grep "ERROR"
```

#### Monitoreo de Recursos
```bash
# Usar htop para monitoreo interactivo
htop -p $(pgrep FileProcessor.Service)

# Usar systemctl para estado del servicio
systemctl status fileprocessor

# Monitoreo de archivos de log
tail -f /opt/fileprocessor/logs/*.log
```

## Deployment en la Nube

### Azure

#### Azure Container Instances
```bash
# Crear grupo de recursos
az group create --name fileprocessor-rg --location eastus

# Crear container instance
az container create \
  --resource-group fileprocessor-rg \
  --name fileprocessor \
  --image your-registry/fileprocessor:latest \
  --restart-policy Always \
  --environment-variables DOTNET_ENVIRONMENT=Production \
  --azure-file-volume-account-name mystorageaccount \
  --azure-file-volume-account-key $STORAGE_KEY \
  --azure-file-volume-share-name fileprocessor-data \
  --azure-file-volume-mount-path /app/data
```

#### Azure App Service (Web Jobs)
```bash
# Crear App Service Plan
az appservice plan create --name fileprocessor-plan --resource-group fileprocessor-rg --sku B1 --is-linux

# Crear Web App
az webapp create --resource-group fileprocessor-rg --plan fileprocessor-plan --name fileprocessor-app --runtime "DOTNETCORE|8.0"

# Configurar como WebJob continuo
az webapp webjob continuous create --resource-group fileprocessor-rg --name fileprocessor-app --webjob-name fileprocessor --webjob-type continuous
```

### AWS

#### ECS Fargate
```json
{
  "family": "fileprocessor",
  "networkMode": "awsvpc",
  "requiresCompatibilities": ["FARGATE"],
  "cpu": "256",
  "memory": "512",
  "executionRoleArn": "arn:aws:iam::account:role/ecsTaskExecutionRole",
  "containerDefinitions": [
    {
      "name": "fileprocessor",
      "image": "your-account.dkr.ecr.region.amazonaws.com/fileprocessor:latest",
      "essential": true,
      "environment": [
        {"name": "DOTNET_ENVIRONMENT", "value": "Production"}
      ],
      "mountPoints": [
        {
          "sourceVolume": "fileprocessor-data",
          "containerPath": "/app/data"
        }
      ],
      "logConfiguration": {
        "logDriver": "awslogs",
        "options": {
          "awslogs-group": "/ecs/fileprocessor",
          "awslogs-region": "us-east-1",
          "awslogs-stream-prefix": "ecs"
        }
      }
    }
  ]
}
```

#### EC2 Instance
```bash
# Instalar .NET 8 Runtime
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-runtime-8.0

# Desplegar aplicación
sudo mkdir -p /opt/fileprocessor
sudo cp -r ./publish/* /opt/fileprocessor/
sudo ./install-linux-service.sh
```

## Configuración de Monitoreo y Alertas

### Prometheus + Grafana

#### Métricas Personalizadas
```csharp
// Agregar al proyecto
services.AddSingleton<IMetricsLogger, PrometheusMetricsLogger>();

// Exponer métricas
app.UseRouting();
app.UseHttpMetrics();
app.MapMetrics();
```

#### Configuración de Prometheus
```yaml
# prometheus.yml
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: 'fileprocessor'
    static_configs:
      - targets: ['localhost:8080']
```

### Application Insights (Azure)

```csharp
// Configuración en Program.cs
services.AddApplicationInsightsTelemetryWorkerService();

// En appsettings.json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-key-here"
  }
}
```

### CloudWatch (AWS)

```csharp
// Configuración de logging
services.AddLogging(builder =>
{
    builder.AddAWSProvider();
});
```

## Backup y Recuperación

### Estrategia de Backup
```bash
# Script de backup diario
#!/bin/bash
BACKUP_DIR="/backup/fileprocessor/$(date +%Y%m%d)"
mkdir -p $BACKUP_DIR

# Backup de configuración
cp /opt/fileprocessor/*.json $BACKUP_DIR/

# Backup de logs
cp -r /opt/fileprocessor/logs $BACKUP_DIR/

# Backup de datos procesados (últimos 7 días)
find /opt/fileprocessor/output -mtime -7 -type f -exec cp {} $BACKUP_DIR/ \;

# Comprimir backup
tar -czf $BACKUP_DIR.tar.gz $BACKUP_DIR
rm -rf $BACKUP_DIR
```

### Procedimiento de Recuperación
1. Detener el servicio
2. Restaurar archivos de configuración
3. Verificar permisos y propietarios
4. Reiniciar el servicio
5. Verificar logs de inicio

## Troubleshooting de Deployment

### Problemas Comunes

#### Permisos Insuficientes
```bash
# Linux
sudo chown -R fileprocessor:fileprocessor /opt/fileprocessor
sudo chmod +x /opt/fileprocessor/FileProcessor.Service

# Windows
icacls "C:\Services\FileProcessor" /grant "FileProcessorSvc:(OI)(CI)F" /T
```

#### Dependencias Faltantes
```bash
# Verificar dependencias en Linux
ldd /opt/fileprocessor/FileProcessor.Service

# Instalar dependencias faltantes
sudo apt-get install -y libc6-dev libgdiplus
```

#### Configuración de Red
```bash
# Verificar conectividad
netstat -tlnp | grep FileProcessor
ss -tlnp | grep FileProcessor

# Verificar firewall
sudo ufw status
sudo firewall-cmd --list-all
```

### Logs de Diagnóstico

#### Habilitar Logging Detallado
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information",
      "FileProcessor.Service": "Trace"
    }
  }
}
```

#### Análisis de Performance
```bash
# Linux - usar perf
sudo perf record -g ./FileProcessor.Service
sudo perf report

# Windows - usar PerfView
PerfView.exe collect -AcceptEula -NoGui FileProcessor.Service.exe
```

Esta guía proporciona un marco completo para desplegar el FileProcessor Service en diferentes entornos, desde desarrollo local hasta producción en la nube.