using BlogApi.Data;
using BlogApi.Models;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace BlogApi.Repositories;

/// <summary>
/// Implementación del repositorio para la entidad Comment
/// Demuestra el uso de Entity Framework Core y consultas LINQ
/// </summary>
public class CommentRepository : ICommentRepository
{
    private readonly BlogDbContext _context;
    private readonly IConfiguration _configuration;

    public CommentRepository(BlogDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Obtiene todos los comentarios usando Entity Framework
    /// </summary>
    public async Task<IEnumerable<Comment>> GetAllAsync()
    {
        return await _context.Comments
            .Include(c => c.Post)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene comentarios aprobados usando LINQ
    /// </summary>
    public async Task<IEnumerable<Comment>> GetApprovedAsync()
    {
        return await _context.Comments
            .Include(c => c.Post)
            .Where(c => c.IsApproved)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene comentarios pendientes usando LINQ
    /// </summary>
    public async Task<IEnumerable<Comment>> GetPendingAsync()
    {
        return await _context.Comments
            .Include(c => c.Post)
            .Where(c => !c.IsApproved)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene un comentario por ID
    /// </summary>
    public async Task<Comment?> GetByIdAsync(int id)
    {
        return await _context.Comments
            .Include(c => c.Post)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    /// <summary>
    /// Obtiene comentarios por ID de post usando LINQ
    /// </summary>
    public async Task<IEnumerable<Comment>> GetByPostIdAsync(int postId)
    {
        return await _context.Comments
            .Where(c => c.PostId == postId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene comentarios aprobados por ID de post usando LINQ con múltiples condiciones
    /// </summary>
    public async Task<IEnumerable<Comment>> GetApprovedByPostIdAsync(int postId)
    {
        return await _context.Comments
            .Where(c => c.PostId == postId && c.IsApproved)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene comentarios por email del autor usando LINQ
    /// </summary>
    public async Task<IEnumerable<Comment>> GetByAuthorEmailAsync(string email)
    {
        return await _context.Comments
            .Include(c => c.Post)
            .Where(c => c.AuthorEmail == email)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Crea un nuevo comentario
    /// </summary>
    public async Task<Comment> CreateAsync(Comment comment)
    {
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    /// <summary>
    /// Actualiza un comentario existente
    /// </summary>
    public async Task<Comment> UpdateAsync(Comment comment)
    {
        _context.Comments.Update(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    /// <summary>
    /// Elimina un comentario por ID
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment == null)
            return false;

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Aprueba un comentario
    /// </summary>
    public async Task<bool> ApproveAsync(int id)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment == null)
            return false;

        comment.IsApproved = true;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Rechaza un comentario
    /// </summary>
    public async Task<bool> RejectAsync(int id)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment == null)
            return false;

        comment.IsApproved = false;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Obtiene el conteo total usando Count
    /// </summary>
    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Comments.CountAsync();
    }

    /// <summary>
    /// Obtiene el conteo de comentarios aprobados usando Count con condición
    /// </summary>
    public async Task<int> GetApprovedCountAsync()
    {
        return await _context.Comments.CountAsync(c => c.IsApproved);
    }

    /// <summary>
    /// Obtiene el conteo de comentarios pendientes usando Count con condición
    /// </summary>
    public async Task<int> GetPendingCountAsync()
    {
        return await _context.Comments.CountAsync(c => !c.IsApproved);
    }

    /// <summary>
    /// Verifica si existe un comentario usando Any
    /// </summary>
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Comments.AnyAsync(c => c.Id == id);
    }

    // Métodos adicionales que demuestran consultas LINQ más complejas

    /// <summary>
    /// Obtiene estadísticas de comentarios por post usando LINQ con GroupBy
    /// </summary>
    public async Task<IEnumerable<dynamic>> GetCommentStatsByPostAsync()
    {
        return await _context.Comments
            .GroupBy(c => new { c.PostId, c.Post.Title })
            .Select(g => new
            {
                PostId = g.Key.PostId,
                PostTitle = g.Key.Title,
                TotalComments = g.Count(),
                ApprovedComments = g.Count(c => c.IsApproved),
                PendingComments = g.Count(c => !c.IsApproved),
                FirstCommentDate = g.Min(c => c.CreatedAt),
                LastCommentDate = g.Max(c => c.CreatedAt)
            })
            .OrderByDescending(x => x.TotalComments)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene comentarios recientes usando LINQ con Take
    /// </summary>
    public async Task<IEnumerable<Comment>> GetRecentCommentsAsync(int count = 10)
    {
        return await _context.Comments
            .Include(c => c.Post)
            .Where(c => c.IsApproved)
            .OrderByDescending(c => c.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    /// <summary>
    /// Busca comentarios por contenido usando LINQ con Contains
    /// </summary>
    public async Task<IEnumerable<Comment>> SearchByContentAsync(string searchTerm)
    {
        return await _context.Comments
            .Include(c => c.Post)
            .Where(c => c.Content.Contains(searchTerm))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Ejemplo de consulta compleja usando Oracle.ManagedDataAccess.Core
    /// Obtiene comentarios con información del post usando SQL directo
    /// </summary>
    public async Task<IEnumerable<dynamic>> GetCommentsWithPostInfoAsync()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        var results = new List<dynamic>();

        using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync();

        var sql = @"
            SELECT 
                c.ID as COMMENT_ID,
                c.CONTENT,
                c.AUTHOR_NAME,
                c.AUTHOR_EMAIL,
                c.CREATED_AT as COMMENT_DATE,
                c.IS_APPROVED,
                p.ID as POST_ID,
                p.TITLE as POST_TITLE,
                p.AUTHOR as POST_AUTHOR,
                p.CREATED_AT as POST_DATE
            FROM COMMENTS c
            INNER JOIN POSTS p ON c.POST_ID = p.ID
            ORDER BY c.CREATED_AT DESC";

        using var command = new OracleCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(new
            {
                CommentId = reader.GetInt32(reader.GetOrdinal("COMMENT_ID")),
                Content = reader.GetString(reader.GetOrdinal("CONTENT")),
                AuthorName = reader.GetString(reader.GetOrdinal("AUTHOR_NAME")),
                AuthorEmail = reader.GetString(reader.GetOrdinal("AUTHOR_EMAIL")),
                CommentDate = reader.GetDateTime(reader.GetOrdinal("COMMENT_DATE")),
                IsApproved = reader.GetInt32(reader.GetOrdinal("IS_APPROVED")) == 1,
                PostId = reader.GetInt32(reader.GetOrdinal("POST_ID")),
                PostTitle = reader.GetString(reader.GetOrdinal("POST_TITLE")),
                PostAuthor = reader.GetString(reader.GetOrdinal("POST_AUTHOR")),
                PostDate = reader.GetDateTime(reader.GetOrdinal("POST_DATE"))
            });
        }

        return results;
    }

    /// <summary>
    /// Ejemplo de operación bulk usando Oracle.ManagedDataAccess.Core
    /// Aprueba múltiples comentarios de un post específico
    /// </summary>
    public async Task<int> BulkApproveCommentsByPostAsync(int postId)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        using var connection = new OracleConnection(connectionString);
        await connection.OpenAsync();

        var sql = @"
            UPDATE COMMENTS 
            SET IS_APPROVED = 1
            WHERE POST_ID = :postId AND IS_APPROVED = 0";

        using var command = new OracleCommand(sql, connection);
        command.Parameters.Add(new OracleParameter("postId", postId));

        return await command.ExecuteNonQueryAsync();
    }
}