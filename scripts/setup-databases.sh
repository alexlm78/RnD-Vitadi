#!/bin/bash

# Script para configurar bases de datos Oracle para todos los proyectos
# Ejemplos Autocontenidos .NET Core 8

set -e

echo "=== Configuración de Bases de Datos Oracle ==="
echo "Este script configurará las bases de datos necesarias para todos los proyectos"

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
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

# Verificar si Oracle está disponible
check_oracle() {
    log_info "Verificando conexión a Oracle..."
    
    if command -v sqlplus >/dev/null 2>&1; then
        log_info "Oracle SQL*Plus encontrado"
    else
        log_error "Oracle SQL*Plus no encontrado. Por favor instalar Oracle Client."
        exit 1
    fi
    
    # Verificar conectividad básica
    if echo "SELECT 'Oracle OK' FROM dual;" | sqlplus -S system/oracle@localhost:1521/XE >/dev/null 2>&1; then
        log_info "Conexión a Oracle exitosa"
    else
        log_warn "No se pudo conectar con credenciales por defecto. Continuando..."
    fi
}

# Crear usuarios y esquemas
create_users() {
    log_info "Creando usuarios y esquemas..."
    
    # Leer credenciales de administrador
    read -p "Usuario administrador Oracle (default: system): " ADMIN_USER
    ADMIN_USER=${ADMIN_USER:-system}
    
    read -s -p "Password administrador Oracle: " ADMIN_PASS
    echo
    
    read -p "Host Oracle (default: localhost): " ORACLE_HOST
    ORACLE_HOST=${ORACLE_HOST:-localhost}
    
    read -p "Puerto Oracle (default: 1521): " ORACLE_PORT
    ORACLE_PORT=${ORACLE_PORT:-1521}
    
    read -p "SID/Service Oracle (default: XE): " ORACLE_SID
    ORACLE_SID=${ORACLE_SID:-XE}
    
    CONNECTION_STRING="${ADMIN_USER}/${ADMIN_PASS}@${ORACLE_HOST}:${ORACLE_PORT}/${ORACLE_SID}"
    
    # Script SQL para crear usuarios
    cat > /tmp/create_users.sql << 'EOF'
-- Crear usuario principal para training
CREATE USER training_user IDENTIFIED BY "TrainingPass123";
GRANT CONNECT, RESOURCE, DBA TO training_user;
GRANT UNLIMITED TABLESPACE TO training_user;

-- Crear usuarios específicos por aplicación
CREATE USER blog_user IDENTIFIED BY "BlogPass123";
CREATE USER task_user IDENTIFIED BY "TaskPass123";
CREATE USER calc_user IDENTIFIED BY "CalcPass123";
CREATE USER health_user IDENTIFIED BY "HealthPass123";
CREATE USER product_user IDENTIFIED BY "ProductPass123";
CREATE USER library_user IDENTIFIED BY "LibraryPass123";
CREATE USER hangfire_user IDENTIFIED BY "HangfirePass123";

-- Otorgar permisos básicos
GRANT CONNECT, RESOURCE TO blog_user;
GRANT CONNECT, RESOURCE TO task_user;
GRANT CONNECT, RESOURCE TO calc_user;
GRANT CONNECT, RESOURCE TO health_user;
GRANT CONNECT, RESOURCE TO product_user;
GRANT CONNECT, RESOURCE TO library_user;
GRANT CONNECT, RESOURCE TO hangfire_user;

-- Permisos adicionales para Hangfire
GRANT CREATE TABLE, CREATE SEQUENCE, CREATE PROCEDURE TO hangfire_user;
GRANT UNLIMITED TABLESPACE TO hangfire_user;

-- Mostrar usuarios creados
SELECT username FROM dba_users WHERE username LIKE '%_USER';

EXIT;
EOF

    log_info "Ejecutando script de creación de usuarios..."
    if sqlplus -S "$CONNECTION_STRING" @/tmp/create_users.sql; then
        log_info "Usuarios creados exitosamente"
    else
        log_error "Error creando usuarios"
        exit 1
    fi
    
    # Limpiar archivo temporal
    rm -f /tmp/create_users.sql
}

