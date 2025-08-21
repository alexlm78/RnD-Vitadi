# Patrones Comunes - Ejemplos .NET Core 8

## Introducción

Este documento explica los patrones de diseño y arquitectura utilizados consistentemente a través de todas las mini-aplicaciones del proyecto. Entender estos patrones te ayudará a navegar el código y aplicar las mejores prácticas en tus propios proyectos.

## Patrones Arquitectónicos

### 1. Clean Architecture / Layered Architecture

Todas las aplicaciones siguen una arquitectura en capas que separa responsabilidades:

```
┌─────────────────────────────────────┐
│         Presentation Layer          │  ← Controllers, Middlewares, DTOs
├─────────────────────────────────────┤
│         Application Layer           │  ← Services, Validators, Mappers
├─────────────────────────────────────┤
│           Domain Layer              │  ← Entities, Interfaces, Domain Logic
├─────────────────────────────────────┤
│        Infrastructure Layer         │  ← Repositories, External Services
└─────────────────────────────────────┘
```

**Ejemplo en TaskManager:**
```csharp
// Presentation Layer
[ApiController]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    
    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }
}

// Application Layer
public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;
    
    public TaskService(ITaskRepository repository)
    {
        _repository = repository;
    }
}

// Domain Layer
public interface ITaskRepository
{
    Task<TaskItem> GetByIdAsync(int id);
}

// Infrastructure Layer
public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;
    
    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }
}
```

### 2. Dependency Injection Pattern

Todas las aplicaciones utilizan el contenedor DI integrado de .NET:

```csharp
// Program.cs - Registro de servicios
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(connectionString));

// Uso en controllers
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    
    // DI automático por constructor
    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }
}
```

**Lifetimes utilizados:**
- `AddSingleton`: Para servicios sin estado (loggers, configuración)
- `AddScoped`: Para servicios por request (repositories, services)
- `AddTransient`: Para servicios ligeros y stateless

## Patrones de Acceso a Datos

### 1. Repository Pattern

Abstrae el acceso a datos y facilita testing:

```csharp
// Interfaz genérica
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

// Implementación específica
public class StudentRepository : IRepository<Student>
{
    private readonly DbContext _context;
    
    public StudentRepository(DbContext context)
    {
        _context = context;
    }
    
    public async Task<Student> GetByIdAsync(int id)
    {
        return await _context.Students
            .Include(s => s.Enrollments)
            .FirstOrDefaultAsync(s => s.Id == id);
    }
}
```

### 2. Unit of Work Pattern

Coordina múltiples repositories en una transacción:

```csharp
public interface IUnitOfWork : IDisposable
{
    IStudentRepository Students { get; }
    ICourseRepository Courses { get; }
    IEnrollmentRepository Enrollments { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;
    
    public UnitOfWork(DbContext context)
    {
        _context = context;
        Students = new StudentRepository(_context);
        Courses = new CourseRepository(_context);
        Enrollments = new EnrollmentRepository(_context);
    }
    
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
```

## Patrones de Mapeo y Validación

### 1. AutoMapper Pattern

Mapeo automático entre entidades y DTOs:

```csharp
// Profile de mapeo
public class StudentProfile : Profile
{
    public StudentProfile()
    {
        CreateMap<Student, StudentDto>()
            .ForMember(dest => dest.FullName, 
                opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
        
        CreateMap<CreateStudentDto, Student>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
    }
}

// Uso en servicios
public class StudentService
{
    private readonly IMapper _mapper;
    
    public async Task<StudentDto> CreateAsync(CreateStudentDto dto)
    {
        var student = _mapper.Map<Student>(dto);
        await _repository.AddAsync(student);
        return _mapper.Map<StudentDto>(student);
    }
}
```

### 2. FluentValidation Pattern

Validación declarativa y reutilizable:

