# Scripts de Automatización - Ejemplos .NET Core 8

Este directorio contiene scripts de automatización para facilitar el desarrollo, testing y deployment de los ejemplos .NET Core 8.

## Scripts Disponibles

### 🔧 setup-databases.sh
Configura las bases de datos Oracle necesarias para todos los proyectos.

```bash
# Ejecutar configuración interactiva
./scripts/setup-databases.sh

# El script creará:
# - Usuarios de base de datos para cada aplicación
# - Esquemas y tablespaces
# - Datos de ejemplo básicos
# - Ejecutará migraciones de Entity Framework
```

**Requisitos:**
- Oracle Database instalado y ejecutándose
- Oracle SQL*Plus disponible en PATH
- Credenciales de administrador Oracle

### 🧪 run-all-tests.sh
Ejecuta todos los tests de los proyectos de ejemplo.

```bash
# Ejecutar todos los tests
./scripts/run-all-tests.sh

# Opciones disponibles:
./scripts/run-all-tests.sh --clean          # Limpiar antes de ejecutar
./scripts/run-all-tests.sh --coverage       # Generar reporte de cobertura
./scripts/run-all-tests.sh --integration    # Incluir tests de integración
./scripts/run-all-tests.sh --help           # Mostrar ayuda
```

**Características:**
- Tests unitarios y de integración
- Reporte de cobertura de código
- Soporte para múltiples frameworks de testing
- Configuración automática de variables de entorno

### 🚀 start-all-apps.sh
Inicia todas las aplicaciones de ejemplo simultáneamente.

```bash
# Iniciar todas las aplicaciones
./scripts/start-all-apps.sh

# Modo no interactivo
./scripts/start-all-apps.sh --no-interactive

# Mostrar ayuda
./scripts/start-all-apps.sh --help
```

**Características:**
- Inicia todas las aplicaciones en puertos específicos
- Menú interactivo para control
- Verificación automática de health checks
- Apertura automática de Swagger UIs
- Monitoreo de estado en tiempo real

**Puertos utilizados:**
- TaskManager: 5001 (HTTP), 7001 (HTTPS)
- BlogApi: 5002 (HTTP), 7002 (HTTPS)
- ImageProcessor: 5003 (HTTP), 7003 (HTTPS)
- SystemMonitor: 5004 (HTTP), 7004 (HTTPS)
- HealthDashboard: 5005 (HTTP), 7005 (HTTPS)
- ProductCatalog: 5006 (HTTP), 7006 (HTTPS)
- ResilientClient: 5007 (HTTP), 7007 (HTTPS)
- Calculator: 5008 (HTTP), 7008 (HTTPS)
- DigitalLibrary: 5009 (HTTP), 7009 (HTTPS)

### ✅ verify-setup.sh
Verifica que todo esté configurado correctamente.

```bash
# Verificación completa
./scripts/verify-setup.sh

# Verificación rápida
./scripts/verify-setup.sh --quick

# Modo verbose
./scripts/verify-setup.sh --verbose

# Mostrar ayuda
./scripts/verify-setup.sh --help
```

**Verificaciones incluidas:**
- .NET 8 SDK instalado y configurado
- Oracle Database disponible y configurado
- Herramientas de desarrollo (Git, Docker, etc.)
- Estructura del proyecto completa
- Dependencias de proyectos
- Compilación de proyectos
- Configuración de Entity Framework
- Puertos disponibles
- Variables de entorno

### 🐳 docker-setup.sh
Configura y maneja el entorno Docker para desarrollo.

```bash
# Iniciar entorno Docker completo
./scripts/docker-setup.sh start

# Detener servicios
./scripts/docker-setup.sh stop

# Reiniciar servicios
./scripts/docker-setup.sh restart

# Ver estado
./scripts/docker-setup.sh status

# Ver logs
./scripts/docker-setup.sh logs [servicio]

# Limpiar recursos
./scripts/docker-setup.sh cleanup

# Descargar imágenes
./scripts/docker-setup.sh pull
```

**Servicios incluidos:**
- Oracle Database Express Edition
- SQL Server (para Hangfire)
- Redis (caching)
- Elasticsearch + Kibana (logging)
- Prometheus + Grafana (métricas)
- Jaeger (tracing)
- Mailhog (email testing)
- Adminer (database admin)

## Configuración Docker

