# Guía de Solución de Problemas - Ejemplos .NET Core 8

## Problemas Comunes de Configuración

### 1. Error de Conexión a Oracle Database

#### Síntoma
```
Oracle.ManagedDataAccess.Client.OracleException: ORA-12541: TNS:no listener
```

#### Soluciones
```bash
# 1. Verificar que Oracle esté ejecutándose
# Windows
net start OracleServiceXE
net start OracleXETNSListener

# Linux/macOS (Docker)
docker ps | grep oracle
docker start oracle-xe

# 2. Verificar conectividad
tnsping localhost:1521

# 3. Verificar listener
lsnrctl status

# 4. Probar conexión directa
sqlplus hr/password@localhost:1521/XE
```

#### Connection String Correctos
```csharp
// Para Oracle XE local
"Data Source=localhost:1521/XE;User Id=hr;Password=password;"

// Para Oracle con SID
"Data Source=localhost:1521;User Id=hr;Password=password;SID=ORCL;"

// Para Oracle Cloud (con wallet)
"Data Source=mydb_high;User Id=admin;Password=password;Wallet_Location=C:\\wallet;"
```

### 2. Error de Migraciones Entity Framework

#### Síntoma
```
System.InvalidOperationException: No database provider has been configured for this DbContext
```

#### Soluciones
```bash
# 1. Verificar que el provider esté instalado
dotnet add package Oracle.EntityFrameworkCore

# 2. Verificar configuración en Program.cs
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(connectionString));

# 3. Eliminar y recrear migraciones
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update

# 4. Verificar herramientas EF
dotnet tool install --global dotnet-ef
dotnet tool update --global dotnet-ef
```

### 3. Problemas con Hangfire

#### Síntoma
```
Hangfire.SqlServer.SqlServerDistributedLockException: Could not place a lock on the resource
```

#### Soluciones
```csharp
// 1. Configurar Hangfire con Oracle correctamente
services.AddHangfire(config =>
{
    config.UseOracle(connectionString, new OracleStorageOptions
    {
        SchemaName = "HANGFIRE"
    });
});

// 2. Crear esquema manualmente si es necesario
CREATE USER hangfire IDENTIFIED BY "HangfirePass123";
GRANT CONNECT, RESOURCE TO hangfire;
GRANT UNLIMITED TABLESPACE TO hangfire;

// 3. Verificar permisos de base de datos
GRANT CREATE TABLE, CREATE SEQUENCE, CREATE PROCEDURE TO hangfire;
```

## Problemas de Desarrollo

### 1. Error de Compilación - Dependencias

#### Síntoma
```
error CS0246: The type or namespace name 'AutoMapper' could not be found
```

#### Soluciones
```bash
# 1. Restaurar paquetes NuGet
dotnet restore

# 2. Limpiar y reconstruir
dotnet clean
dotnet build

# 3. Verificar PackageReference en .csproj
<PackageReference Include="AutoMapper" Version="12.0.1" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />

# 4. Actualizar paquetes
dotnet list package --outdated
dotnet add package AutoMapper --version 12.0.1
```

### 2. Problemas con Dependency Injection

#### Síntoma
```
System.InvalidOperationException: Unable to resolve service for type 'ITaskService'
```

#### Soluciones
```csharp
// 1. Verificar registro en Program.cs
builder.Services.AddScoped<ITaskService, TaskService>();

// 2. Verificar lifetime correcto
// Singleton: Una instancia para toda la aplicación
builder.Services.AddSingleton<IConfiguration>();

// Scoped: Una instancia por request HTTP
builder.Services.AddScoped<ITaskService, TaskService>();

// Transient: Nueva instancia cada vez
builder.Services.AddTransient<IEmailService, EmailService>();

// 3. Verificar dependencias circulares
// Evitar: A depende de B, B depende de A

// 4. Usar factory pattern para casos complejos
builder.Services.AddScoped<ITaskService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var logger = provider.GetRequiredService<ILogger<TaskService>>();
    return new TaskService(config, logger);
});
```

### 3. Problemas con AutoMapper

#### Síntoma
```
AutoMapper.AutoMapperMappingException: Missing type map configuration or unsupported mapping
```

#### Soluciones
```csharp
// 1. Verificar registro de profiles
builder.Services.AddAutoMapper(typeof(StudentProfile));

// 2. Crear mapping explícito
public class StudentProfile : Profile
{
    public StudentProfile()
    {
        CreateMap<Student, StudentDto>();
        CreateMap<CreateStudentDto, Student>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}

// 3. Verificar configuración compleja
CreateMap<Student, StudentDto>()
    .ForMember(dest => dest.FullName, 
        opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
    .ForMember(dest => dest.CourseCount, 
        opt => opt.MapFrom(src => src.Enrollments.Count));

// 4. Validar configuración en startup
var config = new MapperConfiguration(cfg => cfg.AddProfile<StudentProfile>());
config.AssertConfigurationIsValid();
```