# Ejecutar migraciones de Entity Framework
run_migrations() {
    log_info "Ejecutando migraciones de Entity Framework..."
    
    # Array de proyectos con Entity Framework
    declare -a EF_PROJECTS=(
        "02-BlogApi-EntityFramework/BlogApi"
        "05-HealthDashboard-Diagnostics/HealthDashboard.Api"
        "08-Calculator-Testing/Calculator.Api"
        "09-DigitalLibrary-Documentation/DigitalLibrary.Api"
    )
    
    for project in "${EF_PROJECTS[@]}"; do
        if [ -d "$project" ]; then
            log_info "Ejecutando migraciones para $project"
            cd "$project"
            
            # Verificar si hay migraciones
            if dotnet ef migrations list >/dev/null 2>&1; then
                dotnet ef database update
                if [ $? -eq 0 ]; then
                    log_info "Migraciones aplicadas para $project"
                else
                    log_warn "Error aplicando migraciones para $project"
                fi
            else
                log_warn "No se encontraron migraciones para $project"
            fi
            
            cd - >/dev/null
        else
            log_warn "Proyecto no encontrado: $project"
        fi
    done
}

# Crear datos de prueba
seed_data() {
    log_info "Creando datos de prueba..."
    
    # Script SQL para datos de ejemplo
    cat > /tmp/seed_data.sql << 'EOF'
-- Conectar como training_user
CONNECT training_user/TrainingPass123@localhost:1521/XE;

-- Crear tabla de ejemplo si no existe
CREATE TABLE IF NOT EXISTS sample_data (
    id NUMBER GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    name VARCHAR2(100) NOT NULL,
    description VARCHAR2(500),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Insertar datos de ejemplo
INSERT INTO sample_data (name, description) VALUES ('Ejemplo 1', 'Datos de prueba para verificar conexión');
INSERT INTO sample_data (name, description) VALUES ('Ejemplo 2', 'Más datos de prueba');

COMMIT;

-- Verificar datos
SELECT COUNT(*) as total_records FROM sample_data;

EXIT;
EOF

    if sqlplus -S /nolog @/tmp/seed_data.sql; then
        log_info "Datos de prueba creados"
    else
        log_warn "Error creando datos de prueba (puede ser normal si ya existen)"
    fi
    
    rm -f /tmp/seed_data.sql
}

# Verificar configuración
verify_setup() {
    log_info "Verificando configuración..."
    
    # Verificar conexiones de usuarios
    declare -a USERS=("training_user" "blog_user" "task_user" "calc_user")
    declare -a PASSWORDS=("TrainingPass123" "BlogPass123" "TaskPass123" "CalcPass123")
    
    for i in "${!USERS[@]}"; do
        user="${USERS[$i]}"
        pass="${PASSWORDS[$i]}"
        
        if echo "SELECT 'OK' FROM dual;" | sqlplus -S "${user}/${pass}@localhost:1521/XE" >/dev/null 2>&1; then
            log_info "Usuario $user: ✓ Conexión exitosa"
        else
            log_warn "Usuario $user: ✗ Error de conexión"
        fi
    done
}

# Función principal
main() {
    echo "Iniciando configuración de bases de datos..."
    echo "Presiona Ctrl+C para cancelar en cualquier momento"
    echo
    
    read -p "¿Continuar con la configuración? (y/N): " -n 1 -r
    echo
    
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        log_info "Configuración cancelada"
        exit 0
    fi
    
    check_oracle
    create_users
    run_migrations
    seed_data
    verify_setup
    
    log_info "¡Configuración completada!"
    echo
    echo "Connection strings de ejemplo:"
    echo "Training: Data Source=localhost:1521/XE;User Id=training_user;Password=TrainingPass123;"
    echo "Blog: Data Source=localhost:1521/XE;User Id=blog_user;Password=BlogPass123;"
    echo "Calculator: Data Source=localhost:1521/XE;User Id=calc_user;Password=CalcPass123;"
    echo
    echo "Para usar estos connection strings, actualiza los archivos appsettings.json de cada proyecto."
}

# Ejecutar función principal
main "$@"