using TaskManager.Api.DTOs;

namespace TaskManager.Api.Services;

/// <summary>
/// Interfaz para el servicio de validación
/// Demuestra validación de negocio separada de validación de modelo
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Valida los datos para crear una tarea
    /// </summary>
    /// <param name="createTaskDto">Datos de la tarea a crear</param>
    /// <returns>Resultado de validación</returns>
    ValidationResult ValidateCreateTask(CreateTaskDto createTaskDto);

    /// <summary>
    /// Valida los datos para actualizar una tarea
    /// </summary>
    /// <param name="updateTaskDto">Datos de la tarea a actualizar</param>
    /// <returns>Resultado de validación</returns>
    ValidationResult ValidateUpdateTask(UpdateTaskDto updateTaskDto);

    /// <summary>
    /// Valida si se puede eliminar una tarea
    /// </summary>
    /// <param name="taskId">ID de la tarea</param>
    /// <returns>Resultado de validación</returns>
    Task<ValidationResult> ValidateDeleteTaskAsync(int taskId);

    /// <summary>
    /// Valida parámetros de filtrado
    /// </summary>
    /// <param name="priority">Prioridad a filtrar</param>
    /// <returns>Resultado de validación</returns>
    ValidationResult ValidatePriorityFilter(int priority);
}

/// <summary>
/// Resultado de una operación de validación
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Indica si la validación fue exitosa
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Lista de errores de validación
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Lista de advertencias
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Crea un resultado de validación exitoso
    /// </summary>
    /// <returns>Resultado válido</returns>
    public static ValidationResult Success() => new() { IsValid = true };

    /// <summary>
    /// Crea un resultado de validación con errores
    /// </summary>
    /// <param name="errors">Lista de errores</param>
    /// <returns>Resultado inválido</returns>
    public static ValidationResult Failure(params string[] errors) => new()
    {
        IsValid = false,
        Errors = errors.ToList()
    };

    /// <summary>
    /// Crea un resultado de validación con errores y advertencias
    /// </summary>
    /// <param name="errors">Lista de errores</param>
    /// <param name="warnings">Lista de advertencias</param>
    /// <returns>Resultado inválido con advertencias</returns>
    public static ValidationResult Failure(string[] errors, string[] warnings) => new()
    {
        IsValid = false,
        Errors = errors.ToList(),
        Warnings = warnings.ToList()
    };

    /// <summary>
    /// Agrega un error al resultado
    /// </summary>
    /// <param name="error">Mensaje de error</param>
    public void AddError(string error)
    {
        Errors.Add(error);
        IsValid = false;
    }

    /// <summary>
    /// Agrega una advertencia al resultado
    /// </summary>
    /// <param name="warning">Mensaje de advertencia</param>
    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }
}