## Problemas de Testing

### 1. Tests Fallan por Base de Datos

#### Síntoma
```
System.InvalidOperationException: The database operation expected to affect 1 row(s) but actually affected 0 row(s)
```

#### Soluciones
```csharp
// 1. Usar InMemory database para tests unitarios
services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("TestDb"));

// 2. Limpiar datos entre tests
public void Dispose()
{
    _context.Database.EnsureDeleted();
    _context.Dispose();
}

// 3. Usar transacciones para tests de integración
[Fact]
public async Task Test_WithTransaction()
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    
    // Test code here
    
    await transaction.RollbackAsync();
}

// 4. Para tests con Oracle real (Testcontainers)
public class OracleTestFixture : IAsyncLifetime
{
    private OracleContainer _container;
    
    public async Task InitializeAsync()
    {
        _container = new OracleBuilder()
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .Build();
            
        await _container.StartAsync();
    }
}
```

### 2. Mocking Problemas

#### Síntoma
```
Moq.MockException: Invalid setup on a non-virtual member
```

#### Soluciones
```csharp
// 1. Asegurar que métodos sean virtuales o usar interfaces
public interface ITaskRepository
{
    Task<TaskItem> GetByIdAsync(int id);
}

// 2. Setup correcto de mocks
var mockRepository = new Mock<ITaskRepository>();
mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
          .ReturnsAsync(new TaskItem { Id = 1, Title = "Test" });

// 3. Verificar llamadas
mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);

// 4. Mock de DbContext
var mockSet = new Mock<DbSet<TaskItem>>();
var mockContext = new Mock<AppDbContext>();
mockContext.Setup(c => c.Tasks).Returns(mockSet.Object);
```

## Problemas de Performance

### 1. Consultas EF Core Lentas

#### Síntoma
Aplicación lenta, muchas consultas N+1 en logs

#### Soluciones
```csharp
// 1. Usar Include para cargar datos relacionados
var students = await _context.Students
    .Include(s => s.Enrollments)
    .ThenInclude(e => e.Course)
    .ToListAsync();

// 2. Usar Select para proyecciones
var studentDtos = await _context.Students
    .Select(s => new StudentDto
    {
        Id = s.Id,
        Name = s.FirstName + " " + s.LastName,
        CourseCount = s.Enrollments.Count
    })
    .ToListAsync();

// 3. Usar Split Query para múltiples includes
var students = await _context.Students
    .AsSplitQuery()
    .Include(s => s.Enrollments)
    .Include(s => s.Courses)
    .ToListAsync();

// 4. Habilitar logging de consultas SQL
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseOracle(connectionString);
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.LogTo(Console.WriteLine, LogLevel.Information);
    }
});
```

### 2. Memory Leaks

#### Síntoma
Uso de memoria creciente, OutOfMemoryException

#### Soluciones
```csharp
// 1. Disponer recursos correctamente
public class StudentService : IDisposable
{
    private readonly HttpClient _httpClient;
    
    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

// 2. Usar using statements
using var scope = _serviceProvider.CreateScope();
var service = scope.ServiceProvider.GetRequiredService<IStudentService>();

// 3. Evitar capturar contextos grandes en closures
// Malo
students.ForEach(s => BackgroundJob.Enqueue(() => ProcessStudent(s, _context)));

// Bueno
students.ForEach(s => BackgroundJob.Enqueue(() => ProcessStudent(s.Id)));

// 4. Configurar límites de memoria
builder.Services.Configure<GCSettings>(options =>
{
    options.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
});
```

## Problemas de Deployment

### 1. Errores de Configuración en Producción

#### Síntoma
```
System.ArgumentNullException: Value cannot be null. Parameter name: connectionString
```

#### Soluciones
```csharp
// 1. Verificar variables de entorno
Environment.GetEnvironmentVariable("ORACLE_CONNECTION_STRING");

// 2. Usar configuración por ambiente
// appsettings.Production.json
{
  "ConnectionStrings": {
    "DefaultConnection": "#{ORACLE_CONNECTION_STRING}#"
  }
}

// 3. Validar configuración en startup
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string not configured");
}

// 4. Usar Azure Key Vault o similar para secretos
builder.Configuration.AddAzureKeyVault(keyVaultUrl, credential);
```