### docker-compose.yml
Configuración principal de servicios para desarrollo local.

**Servicios principales:**
- **oracle-db**: Oracle Database XE en puerto 1521
- **sqlserver-hangfire**: SQL Server para Hangfire en puerto 1433
- **redis**: Redis para caching en puerto 6379
- **elasticsearch**: Elasticsearch para logs en puerto 9200
- **kibana**: Kibana UI en puerto 5601
- **prometheus**: Prometheus para métricas en puerto 9090
- **grafana**: Grafana dashboards en puerto 3000
- **jaeger**: Jaeger tracing en puerto 16686

### docker-compose.override.yml
Configuraciones específicas para desarrollo local.

**Características adicionales:**
- Configuración de desarrollo más permisiva
- Servicios adicionales (Mailhog, Adminer, Nginx)
- Volúmenes locales para persistencia
- Variables de entorno de desarrollo

### .env.example
Plantilla de variables de entorno.

```bash
# Copiar y configurar
cp .env.example .env
# Editar .env con valores apropiados
```

## Flujo de Trabajo Recomendado

### 1. Configuración Inicial
```bash
# 1. Verificar sistema
./scripts/verify-setup.sh

# 2. Configurar Docker (opcional)
./scripts/docker-setup.sh start

# 3. Configurar bases de datos
./scripts/setup-databases.sh
```

### 2. Desarrollo Diario
```bash
# Iniciar todas las aplicaciones
./scripts/start-all-apps.sh

# En otra terminal, ejecutar tests
./scripts/run-all-tests.sh --coverage
```

### 3. Verificación Completa
```bash
# Verificar que todo funcione
./scripts/verify-setup.sh --verbose

# Ejecutar tests completos
./scripts/run-all-tests.sh --integration --coverage
```

## Solución de Problemas

### Error: "Permission denied"
```bash
# Hacer scripts ejecutables
chmod +x scripts/*.sh
```

### Error: "Oracle not found"
```bash
# Verificar instalación Oracle
sqlplus -V

# O usar Docker
./scripts/docker-setup.sh start
```

### Error: "Port already in use"
```bash
# Verificar puertos en uso
netstat -tuln | grep :5001

# Detener aplicaciones
pkill -f "dotnet run"
```

### Error: ".NET SDK not found"
```bash
# Verificar instalación .NET
dotnet --version

# Instalar .NET 8 SDK si es necesario
```

## Configuración de CI/CD

Los scripts están diseñados para ser utilizados en pipelines de CI/CD:

```yaml
# Ejemplo para GitHub Actions
- name: Setup Database
  run: ./scripts/setup-databases.sh

- name: Run Tests
  run: ./scripts/run-all-tests.sh --coverage

- name: Verify Setup
  run: ./scripts/verify-setup.sh
```

## Personalización

### Agregar Nuevos Proyectos
1. Actualizar arrays en `start-all-apps.sh`
2. Agregar configuración en `run-all-tests.sh`
3. Incluir verificaciones en `verify-setup.sh`

### Configurar Puertos Personalizados
1. Modificar arrays de puertos en scripts
2. Actualizar `docker-compose.yml` si es necesario
3. Actualizar documentación

### Agregar Servicios Docker
1. Modificar `docker-compose.yml`
2. Actualizar `docker-setup.sh`
3. Configurar health checks apropiados

## Recursos Adicionales

- [Documentación de Docker Compose](https://docs.docker.com/compose/)
- [Oracle Database en Docker](https://container-registry.oracle.com/ords/f?p=113:4:0::::P4_REPOSITORY,AI_REPOSITORY,AI_REPOSITORY_NAME,P4_REPOSITORY_NAME,P4_EULA_ID,P4_BUSINESS_AREA_ID:9,9,Oracle%20Database%20Express%20Edition,Oracle%20Database%20Express%20Edition,1,0&cs=3UwkTFVQd_s6FwNiJorQ_UMiM5ePgmqMhKIjPOhw7Qbxg_oJwYnRahNZKrQgdL-SdwTBz7-lh2s)
- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Prometheus Configuration](https://prometheus.io/docs/prometheus/latest/configuration/configuration/)
- [Grafana Provisioning](https://grafana.com/docs/grafana/latest/administration/provisioning/)

Para más información, consultar:
- `docs/getting-started.md`
- `docs/troubleshooting.md`
- `README.md` del proyecto principal