```csharp
public class CreateStudentValidator : AbstractValidator<CreateStudentDto>
{
    public CreateStudentValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email es requerido")
            .EmailAddress().WithMessage("Email debe tener formato válido")
            .MustAsync(BeUniqueEmail).WithMessage("Email ya existe");
        
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Nombre es requerido")
            .MaximumLength(50).WithMessage("Nombre no puede exceder 50 caracteres");
    }
    
    private async Task<bool> BeUniqueEmail(string email, CancellationToken token)
    {
        // Validación asíncrona personalizada
        return await _repository.IsEmailUniqueAsync(email);
    }
}
```

## Patrones de Manejo de Errores

### 1. Global Exception Handler

Manejo centralizado de excepciones:

```csharp
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    
    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = exception switch
        {
            NotFoundException => new { StatusCode = 404, Message = exception.Message },
            ValidationException => new { StatusCode = 400, Message = exception.Message },
            BusinessRuleException => new { StatusCode = 422, Message = exception.Message },
            _ => new { StatusCode = 500, Message = "Internal server error" }
        };
        
        context.Response.StatusCode = response.StatusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
```

### 2. Result Pattern

Manejo explícito de éxito/error sin excepciones:

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string Error { get; }
    
    private Result(bool isSuccess, T value, string error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }
    
    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}

// Uso en servicios
public async Task<Result<StudentDto>> GetStudentAsync(int id)
{
    var student = await _repository.GetByIdAsync(id);
    if (student == null)
        return Result<StudentDto>.Failure("Student not found");
    
    var dto = _mapper.Map<StudentDto>(student);
    return Result<StudentDto>.Success(dto);
}
```

## Patrones de Resiliencia

### 1. Retry Pattern con Polly

Reintentos automáticos para operaciones transitorias:

```csharp
public class ExternalApiService
{
    private readonly HttpClient _httpClient;
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;
    
    public ExternalApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning("Retry {RetryCount} after {Delay}ms", retryCount, timespan.TotalMilliseconds);
                });
    }
    
    public async Task<string> GetDataAsync(string endpoint)
    {
        var response = await _retryPolicy.ExecuteAsync(async () =>
        {
            return await _httpClient.GetAsync(endpoint);
        });
        
        return await response.Content.ReadAsStringAsync();
    }
}
```

### 2. Circuit Breaker Pattern

Protección contra servicios que fallan consistentemente:

```csharp
public class ResilientService
{
    private readonly IAsyncPolicy<string> _circuitBreakerPolicy;
    
    public ResilientService()
    {
        _circuitBreakerPolicy = Policy
            .Handle<HttpRequestException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, duration) =>
                {
                    _logger.LogWarning("Circuit breaker opened for {Duration}", duration);
                },
                onReset: () =>
                {
                    _logger.LogInformation("Circuit breaker reset");
                });
    }
}
```

## Patrones de Background Processing

### 1. Hosted Service Pattern

Servicios de larga duración:

```csharp
public class FileProcessorService : BackgroundService
{
    private readonly ILogger<FileProcessorService> _logger;
    private readonly IServiceProvider _serviceProvider;
    
    public FileProcessorService(ILogger<FileProcessorService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IFileProcessor>();
                
                await processor.ProcessFilesAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing files");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
```

### 2. Hangfire Job Pattern

Trabajos programados y en cola:

```csharp
public class EmailService : IEmailService
{
    public async Task SendWelcomeEmailAsync(int userId)
    {
        // Fire-and-forget job
        BackgroundJob.Enqueue(() => ProcessWelcomeEmail(userId));
    }
    
    public void ScheduleReminderEmail(int userId, DateTime scheduleTime)
    {
        // Delayed job
        BackgroundJob.Schedule(() => ProcessReminderEmail(userId), scheduleTime);
    }
    
    [AutomaticRetry(Attempts = 3)]
    public async Task ProcessWelcomeEmail(int userId)
    {
        // Implementación con retry automático
        var user = await _userRepository.GetByIdAsync(userId);
        await _emailProvider.SendAsync(user.Email, "Welcome", template);
    }
}

// Configuración de jobs recurrentes
public class JobConfiguration
{
    public static void ConfigureRecurringJobs()
    {
        RecurringJob.AddOrUpdate<IReportService>(
            "daily-report",
            service => service.GenerateDailyReportAsync(),
            Cron.Daily(2)); // 2 AM daily
    }
}
```

## Patrones de Testing

### 1. Test Fixture Pattern

Configuración reutilizable para tests:

```csharp
public class DatabaseFixture : IDisposable
{
    public DbContext Context { get; }
    
    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        Context = new AppDbContext(options);
        SeedTestData();
    }
    
    private void SeedTestData()
    {
        Context.Students.AddRange(
            new Student { FirstName = "John", LastName = "Doe", Email = "john@test.com" },
            new Student { FirstName = "Jane", LastName = "Smith", Email = "jane@test.com" }
        );
        Context.SaveChanges();
    }
    
    public void Dispose() => Context.Dispose();
}

// Uso en tests
public class StudentServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    
    public StudentServiceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public async Task GetStudent_ShouldReturnStudent_WhenExists()
    {
        // Arrange
        var service = new StudentService(_fixture.Context);
        
        // Act
        var result = await service.GetByIdAsync(1);
        
        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("John");
    }
}
```

### 2. Builder Pattern para Test Data

Creación flexible de datos de prueba:

```csharp
public class StudentBuilder
{
    private Student _student = new Student();
    