### 2. Problemas con Hangfire en Producción

#### Síntoma
Jobs no se ejecutan o fallan silenciosamente

#### Soluciones
```csharp
// 1. Configurar logging para Hangfire
services.AddHangfire(config =>
{
    config.UseOracle(connectionString);
    config.UseConsole();
    config.UseSerilogLogProvider();
});

// 2. Configurar retry policies
[AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
public async Task ProcessEmailJob(int userId)
{
    // Job implementation
}

// 3. Monitorear jobs fallidos
RecurringJob.AddOrUpdate<IMonitoringService>(
    "check-failed-jobs",
    service => service.CheckFailedJobsAsync(),
    Cron.Hourly);

// 4. Configurar dashboard con autenticación
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});
```

## Herramientas de Diagnóstico

### 1. Logging y Monitoreo

```csharp
// Configuración de Serilog para diagnóstico
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "MyApp")
    .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File("logs/app-.txt", 
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

// Logging estructurado en servicios
_logger.LogInformation("Processing student {StudentId} for course {CourseId}", 
    studentId, courseId);

_logger.LogError(ex, "Failed to process enrollment for student {StudentId}", studentId);
```

### 2. Health Checks para Diagnóstico

```csharp
// Configuración completa de health checks
services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database")
    .AddOracle(connectionString, name: "oracle")
    .AddHangfire(options => options.MinimumAvailableServers = 1)
    .AddCheck<ExternalApiHealthCheck>("external-api")
    .AddCheck("memory", () =>
    {
        var allocated = GC.GetTotalMemory(false);
        var threshold = 1024 * 1024 * 1024; // 1GB
        
        return allocated < threshold 
            ? HealthCheckResult.Healthy($"Memory usage: {allocated / 1024 / 1024} MB")
            : HealthCheckResult.Unhealthy($"Memory usage too high: {allocated / 1024 / 1024} MB");
    });

// Endpoint detallado de health checks
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

### 3. Scripts de Diagnóstico

```bash
#!/bin/bash
# diagnose.sh - Script de diagnóstico completo

echo "=== .NET Core Diagnostics ==="

# Verificar versión de .NET
echo "1. .NET Version:"
dotnet --version

# Verificar conexión a Oracle
echo "2. Oracle Connection:"
sqlplus -S hr/password@localhost:1521/XE <<EOF
SELECT 'Oracle connection OK' FROM dual;
EXIT;
EOF

# Verificar puertos
echo "3. Port Status:"
netstat -an | grep :5000
netstat -an | grep :1521

# Verificar procesos
echo "4. Application Processes:"
ps aux | grep dotnet

# Verificar logs recientes
echo "5. Recent Errors:"
tail -n 20 logs/app-$(date +%Y%m%d).txt | grep ERROR

# Verificar uso de memoria
echo "6. Memory Usage:"
free -h

# Verificar espacio en disco
echo "7. Disk Usage:"
df -h

echo "=== Diagnostics Complete ==="
```

### 4. Comandos Útiles para Troubleshooting

```bash
# Verificar configuración de .NET
dotnet --info

# Limpiar caché de NuGet
dotnet nuget locals all --clear

# Verificar dependencias
dotnet list package --include-transitive

# Ejecutar con logging detallado
dotnet run --verbosity diagnostic

# Verificar configuración de EF
dotnet ef dbcontext info

# Generar script SQL de migraciones
dotnet ef migrations script

# Verificar health checks
curl -i http://localhost:5000/health

# Monitorear performance
dotnet-counters monitor --process-id <pid>

# Analizar dumps de memoria
dotnet-dump collect --process-id <pid>
dotnet-dump analyze <dump-file>
```

## Contacto y Soporte

### Recursos Adicionales
- **Documentación oficial:** https://docs.microsoft.com/en-us/dotnet/
- **Oracle .NET Documentation:** https://docs.oracle.com/en/database/oracle/oracle-database/21/odpnt/
- **Stack Overflow:** Usar tags `.net-core`, `entity-framework-core`, `oracle`
- **GitHub Issues:** Reportar problemas específicos del proyecto

### Información para Reportar Problemas
Cuando reportes un problema, incluye:
1. Versión de .NET Core
2. Versión de Oracle Database
3. Sistema operativo
4. Mensaje de error completo
5. Stack trace si está disponible
6. Pasos para reproducir el problema
7. Configuración relevante (connection strings, etc.)

Esta guía cubre los problemas más comunes. Para problemas específicos no cubiertos aquí, consulta la documentación oficial o busca ayuda en la comunidad.