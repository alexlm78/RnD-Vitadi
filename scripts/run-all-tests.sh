#!/bin/bash

# Script para ejecutar todos los tests de los proyectos
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

log_test() {
    echo -e "${BLUE}[TEST]${NC} $1"
}

# Variables globales
TOTAL_PROJECTS=0
PASSED_PROJECTS=0
FAILED_PROJECTS=0
TEST_RESULTS=()

# Función para ejecutar tests de un proyecto
run_project_tests() {
    local project_path=$1
    local project_name=$2
    
    log_test "Ejecutando tests para: $project_name"
    
    if [ ! -d "$project_path" ]; then
        log_warn "Directorio no encontrado: $project_path"
        return 1
    fi
    
    cd "$project_path"
    
    # Verificar si hay archivos de test
    if ! find . -name "*.Tests.csproj" -o -name "*Tests.csproj" -o -name "*.Test.csproj" | grep -q .; then
        log_warn "No se encontraron proyectos de test en $project_name"
        cd - >/dev/null
        return 0
    fi
    
    # Restaurar dependencias
    log_info "Restaurando dependencias para $project_name..."
    if ! dotnet restore >/dev/null 2>&1; then
        log_error "Error restaurando dependencias para $project_name"
        cd - >/dev/null
        return 1
    fi
    
    # Ejecutar tests
    log_info "Ejecutando tests..."
    
    # Configurar variables de entorno para tests
    export ASPNETCORE_ENVIRONMENT=Testing
    export ConnectionStrings__DefaultConnection="Data Source=localhost:1521/XE;User Id=test_user;Password=TestPass123;"
    
    if dotnet test --configuration Release --logger "console;verbosity=minimal" --collect:"XPlat Code Coverage"; then
        log_info "✓ Tests pasaron para $project_name"
        TEST_RESULTS+=("✓ $project_name: PASSED")
        ((PASSED_PROJECTS++))
    else
        log_error "✗ Tests fallaron para $project_name"
        TEST_RESULTS+=("✗ $project_name: FAILED")
        ((FAILED_PROJECTS++))
    fi
    
    cd - >/dev/null
    ((TOTAL_PROJECTS++))
}

# Función para ejecutar tests de integración específicos
run_integration_tests() {
    log_test "Ejecutando tests de integración..."
    
    # Calculator - Tests más completos
    if [ -d "08-Calculator-Testing" ]; then
        log_info "Ejecutando tests de Calculator (incluye integración con Oracle)..."
        cd "08-Calculator-Testing"
        
        # Tests unitarios
        if [ -d "Calculator.Tests" ]; then
            cd "Calculator.Tests"
            dotnet test --filter "Category!=Integration" --logger "console;verbosity=normal"
            cd ..
        fi
        
        # Tests de integración (si Oracle está disponible)
        if echo "SELECT 1 FROM dual;" | sqlplus -S test_user/TestPass123@localhost:1521/XE >/dev/null 2>&1; then
            log_info "Oracle disponible, ejecutando tests de integración..."
            if [ -d "Calculator.Tests" ]; then
                cd "Calculator.Tests"
                dotnet test --filter "Category=Integration" --logger "console;verbosity=normal"
                cd ..
            fi
        else
            log_warn "Oracle no disponible, saltando tests de integración"
        fi
        
        cd - >/dev/null
    fi
}

# Función para generar reporte de cobertura
generate_coverage_report() {
    log_info "Generando reporte de cobertura..."
    
    # Buscar archivos de cobertura
    coverage_files=$(find . -name "coverage.cobertura.xml" -type f 2>/dev/null)
    
    if [ -n "$coverage_files" ]; then
        log_info "Archivos de cobertura encontrados:"
        echo "$coverage_files"
        
        # Si reportgenerator está instalado, generar reporte HTML
        if command -v reportgenerator >/dev/null 2>&1; then
            log_info "Generando reporte HTML de cobertura..."
            reportgenerator \
                "-reports:**/coverage.cobertura.xml" \
                "-targetdir:TestResults/CoverageReport" \
                "-reporttypes:Html;Badges"
            
            log_info "Reporte de cobertura generado en: TestResults/CoverageReport/index.html"
        else
            log_warn "ReportGenerator no instalado. Para instalar: dotnet tool install -g dotnet-reportgenerator-globaltool"
        fi
    else
        log_warn "No se encontraron archivos de cobertura"
    fi
}

