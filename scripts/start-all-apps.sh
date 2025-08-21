#!/bin/bash

# Script para iniciar todas las aplicaciones
# Ejemplos Autocontenidos .NET Core 8

set -e

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
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

log_app() {
    echo -e "${BLUE}[APP]${NC} $1"
}

log_url() {
    echo -e "${CYAN}[URL]${NC} $1"
}

# Variables globales
declare -a RUNNING_PIDS=()
declare -a APP_NAMES=()
declare -a APP_URLS=()

# Función para limpiar procesos al salir
cleanup() {
    echo
    log_info "Deteniendo aplicaciones..."
    
    for pid in "${RUNNING_PIDS[@]}"; do
        if kill -0 "$pid" 2>/dev/null; then
            log_info "Deteniendo proceso $pid"
            kill "$pid" 2>/dev/null || true
        fi
    done
    
    # Esperar un momento para que los procesos terminen
    sleep 2
    
    # Forzar terminación si es necesario
    for pid in "${RUNNING_PIDS[@]}"; do
        if kill -0 "$pid" 2>/dev/null; then
            log_warn "Forzando terminación del proceso $pid"
            kill -9 "$pid" 2>/dev/null || true
        fi
    done
    
    log_info "Todas las aplicaciones han sido detenidas"
}

# Configurar trap para limpiar al salir
trap cleanup EXIT INT TERM

# Función para verificar si un puerto está disponible
is_port_available() {
    local port=$1
    ! netstat -tuln 2>/dev/null | grep -q ":$port "
}

# Función para esperar que una aplicación esté lista
wait_for_app() {
    local url=$1
    local app_name=$2
    local max_attempts=30
    local attempt=1
    
    log_info "Esperando que $app_name esté lista en $url..."
    
    while [ $attempt -le $max_attempts ]; do
        if curl -s -f "$url/health" >/dev/null 2>&1 || curl -s -f "$url" >/dev/null 2>&1; then
            log_info "✓ $app_name está lista"
            return 0
        fi
        
        echo -n "."
        sleep 2
        ((attempt++))
    done
    
    echo
    log_warn "⚠ $app_name no respondió en el tiempo esperado"
    return 1
}

# Función para iniciar una aplicación
start_app() {
    local project_path=$1
    local app_name=$2
    local port=$3
    local https_port=$4
    
    log_app "Iniciando $app_name..."
    
    if [ ! -d "$project_path" ]; then
        log_warn "Directorio no encontrado: $project_path"
        return 1
    fi
    
    # Verificar puertos
    if ! is_port_available "$port"; then
        log_warn "Puerto $port ya está en uso para $app_name"
        return 1
    fi
    
    if [ -n "$https_port" ] && ! is_port_available "$https_port"; then
        log_warn "Puerto HTTPS $https_port ya está en uso para $app_name"
        return 1
    fi
    
    cd "$project_path"
    
    # Configurar variables de entorno
    export ASPNETCORE_ENVIRONMENT=Development
    export ASPNETCORE_URLS="http://localhost:$port"
    
    if [ -n "$https_port" ]; then
        export ASPNETCORE_URLS="http://localhost:$port;https://localhost:$https_port"
    fi
    
    # Restaurar dependencias si es necesario
    if [ ! -d "bin" ] || [ ! -d "obj" ]; then
        log_info "Restaurando dependencias para $app_name..."
        dotnet restore >/dev/null 2>&1
    fi
    
    # Iniciar aplicación en background
    dotnet run --no-build --configuration Development >/dev/null 2>&1 &
    local pid=$!
    
    # Verificar que el proceso se inició correctamente
    sleep 2
    if ! kill -0 "$pid" 2>/dev/null; then
        log_error "Error iniciando $app_name"
        cd - >/dev/null
        return 1
    fi
    
    # Guardar información del proceso
    RUNNING_PIDS+=("$pid")
    APP_NAMES+=("$app_name")
    
    local base_url="http://localhost:$port"
    APP_URLS+=("$base_url")
    
    log_info "✓ $app_name iniciado (PID: $pid)"
    log_url "  HTTP:  $base_url"
    
    if [ -n "$https_port" ]; then
        log_url "  HTTPS: https://localhost:$https_port"
    fi
    
    # Verificar endpoints comunes
    local swagger_url="$base_url/swagger"
    local health_url="$base_url/health"
    
    log_url "  Swagger: $swagger_url"
    log_url "  Health:  $health_url"
    
    cd - >/dev/null
    
    # Esperar que la aplicación esté lista
    wait_for_app "$base_url" "$app_name"
    
    return 0
}

