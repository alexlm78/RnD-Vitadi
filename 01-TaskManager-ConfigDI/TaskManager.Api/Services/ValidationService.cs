using TaskManager.Api.DTOs;

namespace TaskManager.Api.Services;

/// <summary>
/// Implementación del servicio de validación
/// Demuestra validación de reglas de negocio usando dependency injection
/// </summary>
public class ValidationService : IValidationService
{
    private readonly ITaskService _taskService;
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<ValidationService> _logger;

    public ValidationService(
        ITaskService taskService,
        IConfigurationService configurationService,
        ILogger<ValidationService> logger)
    {
        _taskService = taskService;
        _configurationService = configurationService;
        _logger = logger;
    }

    public ValidationResult ValidateCreateTask(CreateTaskDto createTaskDto)
    {
        _logger.LogDebug("Validando creación de tarea: {Title}", createTaskDto.Title);

        var result = new ValidationResult { IsValid = true };

        // Validar título
        if (string.IsNullOrWhiteSpace(createTaskDto.Title))
        {
            result.AddError("El título es obligatorio");
        }
        else if (createTaskDto.Title.Length > 200)
        {
            result.AddError("El título no puede exceder 200 caracteres");
        }
        else if (ContainsInvalidCharacters(createTaskDto.Title))
        {
            result.AddError("El título contiene caracteres no permitidos");
        }

        // Validar descripción
        if (!string.IsNullOrEmpty(createTaskDto.Description) && createTaskDto.Description.Length > 1000)
        {
            result.AddError("La descripción no puede exceder 1000 caracteres");
        }

        // Validar prioridad
        if (createTaskDto.Priority < 1 || createTaskDto.Priority > 3)
        {
            result.AddError("La prioridad debe ser entre 1 (Baja) y 3 (Alta)");
        }

        // Validaciones de negocio específicas
        ValidateBusinessRules(createTaskDto.Title, createTaskDto.Priority, result);

        _logger.LogDebug("Validación de creación completada. Válido: {IsValid}, Errores: {ErrorCount}", 
            result.IsValid, result.Errors.Count);

        return result;
    }

    public ValidationResult ValidateUpdateTask(UpdateTaskDto updateTaskDto)
    {
        _logger.LogDebug("Validando actualización de tarea: {Title}", updateTaskDto.Title);

        var result = new ValidationResult { IsValid = true };

        // Validar título
        if (string.IsNullOrWhiteSpace(updateTaskDto.Title))
        {
            result.AddError("El título es obligatorio");
        }
        else if (updateTaskDto.Title.Length > 200)
        {
            result.AddError("El título no puede exceder 200 caracteres");
        }
        else if (ContainsInvalidCharacters(updateTaskDto.Title))
        {
            result.AddError("El título contiene caracteres no permitidos");
        }

        // Validar descripción
        if (!string.IsNullOrEmpty(updateTaskDto.Description) && updateTaskDto.Description.Length > 1000)
        {
            result.AddError("La descripción no puede exceder 1000 caracteres");
        }

        // Validar prioridad
        if (updateTaskDto.Priority < 1 || updateTaskDto.Priority > 3)
        {
            result.AddError("La prioridad debe ser entre 1 (Baja) y 3 (Alta)");
        }

        // Advertencia si se marca como completada una tarea de alta prioridad
        if (updateTaskDto.IsCompleted && updateTaskDto.Priority == 3)
        {
            result.AddWarning("Se está completando una tarea de alta prioridad. Verificar que esté realmente terminada.");
        }

        // Validaciones de negocio específicas
        ValidateBusinessRules(updateTaskDto.Title, updateTaskDto.Priority, result);

        _logger.LogDebug("Validación de actualización completada. Válido: {IsValid}, Errores: {ErrorCount}", 
            result.IsValid, result.Errors.Count);

        return result;
    }

    public async Task<ValidationResult> ValidateDeleteTaskAsync(int taskId)
    {
        _logger.LogDebug("Validando eliminación de tarea: {TaskId}", taskId);

        var result = new ValidationResult { IsValid = true };

        // Validar que el ID sea válido
        if (taskId <= 0)
        {
            result.AddError("El ID de la tarea debe ser mayor a 0");
            return result;
        }

        // Verificar que la tarea existe
        var task = await _taskService.GetTaskByIdAsync(taskId);
        if (task == null)
        {
            result.AddError($"La tarea con ID {taskId} no existe");
            return result;
        }

        // Validar reglas de negocio para eliminación
        if (task.Priority == 3 && !task.IsCompleted)
        {
            result.AddWarning("Se está eliminando una tarea de alta prioridad que no está completada");
        }

        // Verificar si hay funcionalidades que impiden la eliminación
        if (_configurationService.IsFeatureEnabled("TaskHistory") && task.IsCompleted)
        {
            result.AddWarning("La tarea completada se mantendrá en el historial aunque se elimine");
        }

        _logger.LogDebug("Validación de eliminación completada. Válido: {IsValid}, Errores: {ErrorCount}", 
            result.IsValid, result.Errors.Count);

        return result;
    }

    public ValidationResult ValidatePriorityFilter(int priority)
    {
        _logger.LogDebug("Validando filtro de prioridad: {Priority}", priority);

        var result = new ValidationResult { IsValid = true };

        if (priority < 1 || priority > 3)
        {
            result.AddError("La prioridad debe ser entre 1 (Baja), 2 (Media) y 3 (Alta)");
        }

        return result;
    }

    /// <summary>
    /// Valida reglas de negocio específicas
    /// </summary>
    private void ValidateBusinessRules(string title, int priority, ValidationResult result)
    {
        var taskManagerOptions = _configurationService.GetTaskManagerOptions();

        // Validar palabras prohibidas en títulos
        var prohibitedWords = new[] { "spam", "test123", "delete" };
        if (prohibitedWords.Any(word => title.ToLowerInvariant().Contains(word)))
        {
            result.AddError("El título contiene palabras no permitidas");
        }

        // Validar que tareas urgentes tengan descripción
        if (priority == 3 && string.IsNullOrWhiteSpace(title))
        {
            result.AddWarning("Las tareas de alta prioridad deberían tener una descripción detallada");
        }

        // Validar límites de configuración
        if (priority > taskManagerOptions.DefaultPriority + 1)
        {
            result.AddWarning($"La prioridad {priority} es mayor a la recomendada ({taskManagerOptions.DefaultPriority})");
        }
    }

    /// <summary>
    /// Verifica si el texto contiene caracteres no permitidos
    /// </summary>
    private static bool ContainsInvalidCharacters(string text)
    {
        var invalidChars = new[] { '<', '>', '"', '\'', '&' };
        return invalidChars.Any(text.Contains);
    }
}