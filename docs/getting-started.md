# Guía de Inicio - Ejemplos Autocontenidos .NET Core 8

## Introducción

Este proyecto contiene 10 mini-aplicaciones independientes diseñadas para enseñar conceptos específicos de .NET Core 8 de manera práctica. Cada ejemplo es completamente funcional pero simple, permitiendo a desarrolladores junior entender conceptos específicos sin la complejidad de una aplicación empresarial completa.

## Requisitos del Sistema

### Software Requerido

- **.NET 8 SDK** (versión 8.0 o superior)
  - Descargar desde: https://dotnet.microsoft.com/download/dotnet/8.0
- **Oracle Database** (19c o superior)
  - Oracle Database Express Edition (XE) es suficiente para desarrollo
  - Alternativamente, usar Oracle Cloud Always Free Tier
- **Visual Studio 2022** o **Visual Studio Code**
  - Extensiones recomendadas para VS Code:
    - C# Dev Kit
    - Oracle Developer Tools for VS Code
- **Git** para control de versiones

### Configuración de Base de Datos Oracle

#### Opción 1: Oracle Database Express Edition (XE)
```bash
# Descargar Oracle XE desde Oracle.com
# Instalar siguiendo las instrucciones del instalador
# Configurar puerto: 1521
# SID por defecto: XE
```

#### Opción 2: Oracle en Docker
```bash
# Ejecutar Oracle en contenedor Docker
docker run -d --name oracle-xe \
  -p 1521:1521 \
  -p 5500:5500 \
  -e ORACLE_PWD=YourPassword123 \
  -e ORACLE_CHARACTERSET=AL32UTF8 \
  container-registry.oracle.com/database/express:latest
```

#### Opción 3: Oracle Cloud Always Free
1. Crear cuenta en Oracle Cloud (https://cloud.oracle.com)
2. Crear Autonomous Database (Always Free)
3. Descargar wallet de conexión
4. Configurar connection string en aplicaciones

### Variables de Entorno

Crear archivo `.env` en la raíz del proyecto:
```bash
# Oracle Database Connection
ORACLE_CONNECTION_STRING="Data Source=localhost:1521/XE;User Id=hr;Password=YourPassword123;"

# Application Insights (opcional)
APPLICATIONINSIGHTS_CONNECTION_STRING="your-app-insights-connection-string"

# External APIs (para ejemplos de resiliencia)
WEATHER_API_KEY="your-openweathermap-api-key"
NEWS_API_KEY="your-newsapi-key"
```

## Instalación y Configuración

### 1. Clonar el Repositorio
```bash
git clone <repository-url>
cd dotnet-core-8-examples
```

### 2. Verificar Instalación de .NET
```bash
dotnet --version
# Debe mostrar versión 8.0.x o superior
```

### 3. Restaurar Dependencias
```bash
# Restaurar todas las dependencias del proyecto
dotnet restore
```

### 4. Configurar Base de Datos

#### Crear Usuario y Esquema
```sql
-- Conectar como SYSTEM o SYS
CREATE USER training_user IDENTIFIED BY "TrainingPass123";
GRANT CONNECT, RESOURCE, DBA TO training_user;
GRANT UNLIMITED TABLESPACE TO training_user;

-- Crear esquemas para cada mini-aplicación
CREATE USER blog_user IDENTIFIED BY "BlogPass123";
CREATE USER calculator_user IDENTIFIED BY "CalcPass123";
CREATE USER health_user IDENTIFIED BY "HealthPass123";

GRANT CONNECT, RESOURCE TO blog_user;
GRANT CONNECT, RESOURCE TO calculator_user;
GRANT CONNECT, RESOURCE TO health_user;
```

### 5. Configurar Connection Strings

Actualizar `appsettings.json` en cada proyecto:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=localhost:1521/XE;User Id=training_user;Password=TrainingPass123;"
  }
}
```

## Estructura del Proyecto

```
DotNetCore8Examples/
├── 01-TaskManager-ConfigDI/           # Configuración y DI básico
├── 02-BlogApi-EntityFramework/        # EF Core con Oracle
├── 03-ImageProcessor-Hangfire/        # Background jobs
├── 04-SystemMonitor-Logging/          # Serilog y métricas
├── 05-HealthDashboard-Diagnostics/    # Health checks
├── 06-ProductCatalog-Validation/      # AutoMapper y FluentValidation
├── 07-ResilientClient-Polly/          # Patrones de resiliencia
├── 08-Calculator-Testing/             # Testing completo
├── 09-DigitalLibrary-Documentation/   # Swagger avanzado
├── 10-FileProcessor-HostedService/    # Background services
└── docs/                              # Documentación general
```

## Ejecutar las Mini-Aplicaciones

### Método 1: Ejecutar Individualmente
```bash
# Navegar al directorio de la aplicación
cd 01-TaskManager-ConfigDI/TaskManager.Api

