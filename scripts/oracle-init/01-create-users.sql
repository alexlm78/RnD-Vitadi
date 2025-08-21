-- Script de inicialización para Oracle Database
-- Crea usuarios y esquemas necesarios para los ejemplos .NET Core 8

-- Conectar como SYSTEM
CONNECT system/&1@localhost:1521/XE;

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
CREATE USER test_user IDENTIFIED BY "TestPass123";

-- Otorgar permisos básicos
GRANT CONNECT, RESOURCE TO blog_user;
GRANT CONNECT, RESOURCE TO task_user;
GRANT CONNECT, RESOURCE TO calc_user;
GRANT CONNECT, RESOURCE TO health_user;
GRANT CONNECT, RESOURCE TO product_user;
GRANT CONNECT, RESOURCE TO library_user;
GRANT CONNECT, RESOURCE TO hangfire_user;
GRANT CONNECT, RESOURCE TO test_user;

-- Permisos adicionales para Hangfire
GRANT CREATE TABLE, CREATE SEQUENCE, CREATE PROCEDURE TO hangfire_user;
GRANT UNLIMITED TABLESPACE TO hangfire_user;

-- Permisos adicionales para testing
GRANT CREATE TABLE, CREATE SEQUENCE, CREATE PROCEDURE TO test_user;
GRANT UNLIMITED TABLESPACE TO test_user;

-- Crear tablespaces específicos (opcional)
CREATE TABLESPACE dotnet_examples_data
DATAFILE '/opt/oracle/oradata/XE/dotnet_examples_data.dbf'
SIZE 100M AUTOEXTEND ON NEXT 10M MAXSIZE 1G;

CREATE TABLESPACE dotnet_examples_index
DATAFILE '/opt/oracle/oradata/XE/dotnet_examples_index.dbf'
SIZE 50M AUTOEXTEND ON NEXT 5M MAXSIZE 500M;

-- Asignar tablespaces a usuarios
ALTER USER training_user DEFAULT TABLESPACE dotnet_examples_data;
ALTER USER blog_user DEFAULT TABLESPACE dotnet_examples_data;
ALTER USER calc_user DEFAULT TABLESPACE dotnet_examples_data;
ALTER USER health_user DEFAULT TABLESPACE dotnet_examples_data;
ALTER USER product_user DEFAULT TABLESPACE dotnet_examples_data;
ALTER USER library_user DEFAULT TABLESPACE dotnet_examples_data;
ALTER USER test_user DEFAULT TABLESPACE dotnet_examples_data;

-- Mostrar usuarios creados
SELECT username, default_tablespace, account_status 
FROM dba_users 
WHERE username LIKE '%_USER' OR username = 'TRAINING_USER'
ORDER BY username;

-- Crear datos de ejemplo básicos
CONNECT training_user/TrainingPass123@localhost:1521/XE;

-- Tabla de configuración del sistema
CREATE TABLE system_config (
    config_key VARCHAR2(100) PRIMARY KEY,
    config_value VARCHAR2(500),
    description VARCHAR2(1000),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Insertar configuración inicial
INSERT INTO system_config (config_key, config_value, description) VALUES 
('system.version', '1.0.0', 'Versión del sistema de ejemplos .NET Core 8');

INSERT INTO system_config (config_key, config_value, description) VALUES 
('database.initialized', 'true', 'Indica si la base de datos ha sido inicializada');

INSERT INTO system_config (config_key, config_value, description) VALUES 
('examples.count', '10', 'Número total de aplicaciones de ejemplo');

COMMIT;

-- Verificar instalación
SELECT 'Oracle Database configurado correctamente para .NET Core 8 Examples' as status FROM dual;

EXIT;