# Función para mostrar el estado de las aplicaciones
show_status() {
    echo
    echo "=== ESTADO DE APLICACIONES ==="
    
    for i in "${!RUNNING_PIDS[@]}"; do
        local pid="${RUNNING_PIDS[$i]}"
        local name="${APP_NAMES[$i]}"
        local url="${APP_URLS[$i]}"
        
        if kill -0 "$pid" 2>/dev/null; then
            log_info "✓ $name (PID: $pid) - $url"
        else
            log_error "✗ $name (PID: $pid) - DETENIDO"
        fi
    done
    
    echo
}

# Función para mostrar menú interactivo
show_menu() {
    echo
    echo "=== MENÚ DE CONTROL ==="
    echo "1. Mostrar estado de aplicaciones"
    echo "2. Abrir Swagger UI de todas las apps"
    echo "3. Verificar health checks"
    echo "4. Mostrar logs en tiempo real"
    echo "5. Reiniciar aplicación específica"
    echo "6. Detener todas las aplicaciones"
    echo "h. Mostrar ayuda"
    echo "q. Salir"
    echo
}

# Función para abrir Swagger UIs
open_swagger_uis() {
    log_info "Abriendo Swagger UIs..."
    
    for url in "${APP_URLS[@]}"; do
        local swagger_url="$url/swagger"
        
        # Verificar si Swagger está disponible
        if curl -s -f "$swagger_url" >/dev/null 2>&1; then
            log_info "Abriendo: $swagger_url"
            
            # Intentar abrir en el navegador (multiplataforma)
            if command -v xdg-open >/dev/null 2>&1; then
                xdg-open "$swagger_url" >/dev/null 2>&1 &
            elif command -v open >/dev/null 2>&1; then
                open "$swagger_url" >/dev/null 2>&1 &
            elif command -v start >/dev/null 2>&1; then
                start "$swagger_url" >/dev/null 2>&1 &
            else
                log_info "Abrir manualmente: $swagger_url"
            fi
        else
            log_warn "Swagger no disponible en: $swagger_url"
        fi
    done
}

# Función para verificar health checks
check_health() {
    log_info "Verificando health checks..."
    
    for i in "${!APP_URLS[@]}"; do
        local url="${APP_URLS[$i]}"
        local name="${APP_NAMES[$i]}"
        local health_url="$url/health"
        
        if curl -s -f "$health_url" >/dev/null 2>&1; then
            log_info "✓ $name: Healthy"
        else
            log_warn "⚠ $name: No healthy o sin health check"
        fi
    done
}