# Ejecutar la aplicación
dotnet run

# La aplicación estará disponible en:
# https://localhost:7001 (HTTPS)
# http://localhost:5001 (HTTP)
```

### Método 2: Usar Scripts de Automatización
```bash
# Ejecutar todas las migraciones
./scripts/setup-databases.sh

# Ejecutar todos los tests
./scripts/run-all-tests.sh

# Iniciar todas las aplicaciones
./scripts/start-all-apps.sh
```

## Orden de Aprendizaje Recomendado

### Nivel Principiante
1. **TaskManager** - Fundamentos de configuración y DI
2. **BlogApi** - Entity Framework y acceso a datos
3. **ProductCatalog** - Validación y mapeo automático

### Nivel Intermedio
4. **SystemMonitor** - Logging estructurado y métricas
5. **HealthDashboard** - Health checks y diagnósticos
6. **ImageProcessor** - Background jobs con Hangfire

### Nivel Avanzado
7. **ResilientClient** - Patrones de resiliencia
8. **Calculator** - Testing completo
9. **DigitalLibrary** - Documentación avanzada
10. **FileProcessor** - Hosted services

## Verificación de Instalación

### Script de Verificación
```bash
# Crear y ejecutar script de verificación
./scripts/verify-setup.sh
```

### Verificación Manual
```bash
# 1. Verificar .NET SDK
dotnet --version

# 2. Verificar conexión a Oracle
sqlplus training_user/TrainingPass123@localhost:1521/XE

# 3. Compilar proyecto de ejemplo
cd 01-TaskManager-ConfigDI/TaskManager.Api
dotnet build

# 4. Ejecutar tests básicos
dotnet test
```

## Recursos Adicionales

### Documentación Oficial
- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Oracle .NET Documentation](https://docs.oracle.com/en/database/oracle/oracle-database/21/odpnt/)

### Herramientas Útiles
- **Oracle SQL Developer** - IDE para Oracle Database
- **Postman** - Testing de APIs
- **Docker Desktop** - Contenedores para desarrollo
- **Azure Data Studio** - Editor de base de datos multiplataforma

### Comunidad y Soporte
- [.NET Community](https://dotnet.microsoft.com/platform/community)
- [Stack Overflow - .NET Tag](https://stackoverflow.com/questions/tagged/.net)
- [Oracle Developer Community](https://community.oracle.com/tech/developers/)

## Próximos Pasos

1. **Configurar el entorno** siguiendo esta guía
2. **Ejecutar el script de verificación** para confirmar la instalación
3. **Comenzar con TaskManager** (01-TaskManager-ConfigDI)
4. **Leer la documentación específica** de cada mini-aplicación
5. **Completar los ejercicios prácticos** incluidos en cada proyecto

## Solución de Problemas Rápidos

### Error de Conexión a Oracle
```bash
# Verificar que Oracle esté ejecutándose
lsnrctl status

# Verificar conectividad
tnsping localhost:1521
```

### Error de Compilación .NET
```bash
# Limpiar y restaurar
dotnet clean
dotnet restore
dotnet build
```

### Problemas con Migraciones EF
```bash
# Eliminar migraciones existentes
dotnet ef migrations remove

# Crear nueva migración
dotnet ef migrations add InitialCreate

# Aplicar migración
dotnet ef database update
```

Para problemas más específicos, consultar [docs/troubleshooting.md](./troubleshooting.md).