# Función para limpiar artefactos de test
cleanup_test_artifacts() {
    log_info "Limpiando artefactos de test..."
    
    # Limpiar directorios bin y obj
    find . -type d -name "bin" -exec rm -rf {} + 2>/dev/null || true
    find . -type d -name "obj" -exec rm -rf {} + 2>/dev/null || true
    
    # Limpiar archivos de test temporales
    find . -name "*.trx" -delete 2>/dev/null || true
    find . -name "TestResults" -type d -exec rm -rf {} + 2>/dev/null || true
}

# Función para verificar herramientas necesarias
check_prerequisites() {
    log_info "Verificando herramientas necesarias..."
    
    # Verificar .NET SDK
    if ! command -v dotnet >/dev/null 2>&1; then
        log_error ".NET SDK no encontrado. Por favor instalar .NET 8 SDK."
        exit 1
    fi
    
    local dotnet_version=$(dotnet --version)
    log_info ".NET SDK version: $dotnet_version"
    
    # Verificar si es .NET 8
    if [[ ! $dotnet_version =~ ^8\. ]]; then
        log_warn "Se recomienda .NET 8. Versión actual: $dotnet_version"
    fi
    
    # Verificar Oracle (opcional)
    if command -v sqlplus >/dev/null 2>&1; then
        log_info "Oracle SQL*Plus encontrado"
    else
        log_warn "Oracle SQL*Plus no encontrado. Tests de integración con Oracle serán saltados."
    fi
}

# Función principal
main() {
    echo "=== Ejecutor de Tests - Ejemplos .NET Core 8 ==="
    echo "Este script ejecutará todos los tests de los proyectos"
    echo
    
    # Verificar herramientas
    check_prerequisites
    
    # Opciones de ejecución
    CLEAN_BEFORE=false
    GENERATE_COVERAGE=false
    RUN_INTEGRATION=false
    
    while [[ $# -gt 0 ]]; do
        case $1 in
            --clean)
                CLEAN_BEFORE=true
                shift
                ;;
            --coverage)
                GENERATE_COVERAGE=true
                shift
                ;;
            --integration)
                RUN_INTEGRATION=true
                shift
                ;;
            --help)
                echo "Uso: $0 [opciones]"
                echo "Opciones:"
                echo "  --clean       Limpiar antes de ejecutar tests"
                echo "  --coverage    Generar reporte de cobertura"
                echo "  --integration Ejecutar tests de integración"
                echo "  --help        Mostrar esta ayuda"
                exit 0
                ;;
            *)
                log_warn "Opción desconocida: $1"
                shift
                ;;
        esac
    done
    
    # Limpiar si se solicita
    if [ "$CLEAN_BEFORE" = true ]; then
        cleanup_test_artifacts
        log_info "Limpieza completada"
    fi
    
    log_info "Iniciando ejecución de tests..."
    
    # Lista de proyectos con tests
    declare -a TEST_PROJECTS=(
        "01-TaskManager-ConfigDI:TaskManager"
        "02-BlogApi-EntityFramework:BlogApi"
        "03-ImageProcessor-Hangfire:ImageProcessor"
        "04-SystemMonitor-Logging:SystemMonitor"
        "05-HealthDashboard-Diagnostics:HealthDashboard"
        "06-ProductCatalog-Validation:ProductCatalog"
        "07-ResilientClient-Polly:ResilientClient"
        "08-Calculator-Testing:Calculator"
        "09-DigitalLibrary-Documentation:DigitalLibrary"
        "10-FileProcessor-HostedService:FileProcessor"
    )
    
    # Ejecutar tests para cada proyecto
    for project_info in "${TEST_PROJECTS[@]}"; do
        IFS=':' read -r project_path project_name <<< "$project_info"
        run_project_tests "$project_path" "$project_name"
        echo
    done
    
    # Ejecutar tests de integración si se solicita
    if [ "$RUN_INTEGRATION" = true ]; then
        run_integration_tests
    fi
    
    # Generar reporte de cobertura si se solicita
    if [ "$GENERATE_COVERAGE" = true ]; then
        generate_coverage_report
    fi
    
    # Mostrar resumen
    echo
    echo "=== RESUMEN DE TESTS ==="
    echo "Total de proyectos: $TOTAL_PROJECTS"
    echo "Proyectos exitosos: $PASSED_PROJECTS"
    echo "Proyectos fallidos: $FAILED_PROJECTS"
    echo
    
    echo "Resultados detallados:"
    for result in "${TEST_RESULTS[@]}"; do
        echo "  $result"
    done
    
    echo
    if [ $FAILED_PROJECTS -eq 0 ]; then
        log_info "¡Todos los tests pasaron exitosamente! 🎉"
        exit 0
    else
        log_error "Algunos tests fallaron. Revisar logs para más detalles."
        exit 1
    fi
}

# Ejecutar función principal con todos los argumentos
main "$@"