#!/bin/bash

# Script para verificar la instalación y configuración
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

log_check() {
    echo -e "${BLUE}[CHECK]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[✓]${NC} $1"
}

log_fail() {
    echo -e "${RED}[✗]${NC} $1"
}

# Variables globales para el reporte
TOTAL_CHECKS=0
PASSED_CHECKS=0
FAILED_CHECKS=0
WARNINGS=0
declare -a CHECK_RESULTS=()

# Función para registrar resultado de verificación
record_check() {
    local status=$1
    local message=$2
    
    ((TOTAL_CHECKS++))
    
    case $status in
        "PASS")
            log_success "$message"
            CHECK_RESULTS+=("✓ $message")
            ((PASSED_CHECKS++))
            ;;
        "FAIL")
            log_fail "$message"
            CHECK_RESULTS+=("✗ $message")
            ((FAILED_CHECKS++))
            ;;
        "WARN")
            log_warn "$message"
            CHECK_RESULTS+=("⚠ $message")
            ((WARNINGS++))
            ;;
    esac
}

# Verificar .NET SDK
check_dotnet_sdk() {
    log_check "Verificando .NET SDK..."
    
    if command -v dotnet >/dev/null 2>&1; then
        local version=$(dotnet --version)
        local major_version=$(echo "$version" | cut -d. -f1)
        
        if [ "$major_version" = "8" ]; then
            record_check "PASS" ".NET 8 SDK instalado (versión: $version)"
        else
            record_check "WARN" ".NET SDK instalado pero no es versión 8 (versión: $version)"
        fi
        
        # Verificar workloads instalados
        local workloads=$(dotnet workload list 2>/dev/null | grep -c "Installed workloads" || echo "0")
        if [ "$workloads" -gt 0 ]; then
            record_check "PASS" "Workloads de .NET disponibles"
        fi
        
    else
        record_check "FAIL" ".NET SDK no encontrado"
    fi
}

# Verificar Oracle Database
check_oracle_database() {
    log_check "Verificando Oracle Database..."
    
    # Verificar Oracle Client
    if command -v sqlplus >/dev/null 2>&1; then
        record_check "PASS" "Oracle SQL*Plus instalado"
        
        # Verificar conectividad
        if echo "SELECT 'Oracle OK' FROM dual;" | sqlplus -S system/oracle@localhost:1521/XE >/dev/null 2>&1; then
            record_check "PASS" "Conexión a Oracle exitosa (system/oracle)"
        elif echo "SELECT 'Oracle OK' FROM dual;" | sqlplus -S hr/hr@localhost:1521/XE >/dev/null 2>&1; then
            record_check "PASS" "Conexión a Oracle exitosa (hr/hr)"
        else
            record_check "WARN" "Oracle disponible pero no se pudo conectar con credenciales por defecto"
        fi
        
        # Verificar usuarios de training
        if echo "SELECT username FROM dba_users WHERE username LIKE '%_USER';" | sqlplus -S system/oracle@localhost:1521/XE >/dev/null 2>&1; then
            record_check "PASS" "Usuarios de training configurados"
        else
            record_check "WARN" "Usuarios de training no configurados (ejecutar setup-databases.sh)"
        fi
        
    else
        record_check "WARN" "Oracle SQL*Plus no encontrado (tests de integración no disponibles)"
    fi
    
    # Verificar Oracle en Docker
    if command -v docker >/dev/null 2>&1; then
        if docker ps | grep -q oracle; then
            record_check "PASS" "Oracle ejecutándose en Docker"
        else
            record_check "WARN" "Oracle no encontrado en Docker"
        fi
    fi
}

# Verificar herramientas de desarrollo
check_development_tools() {
    log_check "Verificando herramientas de desarrollo..."
    
    # Git
    if command -v git >/dev/null 2>&1; then
        local git_version=$(git --version)
        record_check "PASS" "Git instalado ($git_version)"
    else
        record_check "WARN" "Git no encontrado"
    fi
    
    # Docker
    if command -v docker >/dev/null 2>&1; then
        local docker_version=$(docker --version)
        record_check "PASS" "Docker instalado ($docker_version)"
        
        # Verificar que Docker esté corriendo
        if docker info >/dev/null 2>&1; then
            record_check "PASS" "Docker daemon ejecutándose"
        else
            record_check "WARN" "Docker instalado pero daemon no está ejecutándose"
        fi
    else
        record_check "WARN" "Docker no encontrado (desarrollo local limitado)"
    fi
    
    # curl (para health checks)
    if command -v curl >/dev/null 2>&1; then
        record_check "PASS" "curl disponible"
    else
        record_check "WARN" "curl no encontrado (verificaciones de health check limitadas)"
    fi
}

