#!/bin/bash

# Script para configurar y ejecutar el entorno Docker
# Ejemplos Autocontenidos .NET Core 8

set -e

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Función para mostrar mensajes
log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

log_docker() {
    echo -e "${BLUE}[DOCKER]${NC} $1"
}

# Función para verificar Docker
check_docker() {
    log_info "Verificando Docker..."
    
    if ! command -v docker >/dev/null 2>&1; then
        log_error "Docker no está instalado. Por favor instalar Docker Desktop."
        exit 1
    fi
    
    if ! docker info >/dev/null 2>&1; then
        log_error "Docker daemon no está ejecutándose. Por favor iniciar Docker Desktop."
        exit 1
    fi
    
    log_info "Docker está disponible: $(docker --version)"
    
    # Verificar Docker Compose
    if ! command -v docker-compose >/dev/null 2>&1 && ! docker compose version >/dev/null 2>&1; then
        log_error "Docker Compose no está disponible."
        exit 1
    fi
    
    log_info "Docker Compose está disponible"
}

# Función para crear directorios necesarios
create_directories() {
    log_info "Creando directorios necesarios..."
    
    # Directorios para volúmenes persistentes
    mkdir -p data/{oracle,sqlserver,redis,elasticsearch,prometheus,grafana}
    mkdir -p monitoring/grafana/{dashboards,dev-dashboards,dev-datasources}
    mkdir -p nginx/ssl
    mkdir -p scripts/{oracle-init,sqlserver-init}
    
    # Configurar permisos
    chmod 755 data/
    chmod -R 755 data/*/
    
    log_info "Directorios creados exitosamente"
}

# Función para configurar archivo .env
setup_environment() {
    log_info "Configurando variables de entorno..."
    
    if [ ! -f ".env" ]; then
        if [ -f ".env.example" ]; then
            cp .env.example .env
            log_info "Archivo .env creado desde .env.example"
            log_warn "Por favor revisar y actualizar las variables en .env antes de continuar"
        else
            log_error "Archivo .env.example no encontrado"
            exit 1
        fi
    else
        log_info "Archivo .env ya existe"
    fi
}

# Función para descargar imágenes Docker
pull_images() {
    log_docker "Descargando imágenes Docker..."
    
    # Lista de imágenes necesarias
    local images=(
        "container-registry.oracle.com/database/express:latest"
        "mcr.microsoft.com/mssql/server:2022-latest"
        "redis:7-alpine"
        "docker.elastic.co/elasticsearch/elasticsearch:8.11.0"
        "docker.elastic.co/kibana/kibana:8.11.0"
        "prom/prometheus:latest"
        "grafana/grafana:latest"
        "jaegertracing/all-in-one:latest"
        "mailhog/mailhog:latest"
        "adminer:latest"
        "nginx:alpine"
    )
    
    for image in "${images[@]}"; do
        log_docker "Descargando $image..."
        if docker pull "$image"; then
            log_info "✓ $image descargado"
        else
            log_warn "⚠ Error descargando $image (continuando...)"
        fi
    done
}

# Función para iniciar servicios
start_services() {
    log_docker "Iniciando servicios Docker..."
    
    # Opciones para Docker Compose
    local compose_cmd="docker-compose"
    if docker compose version >/dev/null 2>&1; then
        compose_cmd="docker compose"
    fi
    
    # Iniciar servicios básicos primero
    log_docker "Iniciando servicios de base de datos..."
    $compose_cmd up -d oracle-db sqlserver-hangfire redis
    
    # Esperar que las bases de datos estén listas
    log_info "Esperando que Oracle Database esté listo..."
    sleep 60
    
    # Verificar Oracle
    local oracle_ready=false
    local attempts=0
    while [ $attempts -lt 10 ] && [ "$oracle_ready" = false ]; do
        if docker exec dotnet-examples-oracle sqlplus -S system/OraclePass123@localhost:1521/XE <<< "SELECT 1 FROM dual;" >/dev/null 2>&1; then
            oracle_ready=true
            log_info "✓ Oracle Database está listo"
        else
            log_info "Esperando Oracle Database... (intento $((attempts + 1))/10)"
            sleep 30
            ((attempts++))
        fi
    done
    
    if [ "$oracle_ready" = false ]; then
        log_warn "Oracle Database no respondió en el tiempo esperado"
    fi
    
    # Iniciar servicios de monitoreo
    log_docker "Iniciando servicios de monitoreo..."
    $compose_cmd up -d elasticsearch prometheus grafana jaeger
    
    # Iniciar servicios adicionales
    log_docker "Iniciando servicios adicionales..."
    $compose_cmd up -d mailhog adminer
    
    log_info "Todos los servicios han sido iniciados"
}

# Función para mostrar estado de servicios
show_status() {
    log_info "Estado de los servicios:"
    
    local compose_cmd="docker-compose"
    if docker compose version >/dev/null 2>&1; then
        compose_cmd="docker compose"
    fi
    
    $compose_cmd ps
    
    echo
    log_info "URLs de acceso:"
    echo "  Oracle EM Express: http://localhost:5500/em"
    echo "  Adminer (DB Admin): http://localhost:8080"
    echo "  Grafana: http://localhost:3000 (admin/admin)"
    echo "  Prometheus: http://localhost:9090"
    echo "  Jaeger UI: http://localhost:16686"
    echo "  Kibana: http://localhost:5601"
    echo "  Mailhog: http://localhost:8025"
    echo
}

# Función para detener servicios
stop_services() {
    log_docker "Deteniendo servicios Docker..."
    
    local compose_cmd="docker-compose"
    if docker compose version >/dev/null 2>&1; then
        compose_cmd="docker compose"
    fi
    
    $compose_cmd down
    
    log_info "Servicios detenidos"
}

# Función para limpiar todo
cleanup() {
    log_docker "Limpiando recursos Docker..."
    
    local compose_cmd="docker-compose"
    if docker compose version >/dev/null 2>&1; then
        compose_cmd="docker compose"
    fi
    
    # Detener y eliminar contenedores
    $compose_cmd down -v --remove-orphans
    
    # Eliminar volúmenes (opcional)
    read -p "¿Eliminar volúmenes de datos? (y/N): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        docker volume prune -f
        rm -rf data/
        log_info "Volúmenes eliminados"
    fi
    
    # Eliminar imágenes (opcional)
    read -p "¿Eliminar imágenes Docker? (y/N): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        docker image prune -a -f
        log_info "Imágenes eliminadas"
    fi
    
    log_info "Limpieza completada"
}

# Función para mostrar logs
show_logs() {
    local service=$1
    
    local compose_cmd="docker-compose"
    if docker compose version >/dev/null 2>&1; then
        compose_cmd="docker compose"
    fi
    
    if [ -n "$service" ]; then
        log_info "Mostrando logs de $service..."
        $compose_cmd logs -f "$service"
    else
        log_info "Mostrando logs de todos los servicios..."
        $compose_cmd logs -f
    fi
}

# Función principal
main() {
    echo "=== Configurador Docker - Ejemplos .NET Core 8 ==="
    echo "Este script configurará el entorno Docker para desarrollo"
    echo
    
    case "${1:-start}" in
        "start")
            check_docker
            create_directories
            setup_environment
            pull_images
            start_services
            show_status
            ;;
        "stop")
            stop_services
            ;;
        "restart")
            stop_services
            sleep 5
            start_services
            show_status
            ;;
        "status")
            show_status
            ;;
        "logs")
            show_logs "$2"
            ;;
        "cleanup")
            cleanup
            ;;
        "pull")
            check_docker
            pull_images
            ;;
        "help"|"--help")
            echo "Uso: $0 [comando] [opciones]"
            echo
            echo "Comandos:"
            echo "  start     Iniciar todos los servicios (por defecto)"
            echo "  stop      Detener todos los servicios"
            echo "  restart   Reiniciar todos los servicios"
            echo "  status    Mostrar estado de servicios"
            echo "  logs      Mostrar logs (logs [servicio])"
            echo "  pull      Descargar imágenes Docker"
            echo "  cleanup   Limpiar recursos Docker"
            echo "  help      Mostrar esta ayuda"
            echo
            echo "Ejemplos:"
            echo "  $0 start"
            echo "  $0 logs oracle-db"
            echo "  $0 cleanup"
            ;;
        *)
            log_error "Comando desconocido: $1"
            echo "Usar '$0 help' para ver comandos disponibles"
            exit 1
            ;;
    esac
}

# Ejecutar función principal con todos los argumentos
main "$@"