    public StudentBuilder WithName(string firstName, string lastName)
    {
        _student.FirstName = firstName;
        _student.LastName = lastName;
        return this;
    }
    
    public StudentBuilder WithEmail(string email)
    {
        _student.Email = email;
        return this;
    }
    
    public StudentBuilder WithEnrollment(Course course)
    {
        _student.Enrollments ??= new List<Enrollment>();
        _student.Enrollments.Add(new Enrollment { Course = course });
        return this;
    }
    
    public Student Build() => _student;
    
    public static StudentBuilder Create() => new StudentBuilder();
}

// Uso en tests
[Fact]
public void Test_StudentWithMultipleEnrollments()
{
    // Arrange
    var student = StudentBuilder.Create()
        .WithName("John", "Doe")
        .WithEmail("john@test.com")
        .WithEnrollment(new Course { Title = "Math" })
        .WithEnrollment(new Course { Title = "Science" })
        .Build();
    
    // Act & Assert
    student.Enrollments.Should().HaveCount(2);
}
```

## Patrones de Configuración

### 1. Options Pattern

Configuración tipada y validada:

```csharp
public class DatabaseOptions
{
    public const string SectionName = "Database";
    
    public string ConnectionString { get; set; }
    public int CommandTimeout { get; set; } = 30;
    public bool EnableSensitiveDataLogging { get; set; } = false;
}

public class EmailOptions
{
    public const string SectionName = "Email";
    
    [Required]
    public string SmtpServer { get; set; }
    
    [Range(1, 65535)]
    public int Port { get; set; } = 587;
    
    [Required]
    public string Username { get; set; }
    
    [Required]
    public string Password { get; set; }
}

// Configuración en Program.cs
builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection(DatabaseOptions.SectionName));

builder.Services.Configure<EmailOptions>(
    builder.Configuration.GetSection(EmailOptions.SectionName));

// Validación de opciones
builder.Services.AddOptions<EmailOptions>()
    .Bind(builder.Configuration.GetSection(EmailOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Uso en servicios
public class EmailService
{
    private readonly EmailOptions _options;
    
    public EmailService(IOptions<EmailOptions> options)
    {
        _options = options.Value;
    }
}
```

## Mejores Prácticas Aplicadas

### 1. Async/Await Consistency
- Todos los métodos I/O son asíncronos
- Uso de `ConfigureAwait(false)` en bibliotecas
- Evitar `async void` excepto en event handlers

### 2. Logging Estructurado
- Uso consistente de Serilog
- Contexto enriquecido con información relevante
- Niveles de log apropiados

### 3. Error Handling
- Excepciones específicas del dominio
- Manejo global de excepciones
- Logging de errores con contexto

### 4. Security
- Validación de entrada en todos los endpoints
- No exposición de información sensible en logs
- Uso de HTTPS en producción

Estos patrones proporcionan una base sólida para el desarrollo de aplicaciones .NET robustas y mantenibles.