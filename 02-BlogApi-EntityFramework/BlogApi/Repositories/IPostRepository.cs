using BlogApi.Models;

namespace BlogApi.Repositories;

/// <summary>
/// Interfaz del repositorio para la entidad Post
/// Define las operaciones de acceso a datos para posts
/// </summary>
public interface IPostRepository
{
    /// <summary>
    /// Obtiene todos los posts
    /// </summary>
    /// <returns>Lista de todos los posts</returns>
    Task<IEnumerable<Post>> GetAllAsync();

    /// <summary>
    /// Obtiene todos los posts publicados
    /// </summary>
    /// <returns>Lista de posts publicados</returns>
    Task<IEnumerable<Post>> GetPublishedAsync();

    /// <summary>
    /// Obtiene un post por su ID
    /// </summary>
    /// <param name="id">ID del post</param>
    /// <returns>Post encontrado o null</returns>
    Task<Post?> GetByIdAsync(int id);

    /// <summary>
    /// Obtiene un post por su ID incluyendo comentarios
    /// </summary>
    /// <param name="id">ID del post</param>
    /// <returns>Post con comentarios o null</returns>
    Task<Post?> GetByIdWithCommentsAsync(int id);

    /// <summary>
    /// Busca posts por título
    /// </summary>
    /// <param name="title">Título o parte del título a buscar</param>
    /// <returns>Lista de posts que coinciden</returns>
    Task<IEnumerable<Post>> SearchByTitleAsync(string title);

    /// <summary>
    /// Obtiene posts por autor
    /// </summary>
    /// <param name="author">Nombre del autor</param>
    /// <returns>Lista de posts del autor</returns>
    Task<IEnumerable<Post>> GetByAuthorAsync(string author);

    /// <summary>
    /// Obtiene posts por tag
    /// </summary>
    /// <param name="tag">Tag a buscar</param>
    /// <returns>Lista de posts que contienen el tag</returns>
    Task<IEnumerable<Post>> GetByTagAsync(string tag);

    /// <summary>
    /// Obtiene posts paginados
    /// </summary>
    /// <param name="pageNumber">Número de página (empezando en 1)</param>
    /// <param name="pageSize">Tamaño de página</param>
    /// <returns>Lista paginada de posts</returns>
    Task<IEnumerable<Post>> GetPagedAsync(int pageNumber, int pageSize);

    /// <summary>
    /// Obtiene el conteo total de posts
    /// </summary>
    /// <returns>Número total de posts</returns>
    Task<int> GetTotalCountAsync();

    /// <summary>
    /// Obtiene el conteo de posts publicados
    /// </summary>
    /// <returns>Número de posts publicados</returns>
    Task<int> GetPublishedCountAsync();

    /// <summary>
    /// Crea un nuevo post
    /// </summary>
    /// <param name="post">Post a crear</param>
    /// <returns>Post creado</returns>
    Task<Post> CreateAsync(Post post);

    /// <summary>
    /// Actualiza un post existente
    /// </summary>
    /// <param name="post">Post a actualizar</param>
    /// <returns>Post actualizado</returns>
    Task<Post> UpdateAsync(Post post);

    /// <summary>
    /// Elimina un post por su ID
    /// </summary>
    /// <param name="id">ID del post a eliminar</param>
    /// <returns>True si se eliminó, false si no se encontró</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Verifica si existe un post con el ID especificado
    /// </summary>
    /// <param name="id">ID del post</param>
    /// <returns>True si existe, false si no</returns>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Publica un post (cambia IsPublished a true y establece PublishedAt)
    /// </summary>
    /// <param name="id">ID del post a publicar</param>
    /// <returns>True si se publicó, false si no se encontró</returns>
    Task<bool> PublishAsync(int id);

    /// <summary>
    /// Despublica un post (cambia IsPublished a false)
    /// </summary>
    /// <param name="id">ID del post a despublicar</param>
    /// <returns>True si se despublicó, false si no se encontró</returns>
    Task<bool> UnpublishAsync(int id);
}