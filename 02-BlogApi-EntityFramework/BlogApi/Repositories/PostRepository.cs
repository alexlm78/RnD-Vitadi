using BlogApi.Data;
using BlogApi.Models;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace BlogApi.Repositories;

/// <summary>
/// Implementación del repositorio para la entidad Post
/// Combina Entity Framework Core con acceso directo a Oracle usando Oracle.ManagedDataAccess.Core
/// </summary>
public class PostRepository : IPostRepository
{
    private readonly BlogDbContext _context;
    private readonly IConfiguration _configuration;

    public PostRepository(BlogDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Obtiene todos los posts usando Entity Framework
    /// </summary>
    public async Task<IEnumerable<Post>> GetAllAsync()
    {
        return await _context.Posts
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene todos los posts publicados usando LINQ
    /// </summary>
    public async Task<IEnumerable<Post>> GetPublishedAsync()
    {
        return await _context.Posts
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.PublishedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene un post por ID usando Entity Framework
    /// </summary>
    public async Task<Post?> GetByIdAsync(int id)
    {
        return await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>
    /// Obtiene un post con comentarios usando Include de Entity Framework
    /// </summary>
    public async Task<Post?> GetByIdWithCommentsAsync(int id)
    {
        return await _context.Posts
            .Include(p => p.Comments.Where(c => c.IsApproved))
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>
    /// Busca posts por título usando LINQ con Contains
    /// </summary>
    public async Task<IEnumerable<Post>> SearchByTitleAsync(string title)
    {
        return await _context.Posts
            .Where(p => p.Title.Contains(title))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene posts por autor usando LINQ
    /// </summary>
    public async Task<IEnumerable<Post>> GetByAuthorAsync(string author)
    {
        return await _context.Posts
            .Where(p => p.Author == author)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene posts por tag usando LINQ con Contains
    /// </summary>
    public async Task<IEnumerable<Post>> GetByTagAsync(string tag)
    {
        return await _context.Posts
            .Where(p => p.Tags != null && p.Tags.Contains(tag))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene posts paginados usando Skip y Take
    /// </summary>
    public async Task<IEnumerable<Post>> GetPagedAsync(int pageNumber, int pageSize)
    {
        return await _context.Posts
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene el conteo total usando Count
    /// </summary>
    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Posts.CountAsync();
    }

    /// <summary>
    /// Obtiene el conteo de posts publicados usando Count con condición
    /// </summary>
    public async Task<int> GetPublishedCountAsync()
    {
        return await _context.Posts
            .CountAsync(p => p.IsPublished);
    }

    /// <summary>
    /// Crea un nuevo post usando Entity Framework
    /// </summary>
    public async Task<Post> CreateAsync(Post post)
    {
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        return post;
    }

    /// <summary>
    /// Actualiza un post existente usando Entity Framework
    /// </summary>
    public async Task<Post> UpdateAsync(Post post)
    {
        _context.Posts.Update(post);
        await _context.SaveChangesAsync();
        return post;
    }

    /// <summary>
    /// Elimina un post por ID usando Entity Framework
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post == null)
            return false;

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Verifica si existe un post usando Any
    /// </summary>
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Posts.AnyAsync(p => p.Id == id);
    }

    /// <summary>
    /// Publica un post usando Entity Framework
    /// </summary>
    public async Task<bool> PublishAsync(int id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post == null)
            return false;

        post.IsPublished = true;
        post.PublishedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Despublica un post usando Entity Framework
    /// </summary>
    public async Task<bool> UnpublishAsync(int id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post == null)
            return false;

        post.IsPublished = false;
        post.PublishedAt = null;
        await _context.SaveChangesAsync();
        return true;
    }

    // Métodos adicionales que demuestran el uso directo de Oracle.ManagedDataAccess.Core

    /// <summary>
    /// Ejemplo de consulta directa usando Oracle.ManagedDataAccess.Core
    /// Obtiene estadísticas de posts por autor usando SQL directo
    /// </summary>
    public async Task<IEnumerable<dynamic>> GetPostStatsByAuthorAsync()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        var results = new List<dynamic>();

        using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync();

        var sql = @"
            SELECT 
                AUTHOR,
                COUNT(*) as TOTAL_POSTS,
                COUNT(CASE WHEN IS_PUBLISHED = 1 THEN 1 END) as PUBLISHED_POSTS,
                COUNT(CASE WHEN IS_PUBLISHED = 0 THEN 1 END) as DRAFT_POSTS,
                MIN(CREATED_AT) as FIRST_POST_DATE,
                MAX(CREATED_AT) as LAST_POST_DATE
            FROM POSTS 
            GROUP BY AUTHOR 
            ORDER BY TOTAL_POSTS DESC";

        using var command = new OracleCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(new
            {
                Author = reader.GetString(reader.GetOrdinal("AUTHOR")),
                TotalPosts = reader.GetInt32(reader.GetOrdinal("TOTAL_POSTS")),
                PublishedPosts = reader.GetInt32(reader.GetOrdinal("PUBLISHED_POSTS")),
                DraftPosts = reader.GetInt32(reader.GetOrdinal("DRAFT_POSTS")),
                FirstPostDate = reader.GetDateTime(reader.GetOrdinal("FIRST_POST_DATE")),
                LastPostDate = reader.GetDateTime(reader.GetOrdinal("LAST_POST_DATE"))
            });
        }

        return results;
    }

    /// <summary>
    /// Ejemplo de procedimiento almacenado usando Oracle.ManagedDataAccess.Core
    /// Actualiza múltiples posts usando un procedimiento almacenado
    /// </summary>
    public async Task<int> BulkUpdatePostStatusAsync(bool isPublished, string author)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync();

        // Ejemplo de llamada a procedimiento almacenado (simulado con SQL directo)
        var sql = @"
            UPDATE POSTS 
            SET IS_PUBLISHED = :isPublished,
                PUBLISHED_AT = CASE WHEN :isPublished = 1 THEN SYSTIMESTAMP ELSE NULL END,
                UPDATED_AT = SYSTIMESTAMP
            WHERE AUTHOR = :author";

        using var command = new OracleCommand(sql, connection);
        command.Parameters.Add(new OracleParameter("isPublished", isPublished ? 1 : 0));
        command.Parameters.Add(new OracleParameter("author", author));

        return await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Ejemplo de consulta compleja usando Oracle.ManagedDataAccess.Core
    /// Obtiene posts con estadísticas de comentarios usando SQL directo
    /// </summary>
    public async Task<IEnumerable<dynamic>> GetPostsWithCommentStatsAsync()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        var results = new List<dynamic>();

        using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync();

        var sql = @"
            SELECT 
                p.ID,
                p.TITLE,
                p.AUTHOR,
                p.CREATED_AT,
                p.IS_PUBLISHED,
                COUNT(c.ID) as TOTAL_COMMENTS,
                COUNT(CASE WHEN c.IS_APPROVED = 1 THEN 1 END) as APPROVED_COMMENTS,
                COUNT(CASE WHEN c.IS_APPROVED = 0 THEN 1 END) as PENDING_COMMENTS,
                MAX(c.CREATED_AT) as LAST_COMMENT_DATE
            FROM POSTS p
            LEFT JOIN COMMENTS c ON p.ID = c.POST_ID
            GROUP BY p.ID, p.TITLE, p.AUTHOR, p.CREATED_AT, p.IS_PUBLISHED
            ORDER BY p.CREATED_AT DESC";

        using var command = new OracleCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var lastCommentOrdinal = reader.GetOrdinal("LAST_COMMENT_DATE");
            results.Add(new
            {
                Id = reader.GetInt32(reader.GetOrdinal("ID")),
                Title = reader.GetString(reader.GetOrdinal("TITLE")),
                Author = reader.GetString(reader.GetOrdinal("AUTHOR")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CREATED_AT")),
                IsPublished = reader.GetInt32(reader.GetOrdinal("IS_PUBLISHED")) == 1,
                TotalComments = reader.GetInt32(reader.GetOrdinal("TOTAL_COMMENTS")),
                ApprovedComments = reader.GetInt32(reader.GetOrdinal("APPROVED_COMMENTS")),
                PendingComments = reader.GetInt32(reader.GetOrdinal("PENDING_COMMENTS")),
                LastCommentDate = reader.IsDBNull(lastCommentOrdinal) ? (DateTime?)null : reader.GetDateTime(lastCommentOrdinal)
            });
        }

        return results;
    }

    /// <summary>
    /// Ejemplo de transacción usando Oracle.ManagedDataAccess.Core
    /// Crea un post y sus comentarios en una transacción
    /// </summary>
    public async Task<bool> CreatePostWithCommentsAsync(Post post, IEnumerable<Comment> comments)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();
        try
        {
            // Insertar el post
            var insertPostSql = @"
                INSERT INTO POSTS (TITLE, CONTENT, SUMMARY, AUTHOR, CREATED_AT, UPDATED_AT, IS_PUBLISHED, PUBLISHED_AT, TAGS)
                VALUES (:title, :content, :summary, :author, :createdAt, :updatedAt, :isPublished, :publishedAt, :tags)
                RETURNING ID INTO :postId";

            using var postCommand = new OracleCommand(insertPostSql, connection);
            postCommand.Transaction = transaction;
            postCommand.Parameters.Add(new OracleParameter("title", post.Title));
            postCommand.Parameters.Add(new OracleParameter("content", post.Content));
            postCommand.Parameters.Add(new OracleParameter("summary", post.Summary ?? (object)DBNull.Value));
            postCommand.Parameters.Add(new OracleParameter("author", post.Author));
            postCommand.Parameters.Add(new OracleParameter("createdAt", post.CreatedAt));
            postCommand.Parameters.Add(new OracleParameter("updatedAt", post.UpdatedAt));
            postCommand.Parameters.Add(new OracleParameter("isPublished", post.IsPublished ? 1 : 0));
            postCommand.Parameters.Add(new OracleParameter("publishedAt", post.PublishedAt ?? (object)DBNull.Value));
            postCommand.Parameters.Add(new OracleParameter("tags", post.Tags ?? (object)DBNull.Value));

            var postIdParam = new OracleParameter("postId", OracleDbType.Int32, ParameterDirection.Output);
            postCommand.Parameters.Add(postIdParam);

            await postCommand.ExecuteNonQueryAsync();
            var postId = Convert.ToInt32(postIdParam.Value);

            // Insertar los comentarios
            foreach (var comment in comments)
            {
                var insertCommentSql = @"
                    INSERT INTO COMMENTS (CONTENT, AUTHOR_NAME, AUTHOR_EMAIL, CREATED_AT, IS_APPROVED, POST_ID)
                    VALUES (:content, :authorName, :authorEmail, :createdAt, :isApproved, :postId)";

                using var commentCommand = new OracleCommand(insertCommentSql, connection);
                commentCommand.Transaction = transaction;
                commentCommand.Parameters.Add(new OracleParameter("content", comment.Content));
                commentCommand.Parameters.Add(new OracleParameter("authorName", comment.AuthorName));
                commentCommand.Parameters.Add(new OracleParameter("authorEmail", comment.AuthorEmail));
                commentCommand.Parameters.Add(new OracleParameter("createdAt", comment.CreatedAt));
                commentCommand.Parameters.Add(new OracleParameter("isApproved", comment.IsApproved ? 1 : 0));
                commentCommand.Parameters.Add(new OracleParameter("postId", postId));

                await commentCommand.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}