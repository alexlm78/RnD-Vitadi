using TaskManager.Api.DTOs;
using TaskManager.Api.Models;

namespace TaskManager.Api.Services;

/// <summary>
/// Implementación del servicio de gestión de tareas
/// Utiliza almacenamiento en memoria para demostrar conceptos de DI
/// </summary>
public class TaskService : ITaskService
{
    private readonly List<TaskItem> _tasks;
    private readonly ILogger<TaskService> _logger;
    private readonly IConfigurationService _configurationService;
    private int _nextId = 1;

    public TaskService(ILogger<TaskService> logger, IConfigurationService configurationService)
    {
        _logger = logger;
        _configurationService = configurationService;
        _tasks = new List<TaskItem>();
        
        // Inicializar con datos de ejemplo si está configurado
        var taskManagerOptions = _configurationService.GetTaskManagerOptions();
        if (taskManagerOptions.SeedData)
        {
            SeedInitialData();
        }

        _logger.LogInformation("TaskService inicializado. SeedData: {SeedData}, MaxTasks: {MaxTasks}", 
            taskManagerOptions.SeedData, taskManagerOptions.MaxTasksPerUser);
    }

    public async Task<IEnumerable<TaskResponseDto>> GetAllTasksAsync()
    {
        _logger.LogInformation("Obteniendo todas las tareas. Total: {Count}", _tasks.Count);
        
        await Task.Delay(10); // Simular operación asíncrona
        
        return _tasks.Select(MapToResponseDto).OrderBy(t => t.CreatedAt);
    }

    public async Task<TaskResponseDto?> GetTaskByIdAsync(int id)
    {
        _logger.LogInformation("Buscando tarea con ID: {TaskId}", id);
        
        await Task.Delay(5); // Simular operación asíncrona
        
        var task = _tasks.FirstOrDefault(t => t.Id == id);
        
        if (task == null)
        {
            _logger.LogWarning("Tarea con ID {TaskId} no encontrada", id);
            return null;
        }

        return MapToResponseDto(task);
    }

    public async Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto createTaskDto)
    {
        _logger.LogInformation("Creando nueva tarea: {Title}", createTaskDto.Title);
        
        // Verificar límite de tareas si está configurado
        var taskManagerOptions = _configurationService.GetTaskManagerOptions();
        if (_tasks.Count >= taskManagerOptions.MaxTasksPerUser)
        {
            _logger.LogWarning("Se ha alcanzado el límite máximo de tareas: {MaxTasks}", taskManagerOptions.MaxTasksPerUser);
            throw new InvalidOperationException($"Se ha alcanzado el límite máximo de {taskManagerOptions.MaxTasksPerUser} tareas");
        }
        
        var task = new TaskItem
        {
            Id = _nextId++,
            Title = createTaskDto.Title,
            Description = createTaskDto.Description,
            Priority = createTaskDto.Priority,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _tasks.Add(task);
        
        await Task.Delay(10); // Simular operación asíncrona
        
        _logger.LogInformation("Tarea creada exitosamente con ID: {TaskId}. Total de tareas: {TotalTasks}", 
            task.Id, _tasks.Count);
        
        return MapToResponseDto(task);
    }

    public async Task<TaskResponseDto?> UpdateTaskAsync(int id, UpdateTaskDto updateTaskDto)
    {
        _logger.LogInformation("Actualizando tarea con ID: {TaskId}", id);
        
        var task = _tasks.FirstOrDefault(t => t.Id == id);
        
        if (task == null)
        {
            _logger.LogWarning("Tarea con ID {TaskId} no encontrada para actualizar", id);
            return null;
        }

        task.Title = updateTaskDto.Title;
        task.Description = updateTaskDto.Description;
        task.IsCompleted = updateTaskDto.IsCompleted;
        task.Priority = updateTaskDto.Priority;
        task.UpdatedAt = DateTime.UtcNow;

        await Task.Delay(10); // Simular operación asíncrona
        
        _logger.LogInformation("Tarea con ID {TaskId} actualizada exitosamente", id);
        
        return MapToResponseDto(task);
    }

    public async Task<bool> DeleteTaskAsync(int id)
    {
        _logger.LogInformation("Eliminando tarea con ID: {TaskId}", id);
        
        var task = _tasks.FirstOrDefault(t => t.Id == id);
        
        if (task == null)
        {
            _logger.LogWarning("Tarea con ID {TaskId} no encontrada para eliminar", id);
            return false;
        }

        _tasks.Remove(task);
        
        await Task.Delay(5); // Simular operación asíncrona
        
        _logger.LogInformation("Tarea con ID {TaskId} eliminada exitosamente", id);
        
        return true;
    }

    public async Task<IEnumerable<TaskResponseDto>> GetTasksByStatusAsync(bool isCompleted)
    {
        _logger.LogInformation("Obteniendo tareas por estado completado: {IsCompleted}", isCompleted);
        
        await Task.Delay(10); // Simular operación asíncrona
        
        var filteredTasks = _tasks.Where(t => t.IsCompleted == isCompleted);
        
        return filteredTasks.Select(MapToResponseDto).OrderBy(t => t.CreatedAt);
    }

    public async Task<IEnumerable<TaskResponseDto>> GetTasksByPriorityAsync(int priority)
    {
        _logger.LogInformation("Obteniendo tareas por prioridad: {Priority}", priority);
        
        await Task.Delay(10); // Simular operación asíncrona
        
        var filteredTasks = _tasks.Where(t => t.Priority == priority);
        
        return filteredTasks.Select(MapToResponseDto).OrderBy(t => t.CreatedAt);
    }

    /// <summary>
    /// Mapea una entidad TaskItem a TaskResponseDto
    /// </summary>
    private static TaskResponseDto MapToResponseDto(TaskItem task)
    {
        return new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            Priority = task.Priority
        };
    }

    /// <summary>
    /// Inicializa datos de ejemplo para demostración
    /// </summary>
    private void SeedInitialData()
    {
        _logger.LogInformation("Inicializando datos de ejemplo");
        
        var sampleTasks = new[]
        {
            new TaskItem
            {
                Id = _nextId++,
                Title = "Configurar proyecto .NET 8",
                Description = "Crear un nuevo proyecto Web API con .NET 8 y configurar las dependencias básicas",
                Priority = 3,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new TaskItem
            {
                Id = _nextId++,
                Title = "Implementar logging con Serilog",
                Description = "Configurar Serilog para logging estructurado en la aplicación",
                Priority = 2,
                IsCompleted = true,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new TaskItem
            {
                Id = _nextId++,
                Title = "Escribir documentación de API",
                Description = "Crear documentación completa usando Swagger/OpenAPI",
                Priority = 1,
                CreatedAt = DateTime.UtcNow.AddHours(-3),
                UpdatedAt = DateTime.UtcNow.AddHours(-3)
            }
        };

        _tasks.AddRange(sampleTasks);
        
        _logger.LogInformation("Datos de ejemplo inicializados. Total de tareas: {Count}", _tasks.Count);
    }
}