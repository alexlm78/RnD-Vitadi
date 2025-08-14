using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TaskManager.Api.DTOs;
using TaskManager.Api.Services;

namespace TaskManager.Api.Controllers;

/// <summary>
/// Controlador para la gestión de tareas
/// Demuestra el uso de dependency injection y configuración en ASP.NET Core
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly IValidationService _validationService;
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<TasksController> _logger;

    /// <summary>
    /// Constructor que recibe las dependencias inyectadas
    /// Demuestra inyección de múltiples servicios
    /// </summary>
    /// <param name="taskService">Servicio de gestión de tareas</param>
    /// <param name="validationService">Servicio de validación</param>
    /// <param name="configurationService">Servicio de configuración</param>
    /// <param name="logger">Logger para registrar eventos</param>
    public TasksController(
        ITaskService taskService, 
        IValidationService validationService,
        IConfigurationService configurationService,
        ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _validationService = validationService;
        _configurationService = configurationService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las tareas del sistema
    /// </summary>
    /// <returns>Lista de todas las tareas ordenadas por fecha de creación</returns>
    /// <response code="200">Lista de tareas obtenida exitosamente</response>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Obtener todas las tareas",
        Description = "Retorna una lista completa de todas las tareas en el sistema, ordenadas por fecha de creación ascendente.",
        OperationId = "GetAllTasks",
        Tags = new[] { "Gestión de Tareas" }
    )]
    [SwaggerResponse(200, "Lista de tareas obtenida exitosamente", typeof(IEnumerable<TaskResponseDto>))]
    [ProducesResponseType(typeof(IEnumerable<TaskResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetAllTasks()
    {
        _logger.LogInformation("Solicitud para obtener todas las tareas");
        
        var tasks = await _taskService.GetAllTasksAsync();
        
        return Ok(tasks);
    }

    /// <summary>
    /// Obtiene una tarea específica por su ID
    /// </summary>
    /// <param name="id">ID único de la tarea a buscar</param>
    /// <returns>Tarea encontrada con todos sus detalles</returns>
    /// <response code="200">Tarea encontrada exitosamente</response>
    /// <response code="404">No se encontró una tarea con el ID especificado</response>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Obtener tarea por ID",
        Description = "Busca y retorna una tarea específica utilizando su identificador único.",
        OperationId = "GetTaskById",
        Tags = new[] { "Gestión de Tareas" }
    )]
    [SwaggerResponse(200, "Tarea encontrada exitosamente", typeof(TaskResponseDto))]
    [SwaggerResponse(404, "Tarea no encontrada")]
    [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponseDto>> GetTask(
        [SwaggerParameter("ID único de la tarea", Required = true)] int id)
    {
        _logger.LogInformation("Solicitud para obtener tarea con ID: {TaskId}", id);
        
        var task = await _taskService.GetTaskByIdAsync(id);
        
        if (task == null)
        {
            return NotFound($"Tarea con ID {id} no encontrada");
        }

        return Ok(task);
    }

    /// <summary>
    /// Crea una nueva tarea en el sistema
    /// </summary>
    /// <param name="createTaskDto">Datos de la tarea a crear</param>
    /// <returns>Tarea creada con su ID asignado</returns>
    /// <response code="201">Tarea creada exitosamente</response>
    /// <response code="400">Datos de entrada inválidos o validación de negocio fallida</response>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Crear nueva tarea",
        Description = "Crea una nueva tarea en el sistema con validación de negocio completa. " +
                     "Incluye validación de caracteres especiales, palabras prohibidas y límites configurables.",
        OperationId = "CreateTask",
        Tags = new[] { "Gestión de Tareas" }
    )]
    [SwaggerResponse(201, "Tarea creada exitosamente", typeof(TaskResponseDto))]
    [SwaggerResponse(400, "Datos inválidos o validación de negocio fallida")]
    [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskResponseDto>> CreateTask(
        [FromBody, SwaggerRequestBody("Datos de la nueva tarea", Required = true)] CreateTaskDto createTaskDto)
    {
        _logger.LogInformation("Solicitud para crear nueva tarea: {Title}", createTaskDto.Title);
        
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Validación de negocio usando el servicio de validación
        var validationResult = _validationService.ValidateCreateTask(createTaskDto);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validación fallida para crear tarea: {Errors}", string.Join(", ", validationResult.Errors));
            return BadRequest(new
            {
                Errors = validationResult.Errors,
                Warnings = validationResult.Warnings
            });
        }

        try
        {
            var createdTask = await _taskService.CreateTaskAsync(createTaskDto);
            
            // Incluir advertencias en la respuesta si las hay
            if (validationResult.Warnings.Any())
            {
                Response.Headers.Append("X-Warnings", string.Join("; ", validationResult.Warnings));
            }
            
            return CreatedAtAction(
                nameof(GetTask), 
                new { id = createdTask.Id }, 
                createdTask);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error de operación al crear tarea");
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza una tarea existente
    /// </summary>
    /// <param name="id">ID de la tarea a actualizar</param>
    /// <param name="updateTaskDto">Nuevos datos de la tarea</param>
    /// <returns>Tarea actualizada con los nuevos valores</returns>
    /// <response code="200">Tarea actualizada exitosamente</response>
    /// <response code="400">Datos de entrada inválidos o validación fallida</response>
    /// <response code="404">Tarea no encontrada</response>
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Actualizar tarea existente",
        Description = "Actualiza todos los campos de una tarea existente. " +
                     "Incluye validación de negocio y puede generar advertencias en headers.",
        OperationId = "UpdateTask",
        Tags = new[] { "Gestión de Tareas" }
    )]
    [SwaggerResponse(200, "Tarea actualizada exitosamente", typeof(TaskResponseDto))]
    [SwaggerResponse(400, "Datos inválidos o validación fallida")]
    [SwaggerResponse(404, "Tarea no encontrada")]
    [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponseDto>> UpdateTask(
        [SwaggerParameter("ID de la tarea a actualizar", Required = true)] int id,
        [FromBody, SwaggerRequestBody("Nuevos datos de la tarea", Required = true)] UpdateTaskDto updateTaskDto)
    {
        _logger.LogInformation("Solicitud para actualizar tarea con ID: {TaskId}", id);
        
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Validación de negocio usando el servicio de validación
        var validationResult = _validationService.ValidateUpdateTask(updateTaskDto);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validación fallida para actualizar tarea {TaskId}: {Errors}", id, string.Join(", ", validationResult.Errors));
            return BadRequest(new
            {
                Errors = validationResult.Errors,
                Warnings = validationResult.Warnings
            });
        }

        var updatedTask = await _taskService.UpdateTaskAsync(id, updateTaskDto);
        
        if (updatedTask == null)
        {
            return NotFound($"Tarea con ID {id} no encontrada");
        }

        // Incluir advertencias en la respuesta si las hay
        if (validationResult.Warnings.Any())
        {
            Response.Headers.Append("X-Warnings", string.Join("; ", validationResult.Warnings));
        }

        return Ok(updatedTask);
    }

    /// <summary>
    /// Elimina una tarea del sistema
    /// </summary>
    /// <param name="id">ID de la tarea a eliminar</param>
    /// <returns>Resultado de la operación de eliminación</returns>
    /// <response code="204">Tarea eliminada exitosamente</response>
    /// <response code="400">Validación de eliminación fallida</response>
    /// <response code="404">Tarea no encontrada</response>
    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Eliminar tarea",
        Description = "Elimina permanentemente una tarea del sistema. " +
                     "Incluye validaciones de negocio para tareas de alta prioridad.",
        OperationId = "DeleteTask",
        Tags = new[] { "Gestión de Tareas" }
    )]
    [SwaggerResponse(204, "Tarea eliminada exitosamente")]
    [SwaggerResponse(400, "Validación de eliminación fallida")]
    [SwaggerResponse(404, "Tarea no encontrada")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(
        [SwaggerParameter("ID de la tarea a eliminar", Required = true)] int id)
    {
        _logger.LogInformation("Solicitud para eliminar tarea con ID: {TaskId}", id);
        
        // Validación de negocio para eliminación
        var validationResult = await _validationService.ValidateDeleteTaskAsync(id);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validación fallida para eliminar tarea {TaskId}: {Errors}", id, string.Join(", ", validationResult.Errors));
            return BadRequest(new
            {
                Errors = validationResult.Errors,
                Warnings = validationResult.Warnings
            });
        }
        
        var deleted = await _taskService.DeleteTaskAsync(id);
        
        if (!deleted)
        {
            return NotFound($"Tarea con ID {id} no encontrada");
        }

        // Incluir advertencias en la respuesta si las hay
        if (validationResult.Warnings.Any())
        {
            Response.Headers.Append("X-Warnings", string.Join("; ", validationResult.Warnings));
        }

        return NoContent();
    }

    /// <summary>
    /// Obtiene tareas filtradas por estado de completado
    /// </summary>
    /// <param name="completed">Estado de completado (true para completadas, false para pendientes)</param>
    /// <returns>Lista de tareas filtradas por estado</returns>
    /// <response code="200">Lista de tareas filtradas exitosamente</response>
    [HttpGet("status/{completed}")]
    [SwaggerOperation(
        Summary = "Filtrar tareas por estado",
        Description = "Retorna todas las tareas que coinciden con el estado de completado especificado.",
        OperationId = "GetTasksByStatus",
        Tags = new[] { "Filtros y Búsqueda" }
    )]
    [SwaggerResponse(200, "Lista de tareas filtradas", typeof(IEnumerable<TaskResponseDto>))]
    [ProducesResponseType(typeof(IEnumerable<TaskResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetTasksByStatus(
        [SwaggerParameter("Estado de completado (true=completadas, false=pendientes)", Required = true)] bool completed)
    {
        _logger.LogInformation("Solicitud para obtener tareas por estado: {IsCompleted}", completed);
        
        var tasks = await _taskService.GetTasksByStatusAsync(completed);
        
        return Ok(tasks);
    }

    /// <summary>
    /// Obtiene tareas filtradas por nivel de prioridad
    /// </summary>
    /// <param name="priority">Nivel de prioridad (1=Baja, 2=Media, 3=Alta)</param>
    /// <returns>Lista de tareas con la prioridad especificada</returns>
    /// <response code="200">Lista de tareas filtradas exitosamente</response>
    /// <response code="400">Nivel de prioridad inválido</response>
    [HttpGet("priority/{priority}")]
    [SwaggerOperation(
        Summary = "Filtrar tareas por prioridad",
        Description = "Retorna todas las tareas que tienen el nivel de prioridad especificado. " +
                     "Los niveles válidos son: 1 (Baja), 2 (Media), 3 (Alta).",
        OperationId = "GetTasksByPriority",
        Tags = new[] { "Filtros y Búsqueda" }
    )]
    [SwaggerResponse(200, "Lista de tareas filtradas", typeof(IEnumerable<TaskResponseDto>))]
    [SwaggerResponse(400, "Nivel de prioridad inválido")]
    [ProducesResponseType(typeof(IEnumerable<TaskResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetTasksByPriority(
        [SwaggerParameter("Nivel de prioridad (1=Baja, 2=Media, 3=Alta)", Required = true)] int priority)
    {
        // Usar el servicio de validación para validar el filtro
        var validationResult = _validationService.ValidatePriorityFilter(priority);
        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                Errors = validationResult.Errors
            });
        }

        _logger.LogInformation("Solicitud para obtener tareas por prioridad: {Priority}", priority);
        
        var tasks = await _taskService.GetTasksByPriorityAsync(priority);
        
        return Ok(tasks);
    }

    /// <summary>
    /// Endpoint de salud para verificar el estado del controlador
    /// </summary>
    /// <returns>Estado detallado del servicio y configuración</returns>
    /// <response code="200">Servicio funcionando correctamente</response>
    [HttpGet("health")]
    [SwaggerOperation(
        Summary = "Health check del controlador",
        Description = "Verifica el estado de salud del controlador de tareas y retorna información de configuración.",
        OperationId = "TasksHealthCheck",
        Tags = new[] { "Monitoreo" }
    )]
    [SwaggerResponse(200, "Servicio funcionando correctamente")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        _logger.LogInformation("Verificación de salud del controlador de tareas");
        
        var environmentInfo = _configurationService.GetEnvironmentInfo();
        var taskManagerOptions = _configurationService.GetTaskManagerOptions();
        
        return Ok(new
        {
            Status = "Healthy",
            Service = "TaskManager Controller",
            Environment = environmentInfo.EnvironmentName,
            Version = environmentInfo.Version,
            Configuration = new
            {
                SeedData = taskManagerOptions.SeedData,
                MaxTasksPerUser = taskManagerOptions.MaxTasksPerUser,
                Features = taskManagerOptions.Features
            },
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Obtiene estadísticas detalladas de las tareas
    /// </summary>
    /// <returns>Estadísticas completas del sistema de tareas</returns>
    /// <response code="200">Estadísticas obtenidas exitosamente</response>
    /// <response code="404">Funcionalidad de métricas no habilitada</response>
    [HttpGet("stats")]
    [SwaggerOperation(
        Summary = "Obtener estadísticas de tareas",
        Description = "Retorna estadísticas detalladas del sistema incluyendo totales, completadas, pendientes y distribución por prioridad. " +
                     "Esta funcionalidad debe estar habilitada en la configuración (Features.EnableMetrics).",
        OperationId = "GetTaskStatistics",
        Tags = new[] { "Estadísticas" }
    )]
    [SwaggerResponse(200, "Estadísticas obtenidas exitosamente")]
    [SwaggerResponse(404, "Funcionalidad de métricas no habilitada")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaskStatistics()
    {
        // Verificar si la funcionalidad está habilitada usando configuración
        if (!_configurationService.IsFeatureEnabled("Metrics"))
        {
            _logger.LogWarning("Intento de acceso a estadísticas con funcionalidad deshabilitada");
            return NotFound("La funcionalidad de métricas no está habilitada");
        }

        _logger.LogInformation("Generando estadísticas de tareas");
        
        var allTasks = await _taskService.GetAllTasksAsync();
        var taskList = allTasks.ToList();
        
        var stats = new
        {
            TotalTasks = taskList.Count,
            CompletedTasks = taskList.Count(t => t.IsCompleted),
            PendingTasks = taskList.Count(t => !t.IsCompleted),
            ByPriority = new
            {
                High = taskList.Count(t => t.Priority == 3),
                Medium = taskList.Count(t => t.Priority == 2),
                Low = taskList.Count(t => t.Priority == 1)
            },
            CompletionRate = taskList.Count > 0 ? (double)taskList.Count(t => t.IsCompleted) / taskList.Count * 100 : 0,
            GeneratedAt = DateTime.UtcNow
        };
        
        return Ok(stats);
    }

    /// <summary>
    /// Busca tareas por texto en título o descripción
    /// </summary>
    /// <param name="searchTerm">Término de búsqueda</param>
    /// <returns>Lista de tareas que coinciden con el término de búsqueda</returns>
    /// <response code="200">Búsqueda realizada exitosamente</response>
    /// <response code="400">Término de búsqueda inválido</response>
    [HttpGet("search")]
    [SwaggerOperation(
        Summary = "Buscar tareas por texto",
        Description = "Busca tareas que contengan el término especificado en el título o descripción. " +
                     "La búsqueda es insensible a mayúsculas y minúsculas.",
        OperationId = "SearchTasks",
        Tags = new[] { "Filtros y Búsqueda" }
    )]
    [SwaggerResponse(200, "Búsqueda realizada exitosamente", typeof(IEnumerable<TaskResponseDto>))]
    [SwaggerResponse(400, "Término de búsqueda inválido")]
    [ProducesResponseType(typeof(IEnumerable<TaskResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<TaskResponseDto>>> SearchTasks(
        [FromQuery, SwaggerParameter("Término de búsqueda (mínimo 2 caracteres)", Required = true)] string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
        {
            return BadRequest("El término de búsqueda debe tener al menos 2 caracteres");
        }

        _logger.LogInformation("Búsqueda de tareas con término: {SearchTerm}", searchTerm);

        var allTasks = await _taskService.GetAllTasksAsync();
        var filteredTasks = allTasks.Where(t => 
            t.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            (!string.IsNullOrEmpty(t.Description) && t.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
        );

        return Ok(filteredTasks);
    }

    /// <summary>
    /// Obtiene un resumen de tareas con paginación
    /// </summary>
    /// <param name="page">Número de página (base 1)</param>
    /// <param name="pageSize">Tamaño de página</param>
    /// <returns>Lista paginada de tareas</returns>
    /// <response code="200">Lista paginada obtenida exitosamente</response>
    /// <response code="400">Parámetros de paginación inválidos</response>
    [HttpGet("paged")]
    [SwaggerOperation(
        Summary = "Obtener tareas con paginación",
        Description = "Retorna una lista paginada de tareas con metadatos de paginación en headers.",
        OperationId = "GetPagedTasks",
        Tags = new[] { "Gestión de Tareas" }
    )]
    [SwaggerResponse(200, "Lista paginada obtenida exitosamente", typeof(IEnumerable<TaskResponseDto>))]
    [SwaggerResponse(400, "Parámetros de paginación inválidos")]
    [ProducesResponseType(typeof(IEnumerable<TaskResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetPagedTasks(
        [FromQuery, SwaggerParameter("Número de página (mínimo 1)", Required = false)] int page = 1,
        [FromQuery, SwaggerParameter("Tamaño de página (1-50)", Required = false)] int pageSize = 10)
    {
        var paginationOptions = _configurationService.GetApiOptions().Pagination;
        
        if (page < 1)
        {
            return BadRequest("El número de página debe ser mayor a 0");
        }

        if (pageSize < 1 || pageSize > paginationOptions.MaxPageSize)
        {
            return BadRequest($"El tamaño de página debe estar entre 1 y {paginationOptions.MaxPageSize}");
        }

        _logger.LogInformation("Obteniendo tareas paginadas: página {Page}, tamaño {PageSize}", page, pageSize);

        var allTasks = await _taskService.GetAllTasksAsync();
        var taskList = allTasks.ToList();
        
        var totalItems = taskList.Count;
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
        var skip = (page - 1) * pageSize;
        var pagedTasks = taskList.Skip(skip).Take(pageSize);

        // Agregar metadatos de paginación en headers
        Response.Headers.Append("X-Total-Count", totalItems.ToString());
        Response.Headers.Append("X-Total-Pages", totalPages.ToString());
        Response.Headers.Append("X-Current-Page", page.ToString());
        Response.Headers.Append("X-Page-Size", pageSize.ToString());
        Response.Headers.Append("X-Has-Next", (page < totalPages).ToString());
        Response.Headers.Append("X-Has-Previous", (page > 1).ToString());

        return Ok(pagedTasks);
    }
}