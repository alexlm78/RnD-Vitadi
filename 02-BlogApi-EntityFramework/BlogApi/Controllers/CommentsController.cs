using BlogApi.Models;
using BlogApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Controllers;

/// <summary>
/// Controlador para la gestión de comentarios del blog
/// Demuestra el uso del patrón Repository con Entity Framework Core
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CommentsController : ControllerBase
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly ILogger<CommentsController> _logger;

    public CommentsController(
        ICommentRepository commentRepository, 
        IPostRepository postRepository,
        ILogger<CommentsController> logger)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los comentarios aprobados
    /// </summary>
    /// <returns>Lista de comentarios aprobados</returns>
    /// <response code="200">Retorna la lista de comentarios aprobados</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Comment>>> GetComments()
    {
        _logger.LogInformation("Obteniendo todos los comentarios aprobados");
        
        var comments = await _commentRepository.GetApprovedAsync();
        
        _logger.LogInformation("Se encontraron {Count} comentarios aprobados", comments.Count());
        return Ok(comments);
    }

    /// <summary>
    /// Obtiene un comentario específico por ID
    /// </summary>
    /// <param name="id">ID del comentario</param>
    /// <returns>Comentario solicitado</returns>
    /// <response code="200">Retorna el comentario solicitado</response>
    /// <response code="404">Si el comentario no existe</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Comment>> GetComment(int id)
    {
        _logger.LogInformation("Obteniendo comentario con ID: {CommentId}", id);
        
        var comment = await _commentRepository.GetByIdAsync(id);
        
        if (comment == null)
        {
            _logger.LogWarning("Comentario con ID {CommentId} no encontrado", id);
            return NotFound($"Comentario con ID {id} no encontrado");
        }

        _logger.LogInformation("Comentario {CommentId} encontrado", id);
        return Ok(comment);
    }

    /// <summary>
    /// Obtiene comentarios de un post específico
    /// </summary>
    /// <param name="postId">ID del post</param>
    /// <returns>Lista de comentarios del post</returns>
    /// <response code="200">Retorna los comentarios del post</response>
    /// <response code="404">Si el post no existe</response>
    [HttpGet("post/{postId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<Comment>>> GetCommentsByPost(int postId)
    {
        _logger.LogInformation("Obteniendo comentarios del post: {PostId}", postId);
        
        // Verificar que el post existe
        var postExists = await _postRepository.ExistsAsync(postId);
        if (!postExists)
        {
            _logger.LogWarning("Post con ID {PostId} no encontrado", postId);
            return NotFound($"Post con ID {postId} no encontrado");
        }

        var comments = await _commentRepository.GetApprovedByPostIdAsync(postId);
        
        _logger.LogInformation("Se encontraron {Count} comentarios para el post {PostId}", comments.Count(), postId);
        return Ok(comments);
    }

    /// <summary>
    /// Crea un nuevo comentario
    /// </summary>
    /// <param name="comment">Datos del comentario a crear</param>
    /// <returns>Comentario creado</returns>
    /// <response code="201">Comentario creado exitosamente</response>
    /// <response code="400">Si los datos del comentario son inválidos</response>
    /// <response code="404">Si el post asociado no existe</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Comment>> CreateComment([FromBody] Comment comment)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Intento de crear comentario con datos inválidos");
            return BadRequest(ModelState);
        }

        // Verificar que el post existe
        var postExists = await _postRepository.ExistsAsync(comment.PostId);
        if (!postExists)
        {
            _logger.LogWarning("Intento de crear comentario para post inexistente: {PostId}", comment.PostId);
            return NotFound($"Post con ID {comment.PostId} no encontrado");
        }

        _logger.LogInformation("Creando nuevo comentario para post: {PostId}", comment.PostId);
        
        comment.CreatedAt = DateTime.UtcNow;
        comment.IsApproved = false; // Los comentarios requieren aprobación por defecto
        
        var createdComment = await _commentRepository.CreateAsync(comment);
        
        _logger.LogInformation("Comentario creado exitosamente con ID: {CommentId}", createdComment.Id);
        return CreatedAtAction(nameof(GetComment), new { id = createdComment.Id }, createdComment);
    }

    /// <summary>
    /// Actualiza un comentario existente
    /// </summary>
    /// <param name="id">ID del comentario a actualizar</param>
    /// <param name="comment">Datos actualizados del comentario</param>
    /// <returns>Comentario actualizado</returns>
    /// <response code="200">Comentario actualizado exitosamente</response>
    /// <response code="400">Si los datos son inválidos</response>
    /// <response code="404">Si el comentario no existe</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Comment>> UpdateComment(int id, [FromBody] Comment comment)
    {
        if (id != comment.Id)
        {
            _logger.LogWarning("ID en URL ({UrlId}) no coincide con ID en body ({BodyId})", id, comment.Id);
            return BadRequest("El ID del comentario no coincide");
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Intento de actualizar comentario {CommentId} con datos inválidos", id);
            return BadRequest(ModelState);
        }

        var exists = await _commentRepository.ExistsAsync(id);
        if (!exists)
        {
            _logger.LogWarning("Intento de actualizar comentario inexistente: {CommentId}", id);
            return NotFound($"Comentario con ID {id} no encontrado");
        }

        _logger.LogInformation("Actualizando comentario: {CommentId}", id);
        
        var updatedComment = await _commentRepository.UpdateAsync(comment);
        
        _logger.LogInformation("Comentario {CommentId} actualizado exitosamente", id);
        return Ok(updatedComment);
    }

    /// <summary>
    /// Elimina un comentario
    /// </summary>
    /// <param name="id">ID del comentario a eliminar</param>
    /// <returns>Confirmación de eliminación</returns>
    /// <response code="204">Comentario eliminado exitosamente</response>
    /// <response code="404">Si el comentario no existe</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteComment(int id)
    {
        _logger.LogInformation("Eliminando comentario: {CommentId}", id);
        
        var result = await _commentRepository.DeleteAsync(id);
        
        if (!result)
        {
            _logger.LogWarning("Intento de eliminar comentario inexistente: {CommentId}", id);
            return NotFound($"Comentario con ID {id} no encontrado");
        }

        _logger.LogInformation("Comentario {CommentId} eliminado exitosamente", id);
        return NoContent();
    }

    /// <summary>
    /// Aprueba un comentario
    /// </summary>
    /// <param name="id">ID del comentario a aprobar</param>
    /// <returns>Confirmación de aprobación</returns>
    /// <response code="200">Comentario aprobado exitosamente</response>
    /// <response code="404">Si el comentario no existe</response>
    [HttpPost("{id:int}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveComment(int id)
    {
        _logger.LogInformation("Aprobando comentario: {CommentId}", id);
        
        var result = await _commentRepository.ApproveAsync(id);
        
        if (!result)
        {
            _logger.LogWarning("Intento de aprobar comentario inexistente: {CommentId}", id);
            return NotFound($"Comentario con ID {id} no encontrado");
        }

        _logger.LogInformation("Comentario {CommentId} aprobado exitosamente", id);
        return Ok(new { message = "Comentario aprobado exitosamente" });
    }

    /// <summary>
    /// Rechaza un comentario
    /// </summary>
    /// <param name="id">ID del comentario a rechazar</param>
    /// <returns>Confirmación de rechazo</returns>
    /// <response code="200">Comentario rechazado exitosamente</response>
    /// <response code="404">Si el comentario no existe</response>
    [HttpPost("{id:int}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectComment(int id)
    {
        _logger.LogInformation("Rechazando comentario: {CommentId}", id);
        
        var result = await _commentRepository.RejectAsync(id);
        
        if (!result)
        {
            _logger.LogWarning("Intento de rechazar comentario inexistente: {CommentId}", id);
            return NotFound($"Comentario con ID {id} no encontrado");
        }

        _logger.LogInformation("Comentario {CommentId} rechazado exitosamente", id);
        return Ok(new { message = "Comentario rechazado exitosamente" });
    }

    /// <summary>
    /// Obtiene comentarios pendientes de aprobación
    /// </summary>
    /// <returns>Lista de comentarios pendientes</returns>
    /// <response code="200">Retorna los comentarios pendientes</response>
    [HttpGet("pending")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Comment>>> GetPendingComments()
    {
        _logger.LogInformation("Obteniendo comentarios pendientes de aprobación");
        
        var comments = await _commentRepository.GetPendingAsync();
        
        _logger.LogInformation("Se encontraron {Count} comentarios pendientes", comments.Count());
        return Ok(comments);
    }

    /// <summary>
    /// Obtiene comentarios por email del autor
    /// </summary>
    /// <param name="email">Email del autor</param>
    /// <returns>Lista de comentarios del autor</returns>
    /// <response code="200">Retorna los comentarios del autor</response>
    [HttpGet("author/{email}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Comment>>> GetCommentsByAuthor(string email)
    {
        _logger.LogInformation("Obteniendo comentarios del autor: {Email}", email);
        
        var comments = await _commentRepository.GetByAuthorEmailAsync(email);
        
        _logger.LogInformation("Se encontraron {Count} comentarios del autor: {Email}", comments.Count(), email);
        return Ok(comments);
    }

    /// <summary>
    /// Obtiene estadísticas de comentarios
    /// </summary>
    /// <returns>Estadísticas generales de comentarios</returns>
    /// <response code="200">Retorna las estadísticas</response>
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> GetCommentStats()
    {
        _logger.LogInformation("Obteniendo estadísticas de comentarios");
        
        var totalCount = await _commentRepository.GetTotalCountAsync();
        var approvedCount = await _commentRepository.GetApprovedCountAsync();
        var pendingCount = await _commentRepository.GetPendingCountAsync();

        var stats = new
        {
            TotalComments = totalCount,
            ApprovedComments = approvedCount,
            PendingComments = pendingCount,
            RejectedComments = totalCount - approvedCount - pendingCount,
            ApprovalRate = totalCount > 0 ? Math.Round((double)approvedCount / totalCount * 100, 2) : 0
        };

        _logger.LogInformation("Estadísticas calculadas: {TotalComments} total, {ApprovedComments} aprobados, {PendingComments} pendientes", 
            totalCount, approvedCount, pendingCount);
        return Ok(stats);
    }

    /// <summary>
    /// Obtiene estadísticas de comentarios por post (usando consultas complejas)
    /// </summary>
    /// <returns>Estadísticas de comentarios agrupadas por post</returns>
    /// <response code="200">Retorna las estadísticas por post</response>
    [HttpGet("stats/by-post")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> GetCommentStatsByPost()
    {
        _logger.LogInformation("Obteniendo estadísticas de comentarios por post");
        
        // Esta funcionalidad requiere que el CommentRepository tenga el método GetCommentStatsByPostAsync
        // que fue mencionado en el Program.cs existente
        var stats = await ((CommentRepository)_commentRepository).GetCommentStatsByPostAsync();
        
        _logger.LogInformation("Estadísticas por post calculadas para {Count} posts", stats.Count());
        return Ok(stats);
    }
}