# Verificar estructura del proyecto
check_project_structure() {
    log_check "Verificando estructura del proyecto..."
    
    # Verificar directorios principales
    local expected_dirs=(
        "01-TaskManager-ConfigDI"
        "02-BlogApi-EntityFramework"
        "03-ImageProcessor-Hangfire"
        "04-SystemMonitor-Logging"
        "05-HealthDashboard-Diagnostics"
        "06-ProductCatalog-Validation"
        "07-ResilientClient-Polly"
        "08-Calculator-Testing"
        "09-DigitalLibrary-Documentation"
        "10-FileProcessor-HostedService"
        "docs"
        "scripts"
    )
    
    local missing_dirs=0
    for dir in "${expected_dirs[@]}"; do
        if [ -d "$dir" ]; then
            record_check "PASS" "Directorio encontrado: $dir"
        else
            record_check "FAIL" "Directorio faltante: $dir"
            ((missing_dirs++))
        fi
    done
    
    if [ $missing_dirs -eq 0 ]; then
        record_check "PASS" "Estructura del proyecto completa"
    else
        record_check "FAIL" "Estructura del proyecto incompleta ($missing_dirs directorios faltantes)"
    fi
}

# Verificar dependencias de los proyectos
check_project_dependencies() {
    log_check "Verificando dependencias de los proyectos..."
    
    local projects_checked=0
    local projects_ok=0
    
    # Buscar todos los archivos .csproj
    while IFS= read -r -d '' csproj_file; do
        local project_dir=$(dirname "$csproj_file")
        local project_name=$(basename "$project_dir")
        
        cd "$project_dir"
        
        # Intentar restaurar dependencias
        if dotnet restore >/dev/null 2>&1; then
            record_check "PASS" "Dependencias OK: $project_name"
            ((projects_ok++))
        else
            record_check "FAIL" "Error en dependencias: $project_name"
        fi
        
        cd - >/dev/null
        ((projects_checked++))
        
    done < <(find . -name "*.csproj" -not -path "./bin/*" -not -path "./obj/*" -print0)
    
    if [ $projects_checked -eq 0 ]; then
        record_check "WARN" "No se encontraron proyectos .NET"
    else
        record_check "PASS" "Verificados $projects_checked proyectos ($projects_ok exitosos)"
    fi
}

# Verificar compilación de proyectos
check_project_compilation() {
    log_check "Verificando compilación de proyectos..."
    
    local compile_errors=0
    
    # Lista de proyectos principales
    local main_projects=(
        "01-TaskManager-ConfigDI/TaskManager.Api"
        "02-BlogApi-EntityFramework/BlogApi"
        "03-ImageProcessor-Hangfire/ImageProcessor.Api"
        "04-SystemMonitor-Logging/SystemMonitor.Api"
        "05-HealthDashboard-Diagnostics/HealthDashboard.Api"
        "06-ProductCatalog-Validation/ProductCatalog.Api"
        "07-ResilientClient-Polly/ResilientClient.Api"
        "08-Calculator-Testing/Calculator.Api"
        "09-DigitalLibrary-Documentation/DigitalLibrary.Api"
    )
    
    for project in "${main_projects[@]}"; do
        if [ -d "$project" ]; then
            cd "$project"
            
            if dotnet build --configuration Release --no-restore >/dev/null 2>&1; then
                record_check "PASS" "Compilación exitosa: $(basename "$project")"
            else
                record_check "FAIL" "Error de compilación: $(basename "$project")"
                ((compile_errors++))
            fi
            
            cd - >/dev/null
        else
            record_check "WARN" "Proyecto no encontrado: $project"
        fi
    done
    
    if [ $compile_errors -eq 0 ]; then
        record_check "PASS" "Todos los proyectos compilan correctamente"
    else
        record_check "FAIL" "$compile_errors proyectos con errores de compilación"
    fi
}

# Verificar configuración de Entity Framework
check_entity_framework() {
    log_check "Verificando Entity Framework..."
    
    # Verificar herramientas EF
    if dotnet tool list -g | grep -q dotnet-ef; then
        local ef_version=$(dotnet tool list -g | grep dotnet-ef | awk '{print $2}')
        record_check "PASS" "Entity Framework Tools instalado (versión: $ef_version)"
    else
        record_check "WARN" "Entity Framework Tools no instalado globalmente"
        log_info "Para instalar: dotnet tool install --global dotnet-ef"
    fi
    
    # Verificar proyectos con EF
    local ef_projects=(
        "02-BlogApi-EntityFramework/BlogApi"
        "05-HealthDashboard-Diagnostics/HealthDashboard.Api"
        "08-Calculator-Testing/Calculator.Api"
        "09-DigitalLibrary-Documentation/DigitalLibrary.Api"
    )
    
    for project in "${ef_projects[@]}"; do
        if [ -d "$project" ]; then
            cd "$project"
            
            if dotnet ef migrations list >/dev/null 2>&1; then
                record_check "PASS" "Migraciones EF configuradas: $(basename "$project")"
            else
                record_check "WARN" "Migraciones EF no configuradas: $(basename "$project")"
            fi
            
            cd - >/dev/null
        fi
    done
}

# Verificar puertos disponibles
check_available_ports() {
    log_check "Verificando puertos disponibles..."
    
    local required_ports=(5001 5002 5003 5004 5005 5006 5007 5008 5009 7001 7002 7003 7004 7005 7006 7007 7008 7009)
    local ports_in_use=0
    
    for port in "${required_ports[@]}"; do
        if netstat -tuln 2>/dev/null | grep -q ":$port "; then
            record_check "WARN" "Puerto $port ya está en uso"
            ((ports_in_use++))
        fi
    done
    
    if [ $ports_in_use -eq 0 ]; then
        record_check "PASS" "Todos los puertos requeridos están disponibles"
    else
        record_check "WARN" "$ports_in_use puertos ya están en uso"
    fi
}