# Función principal
main() {
    echo "=== Iniciador de Aplicaciones - Ejemplos .NET Core 8 ==="
    echo "Este script iniciará todas las aplicaciones de ejemplo"
    echo
    
    # Verificar .NET SDK
    if ! command -v dotnet >/dev/null 2>&1; then
        log_error ".NET SDK no encontrado. Por favor instalar .NET 8 SDK."
        exit 1
    fi
    
    log_info ".NET SDK version: $(dotnet --version)"
    
    # Opciones de línea de comandos
    START_ALL=true
    INTERACTIVE=true
    
    while [[ $# -gt 0 ]]; do
        case $1 in
            --no-interactive)
                INTERACTIVE=false
                shift
                ;;
            --help)
                echo "Uso: $0 [opciones]"
                echo "Opciones:"
                echo "  --no-interactive  Ejecutar sin menú interactivo"
                echo "  --help           Mostrar esta ayuda"
                exit 0
                ;;
            *)
                log_warn "Opción desconocida: $1"
                shift
                ;;
        esac
    done
    
    log_info "Iniciando aplicaciones..."
    
    # Lista de aplicaciones con sus puertos
    declare -a APPLICATIONS=(
        "01-TaskManager-ConfigDI/TaskManager.Api:TaskManager:5001:7001"
        "02-BlogApi-EntityFramework/BlogApi:BlogApi:5002:7002"
        "03-ImageProcessor-Hangfire/ImageProcessor.Api:ImageProcessor:5003:7003"
        "04-SystemMonitor-Logging/SystemMonitor.Api:SystemMonitor:5004:7004"
        "05-HealthDashboard-Diagnostics/HealthDashboard.Api:HealthDashboard:5005:7005"
        "06-ProductCatalog-Validation/ProductCatalog.Api:ProductCatalog:5006:7006"
        "07-ResilientClient-Polly/ResilientClient.Api:ResilientClient:5007:7007"
        "08-Calculator-Testing/Calculator.Api:Calculator:5008:7008"
        "09-DigitalLibrary-Documentation/DigitalLibrary.Api:DigitalLibrary:5009:7009"
    )
    
    # Iniciar cada aplicación
    for app_info in "${APPLICATIONS[@]}"; do
        IFS=':' read -r project_path app_name port https_port <<< "$app_info"
        start_app "$project_path" "$app_name" "$port" "$https_port"
        sleep 1  # Pequeña pausa entre aplicaciones
    done
    
    echo
    log_info "¡Todas las aplicaciones han sido iniciadas!"
    
    # Mostrar resumen
    show_status
    
    echo
    echo "=== ENLACES RÁPIDOS ==="
    for i in "${!APP_NAMES[@]}"; do
        local name="${APP_NAMES[$i]}"
        local url="${APP_URLS[$i]}"
        echo "  $name: $url/swagger"
    done
    
    # Modo interactivo
    if [ "$INTERACTIVE" = true ]; then
        echo
        log_info "Modo interactivo activado. Usa 'q' para salir."
        
        while true; do
            show_menu
            read -p "Selecciona una opción: " choice
            
            case $choice in
                1)
                    show_status
                    ;;
                2)
                    open_swagger_uis
                    ;;
                3)
                    check_health
                    ;;
                4)
                    log_info "Mostrando logs... (Ctrl+C para volver al menú)"
                    # Mostrar logs de todas las aplicaciones
                    for pid in "${RUNNING_PIDS[@]}"; do
                        if kill -0 "$pid" 2>/dev/null; then
                            echo "--- Logs del proceso $pid ---"
                        fi
                    done
                    ;;
                5)
                    echo "Funcionalidad de reinicio no implementada aún"
                    ;;
                6|q)
                    log_info "Deteniendo todas las aplicaciones..."
                    break
                    ;;
                h)
                    echo "Ayuda:"
                    echo "  1 - Ver estado de todas las aplicaciones"
                    echo "  2 - Abrir Swagger UI en el navegador"
                    echo "  3 - Verificar health checks"
                    echo "  4 - Ver logs en tiempo real"
                    echo "  5 - Reiniciar aplicación específica"
                    echo "  6 - Detener todas las aplicaciones"
                    echo "  q - Salir del script"
                    ;;
                *)
                    log_warn "Opción no válida: $choice"
                    ;;
            esac
        done
    else
        # Modo no interactivo - mantener aplicaciones corriendo
        log_info "Modo no interactivo. Presiona Ctrl+C para detener todas las aplicaciones."
        
        # Esperar indefinidamente
        while true; do
            sleep 10
            
            # Verificar que las aplicaciones sigan corriendo
            for i in "${!RUNNING_PIDS[@]}"; do
                local pid="${RUNNING_PIDS[$i]}"
                local name="${APP_NAMES[$i]}"
                
                if ! kill -0 "$pid" 2>/dev/null; then
                    log_warn "$name (PID: $pid) se ha detenido inesperadamente"
                fi
            done
        done
    fi
}

# Ejecutar función principal con todos los argumentos
main "$@"