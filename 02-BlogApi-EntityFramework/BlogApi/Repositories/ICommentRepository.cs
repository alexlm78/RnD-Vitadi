using BlogApi.Models;

namespace BlogApi.Repositories;

/// <summary>
/// Interfaz del repositorio para la entidad Comment
/// Define las operaciones de acceso a datos para comentarios
/// </summary>
public interface ICommentRepository
{
    /// <summary>
    /// Obtiene todos los comentarios
    /// </summary>
    /// <returns>Lista de todos los comentarios</returns>
    Task<IEnumerable<Comment>> GetAllAsync();

    /// <summary>
    /// Obtiene todos los comentarios aprobados
    /// </summary>
    /// <returns>Lista de comentarios aprobados</returns>
    Task<IEnumerable<Comment>> GetApprovedAsync();

    /// <summary>
    /// Obtiene todos los comentarios pendientes de aprobación
    /// </summary>
    /// <returns>Lista de comentarios pendientes</returns>
    Task<IEnumerable<Comment>> GetPendingAsync();

    /// <summary>
    /// Obtiene un comentario por su ID
    /// </summary>
    /// <param name="id">ID del comentario</param>
    /// <returns>Comentario encontrado o null</returns>
    Task<Comment?> GetByIdAsync(int id);

    /// <summary>
    /// Obtiene comentarios por ID de post
    /// </summary>
    /// <param name="postId">ID del post</param>
    /// <returns>Lista de comentarios del post</returns>
    Task<IEnumerable<Comment>> GetByPostIdAsync(int postId);

    /// <summary>
    /// Obtiene comentarios aprobados por ID de post
    /// </summary>
    /// <param name="postId">ID del post</param>
    /// <returns>Lista de comentarios aprobados del post</returns>
    Task<IEnumerable<Comment>> GetApprovedByPostIdAsync(int postId);

    /// <summary>
    /// Obtiene comentarios por email del autor
    /// </summary>
    /// <param name="email">Email del autor</param>
    /// <returns>Lista de comentarios del autor</returns>
    Task<IEnumerable<Comment>> GetByAuthorEmailAsync(string email);

    /// <summary>
    /// Crea un nuevo comentario
    /// </summary>
    /// <param name="comment">Comentario a crear</param>
    /// <returns>Comentario creado</returns>
    Task<Comment> CreateAsync(Comment comment);

    /// <summary>
    /// Actualiza un comentario existente
    /// </summary>
    /// <param name="comment">Comentario a actualizar</param>
    /// <returns>Comentario actualizado</returns>
    Task<Comment> UpdateAsync(Comment comment);

    /// <summary>
    /// Elimina un comentario por su ID
    /// </summary>
    /// <param name="id">ID del comentario a eliminar</param>
    /// <returns>True si se eliminó, false si no se encontró</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Aprueba un comentario
    /// </summary>
    /// <param name="id">ID del comentario a aprobar</param>
    /// <returns>True si se aprobó, false si no se encontró</returns>
    Task<bool> ApproveAsync(int id);

    /// <summary>
    /// Rechaza un comentario (lo marca como no aprobado)
    /// </summary>
    /// <param name="id">ID del comentario a rechazar</param>
    /// <returns>True si se rechazó, false si no se encontró</returns>
    Task<bool> RejectAsync(int id);

    /// <summary>
    /// Obtiene el conteo total de comentarios
    /// </summary>
    /// <returns>Número total de comentarios</returns>
    Task<int> GetTotalCountAsync();

    /// <summary>
    /// Obtiene el conteo de comentarios aprobados
    /// </summary>
    /// <returns>Número de comentarios aprobados</returns>
    Task<int> GetApprovedCountAsync();

    /// <summary>
    /// Obtiene el conteo de comentarios pendientes
    /// </summary>
    /// <returns>Número de comentarios pendientes</returns>
    Task<int> GetPendingCountAsync();

    /// <summary>
    /// Verifica si existe un comentario con el ID especificado
    /// </summary>
    /// <param name="id">ID del comentario</param>
    /// <returns>True si existe, false si no</returns>
    Task<bool> ExistsAsync(int id);
}