# Verificar variables de entorno
check_environment_variables() {
    log_check "Verificando variables de entorno..."
    
    # Verificar archivo .env si existe
    if [ -f ".env" ]; then
        record_check "PASS" "Archivo .env encontrado"
        
        # Verificar variables importantes
        if grep -q "ORACLE_CONNECTION_STRING" .env; then
            record_check "PASS" "ORACLE_CONNECTION_STRING configurado"
        else
            record_check "WARN" "ORACLE_CONNECTION_STRING no configurado en .env"
        fi
    else
        record_check "WARN" "Archivo .env no encontrado (usar .env.example como plantilla)"
    fi
    
    # Verificar ASPNETCORE_ENVIRONMENT
    if [ -n "$ASPNETCORE_ENVIRONMENT" ]; then
        record_check "PASS" "ASPNETCORE_ENVIRONMENT configurado: $ASPNETCORE_ENVIRONMENT"
    else
        record_check "WARN" "ASPNETCORE_ENVIRONMENT no configurado (se usará Production por defecto)"
    fi
}

# Generar reporte final
generate_report() {
    echo
    echo "=== REPORTE DE VERIFICACIÓN ==="
    echo "Fecha: $(date)"
    echo "Sistema: $(uname -s) $(uname -r)"
    echo
    
    echo "Resumen:"
    echo "  Total de verificaciones: $TOTAL_CHECKS"
    echo "  Exitosas: $PASSED_CHECKS"
    echo "  Fallidas: $FAILED_CHECKS"
    echo "  Advertencias: $WARNINGS"
    echo
    
    # Calcular porcentaje de éxito
    local success_rate=0
    if [ $TOTAL_CHECKS -gt 0 ]; then
        success_rate=$((PASSED_CHECKS * 100 / TOTAL_CHECKS))
    fi
    
    echo "Tasa de éxito: $success_rate%"
    echo
    
    echo "Resultados detallados:"
    for result in "${CHECK_RESULTS[@]}"; do
        echo "  $result"
    done
    
    echo
    
    # Recomendaciones basadas en los resultados
    if [ $FAILED_CHECKS -gt 0 ]; then
        log_error "Se encontraron $FAILED_CHECKS problemas críticos que deben resolverse"
        echo "Recomendaciones:"
        echo "  1. Revisar la instalación de .NET 8 SDK"
        echo "  2. Verificar la estructura del proyecto"
        echo "  3. Ejecutar 'dotnet restore' en cada proyecto"
        echo "  4. Consultar docs/troubleshooting.md para problemas específicos"
    elif [ $WARNINGS -gt 0 ]; then
        log_warn "Se encontraron $WARNINGS advertencias que podrían afectar la funcionalidad"
        echo "Recomendaciones:"
        echo "  1. Configurar Oracle Database para funcionalidad completa"
        echo "  2. Instalar herramientas opcionales (Docker, EF Tools)"
        echo "  3. Configurar variables de entorno en .env"
    else
        log_success "¡Configuración completamente verificada! El proyecto está listo para usar."
    fi
    
    echo
    echo "Para más información, consultar:"
    echo "  - docs/getting-started.md"
    echo "  - docs/troubleshooting.md"
    echo "  - README.md"
}

# Función principal
main() {
    echo "=== Verificador de Configuración - Ejemplos .NET Core 8 ==="
    echo "Este script verificará que todo esté configurado correctamente"
    echo
    
    # Opciones de línea de comandos
    VERBOSE=false
    QUICK_CHECK=false
    
    while [[ $# -gt 0 ]]; do
        case $1 in
            --verbose)
                VERBOSE=true
                shift
                ;;
            --quick)
                QUICK_CHECK=true
                shift
                ;;
            --help)
                echo "Uso: $0 [opciones]"
                echo "Opciones:"
                echo "  --verbose  Mostrar información detallada"
                echo "  --quick    Verificación rápida (solo elementos esenciales)"
                echo "  --help     Mostrar esta ayuda"
                exit 0
                ;;
            *)
                log_warn "Opción desconocida: $1"
                shift
                ;;
        esac
    done
    
    log_info "Iniciando verificación del sistema..."
    echo
    
    # Ejecutar verificaciones
    check_dotnet_sdk
    check_oracle_database
    check_development_tools
    check_project_structure
    
    if [ "$QUICK_CHECK" = false ]; then
        check_project_dependencies
        check_project_compilation
        check_entity_framework
        check_available_ports
        check_environment_variables
    fi
    
    # Generar reporte final
    generate_report
    
    # Código de salida basado en resultados
    if [ $FAILED_CHECKS -gt 0 ]; then
        exit 1
    elif [ $WARNINGS -gt 0 ]; then
        exit 2
    else
        exit 0
    fi
}

# Ejecutar función principal con todos los argumentos
main "$@"