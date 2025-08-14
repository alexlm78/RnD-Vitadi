using BlogApi.Models;
using BlogApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Controllers;

/// <summary>
/// Controlador para la gestión de posts del blog
/// Demuestra el uso del patrón Repository con Entity Framework Core
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PostsController : ControllerBase
{
    private readonly IPostRepository _postRepository;
    private readonly ILogger<PostsController> _logger;

    public PostsController(IPostRepository postRepository, ILogger<PostsController> logger)
    {
        _postRepository = postRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los posts publicados
    /// </summary>
    /// <returns>Lista de posts publicados</returns>
    /// <response code="200">Retorna la lista de posts publicados</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<object>>> GetPosts()
    {
        _logger.LogInformation("Obteniendo todos los posts publicados");
        
        var posts = await _postRepository.GetPublishedAsync();
        
        var result = posts.Select(p => new
        {
            p.Id,
            p.Title,
            p.Summary,
            p.Author,
            p.CreatedAt,
            p.PublishedAt,
            p.Tags,
            CommentsCount = p.Comments.Count(c => c.IsApproved)
        });

        _logger.LogInformation("Se encontraron {Count} posts publicados", result.Count());
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un post específico por ID
    /// </summary>
    /// <param name="id">ID del post</param>
    /// <returns>Post con sus comentarios</returns>
    /// <response code="200">Retorna el post solicitado</response>
    /// <response code="404">Si el post no existe</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> GetPost(int id)
    {
        _logger.LogInformation("Obteniendo post con ID: {PostId}", id);
        
        var post = await _postRepository.GetByIdWithCommentsAsync(id);
        
        if (post == null)
        {
            _logger.LogWarning("Post con ID {PostId} no encontrado", id);
            return NotFound($"Post con ID {id} no encontrado");
        }

        var result = new
        {
            post.Id,
            post.Title,
            post.Content,
            post.Summary,
            post.Author,
            post.CreatedAt,
            post.UpdatedAt,
            post.PublishedAt,
            post.Tags,
            Comments = post.Comments.Where(c => c.IsApproved).Select(c => new
            {
                c.Id,
                c.Content,
                c.AuthorName,
                c.CreatedAt
            }).OrderBy(c => c.CreatedAt)
        };

        _logger.LogInformation("Post {PostId} encontrado con {CommentsCount} comentarios", id, result.Comments.Count());
        return Ok(result);
    }

    /// <summary>
    /// Crea un nuevo post
    /// </summary>
    /// <param name="post">Datos del post a crear</param>
    /// <returns>Post creado</returns>
    /// <response code="201">Post creado exitosamente</response>
    /// <response code="400">Si los datos del post son inválidos</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Post>> CreatePost([FromBody] Post post)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Intento de crear post con datos inválidos");
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Creando nuevo post: {Title}", post.Title);
        
        post.CreatedAt = DateTime.UtcNow;
        post.UpdatedAt = DateTime.UtcNow;
        
        var createdPost = await _postRepository.CreateAsync(post);
        
        _logger.LogInformation("Post creado exitosamente con ID: {PostId}", createdPost.Id);
        return CreatedAtAction(nameof(GetPost), new { id = createdPost.Id }, createdPost);
    }

    /// <summary>
    /// Actualiza un post existente
    /// </summary>
    /// <param name="id">ID del post a actualizar</param>
    /// <param name="post">Datos actualizados del post</param>
    /// <returns>Post actualizado</returns>
    /// <response code="200">Post actualizado exitosamente</response>
    /// <response code="400">Si los datos son inválidos</response>
    /// <response code="404">Si el post no existe</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Post>> UpdatePost(int id, [FromBody] Post post)
    {
        if (id != post.Id)
        {
            _logger.LogWarning("ID en URL ({UrlId}) no coincide con ID en body ({BodyId})", id, post.Id);
            return BadRequest("El ID del post no coincide");
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Intento de actualizar post {PostId} con datos inválidos", id);
            return BadRequest(ModelState);
        }

        var exists = await _postRepository.ExistsAsync(id);
        if (!exists)
        {
            _logger.LogWarning("Intento de actualizar post inexistente: {PostId}", id);
            return NotFound($"Post con ID {id} no encontrado");
        }

        _logger.LogInformation("Actualizando post: {PostId}", id);
        
        post.UpdatedAt = DateTime.UtcNow;
        var updatedPost = await _postRepository.UpdateAsync(post);
        
        _logger.LogInformation("Post {PostId} actualizado exitosamente", id);
        return Ok(updatedPost);
    }

    /// <summary>
    /// Elimina un post
    /// </summary>
    /// <param name="id">ID del post a eliminar</param>
    /// <returns>Confirmación de eliminación</returns>
    /// <response code="204">Post eliminado exitosamente</response>
    /// <response code="404">Si el post no existe</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePost(int id)
    {
        _logger.LogInformation("Eliminando post: {PostId}", id);
        
        var result = await _postRepository.DeleteAsync(id);
        
        if (!result)
        {
            _logger.LogWarning("Intento de eliminar post inexistente: {PostId}", id);
            return NotFound($"Post con ID {id} no encontrado");
        }

        _logger.LogInformation("Post {PostId} eliminado exitosamente", id);
        return NoContent();
    }

    /// <summary>
    /// Publica un post
    /// </summary>
    /// <param name="id">ID del post a publicar</param>
    /// <returns>Confirmación de publicación</returns>
    /// <response code="200">Post publicado exitosamente</response>
    /// <response code="404">Si el post no existe</response>
    [HttpPost("{id:int}/publish")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublishPost(int id)
    {
        _logger.LogInformation("Publicando post: {PostId}", id);
        
        var result = await _postRepository.PublishAsync(id);
        
        if (!result)
        {
            _logger.LogWarning("Intento de publicar post inexistente: {PostId}", id);
            return NotFound($"Post con ID {id} no encontrado");
        }

        _logger.LogInformation("Post {PostId} publicado exitosamente", id);
        return Ok(new { message = "Post publicado exitosamente" });
    }

    /// <summary>
    /// Despublica un post
    /// </summary>
    /// <param name="id">ID del post a despublicar</param>
    /// <returns>Confirmación de despublicación</returns>
    /// <response code="200">Post despublicado exitosamente</response>
    /// <response code="404">Si el post no existe</response>
    [HttpPost("{id:int}/unpublish")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnpublishPost(int id)
    {
        _logger.LogInformation("Despublicando post: {PostId}", id);
        
        var result = await _postRepository.UnpublishAsync(id);
        
        if (!result)
        {
            _logger.LogWarning("Intento de despublicar post inexistente: {PostId}", id);
            return NotFound($"Post con ID {id} no encontrado");
        }

        _logger.LogInformation("Post {PostId} despublicado exitosamente", id);
        return Ok(new { message = "Post despublicado exitosamente" });
    }

    /// <summary>
    /// Busca posts por título
    /// </summary>
    /// <param name="title">Texto a buscar en el título</param>
    /// <returns>Lista de posts que coinciden</returns>
    /// <response code="200">Retorna los posts encontrados</response>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Post>>> SearchPosts([FromQuery] string title)
    {
        _logger.LogInformation("Buscando posts por título: {SearchTerm}", title);
        
        var posts = await _postRepository.SearchByTitleAsync(title);
        
        _logger.LogInformation("Se encontraron {Count} posts para la búsqueda: {SearchTerm}", posts.Count(), title);
        return Ok(posts);
    }

    /// <summary>
    /// Obtiene posts por autor
    /// </summary>
    /// <param name="author">Nombre del autor</param>
    /// <returns>Lista de posts del autor</returns>
    /// <response code="200">Retorna los posts del autor</response>
    [HttpGet("author/{author}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Post>>> GetPostsByAuthor(string author)
    {
        _logger.LogInformation("Obteniendo posts del autor: {Author}", author);
        
        var posts = await _postRepository.GetByAuthorAsync(author);
        
        _logger.LogInformation("Se encontraron {Count} posts del autor: {Author}", posts.Count(), author);
        return Ok(posts);
    }

    /// <summary>
    /// Obtiene posts por tag
    /// </summary>
    /// <param name="tag">Tag a buscar</param>
    /// <returns>Lista de posts con el tag</returns>
    /// <response code="200">Retorna los posts con el tag</response>
    [HttpGet("tag/{tag}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Post>>> GetPostsByTag(string tag)
    {
        _logger.LogInformation("Obteniendo posts con tag: {Tag}", tag);
        
        var posts = await _postRepository.GetByTagAsync(tag);
        
        _logger.LogInformation("Se encontraron {Count} posts con tag: {Tag}", posts.Count(), tag);
        return Ok(posts);
    }

    /// <summary>
    /// Obtiene posts paginados
    /// </summary>
    /// <param name="pageNumber">Número de página (empezando en 1)</param>
    /// <param name="pageSize">Tamaño de página</param>
    /// <returns>Lista paginada de posts</returns>
    /// <response code="200">Retorna los posts paginados</response>
    /// <response code="400">Si los parámetros de paginación son inválidos</response>
    [HttpGet("paged")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<object>> GetPagedPosts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            _logger.LogWarning("Parámetros de paginación inválidos: pageNumber={PageNumber}, pageSize={PageSize}", pageNumber, pageSize);
            return BadRequest("Los parámetros de paginación deben ser mayores a 0");
        }

        _logger.LogInformation("Obteniendo posts paginados: página {PageNumber}, tamaño {PageSize}", pageNumber, pageSize);
        
        var posts = await _postRepository.GetPagedAsync(pageNumber, pageSize);
        var totalCount = await _postRepository.GetTotalCountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var result = new
        {
            Posts = posts,
            Pagination = new
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasPrevious = pageNumber > 1,
                HasNext = pageNumber < totalPages
            }
        };

        _logger.LogInformation("Retornando {Count} posts de {TotalCount} total", posts.Count(), totalCount);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene estadísticas de posts
    /// </summary>
    /// <returns>Estadísticas generales de posts</returns>
    /// <response code="200">Retorna las estadísticas</response>
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> GetPostStats()
    {
        _logger.LogInformation("Obteniendo estadísticas de posts");
        
        var totalCount = await _postRepository.GetTotalCountAsync();
        var publishedCount = await _postRepository.GetPublishedCountAsync();

        var stats = new
        {
            TotalPosts = totalCount,
            PublishedPosts = publishedCount,
            DraftPosts = totalCount - publishedCount,
            PublishedPercentage = totalCount > 0 ? Math.Round((double)publishedCount / totalCount * 100, 2) : 0
        };

        _logger.LogInformation("Estadísticas calculadas: {TotalPosts} total, {PublishedPosts} publicados", totalCount, publishedCount);
        return Ok(stats);
    }
}