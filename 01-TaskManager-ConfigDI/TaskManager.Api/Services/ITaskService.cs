using TaskManager.Api.DTOs;

namespace TaskManager.Api.Services;

/// <summary>
/// Interfaz para el servicio de gestión de tareas
/// Define las operaciones CRUD disponibles para las tareas
/// </summary>
public interface ITaskService
{
    /// <summary>
    /// Obtiene todas las tareas
    /// </summary>
    /// <returns>Lista de tareas</returns>
    Task<IEnumerable<TaskResponseDto>> GetAllTasksAsync();

    /// <summary>
    /// Obtiene una tarea por su ID
    /// </summary>
    /// <param name="id">ID de la tarea</param>
    /// <returns>Tarea encontrada o null si no existe</returns>
    Task<TaskResponseDto?> GetTaskByIdAsync(int id);

    /// <summary>
    /// Crea una nueva tarea
    /// </summary>
    /// <param name="createTaskDto">Datos de la tarea a crear</param>
    /// <returns>Tarea creada</returns>
    Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto createTaskDto);

    /// <summary>
    /// Actualiza una tarea existente
    /// </summary>
    /// <param name="id">ID de la tarea a actualizar</param>
    /// <param name="updateTaskDto">Nuevos datos de la tarea</param>
    /// <returns>Tarea actualizada o null si no existe</returns>
    Task<TaskResponseDto?> UpdateTaskAsync(int id, UpdateTaskDto updateTaskDto);

    /// <summary>
    /// Elimina una tarea
    /// </summary>
    /// <param name="id">ID de la tarea a eliminar</param>
    /// <returns>True si se eliminó correctamente, false si no existe</returns>
    Task<bool> DeleteTaskAsync(int id);

    /// <summary>
    /// Obtiene tareas filtradas por estado de completado
    /// </summary>
    /// <param name="isCompleted">Estado de completado a filtrar</param>
    /// <returns>Lista de tareas filtradas</returns>
    Task<IEnumerable<TaskResponseDto>> GetTasksByStatusAsync(bool isCompleted);

    /// <summary>
    /// Obtiene tareas filtradas por prioridad
    /// </summary>
    /// <param name="priority">Prioridad a filtrar (1=Baja, 2=Media, 3=Alta)</param>
    /// <returns>Lista de tareas filtradas</returns>
    Task<IEnumerable<TaskResponseDto>> GetTasksByPriorityAsync(int priority);
}