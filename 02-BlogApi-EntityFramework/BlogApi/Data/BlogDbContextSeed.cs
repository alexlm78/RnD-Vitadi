using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Data;

/// <summary>
/// Clase para inicializar datos de ejemplo en la base de datos
/// Útil para desarrollo y testing
/// </summary>
public static class BlogDbContextSeed
{
    /// <summary>
    /// Inicializa la base de datos con datos de ejemplo
    /// </summary>
    /// <param name="context">Contexto de la base de datos</param>
    /// <returns>Task para operación asíncrona</returns>
    public static async Task SeedAsync(BlogDbContext context)
    {
        try
        {
            // Asegurar que la base de datos existe
            await context.Database.EnsureCreatedAsync();

            // Verificar si ya existen datos
            if (await context.Posts.AnyAsync())
            {
                return; // La base de datos ya tiene datos
            }

            // Crear posts de ejemplo
            var posts = new List<Post>
            {
                new Post
                {
                    Title = "Introducción a Entity Framework Core",
                    Content = @"Entity Framework Core es un ORM (Object-Relational Mapper) moderno para .NET. 
                               Permite trabajar con bases de datos usando objetos .NET, eliminando la necesidad 
                               de escribir la mayoría del código de acceso a datos. En este post exploraremos 
                               las características principales de EF Core y cómo configurarlo con Oracle.",
                    Summary = "Una introducción completa a Entity Framework Core y su configuración con Oracle.",
                    Author = "Juan Pérez",
                    IsPublished = true,
                    PublishedAt = DateTime.UtcNow.AddDays(-7),
                    Tags = "EntityFramework,Oracle,.NET,ORM"
                },
                new Post
                {
                    Title = "Patrones de Diseño en .NET: Repository Pattern",
                    Content = @"El patrón Repository es uno de los patrones más utilizados en aplicaciones .NET 
                               para abstraer el acceso a datos. Este patrón proporciona una interfaz uniforme 
                               para acceder a los datos, independientemente del tipo de almacenamiento utilizado. 
                               En este artículo veremos cómo implementar este patrón con Entity Framework Core.",
                    Summary = "Aprende a implementar el patrón Repository con Entity Framework Core.",
                    Author = "María García",
                    IsPublished = true,
                    PublishedAt = DateTime.UtcNow.AddDays(-5),
                    Tags = "Repository,DesignPatterns,.NET,Architecture"
                },
                new Post
                {
                    Title = "Migraciones en Entity Framework Core",
                    Content = @"Las migraciones en Entity Framework Core permiten evolucionar el esquema de la 
                               base de datos de manera controlada y versionada. Son especialmente útiles cuando 
                               trabajamos en equipo o cuando necesitamos desplegar cambios en producción. 
                               Este post cubre todo lo que necesitas saber sobre migraciones.",
                    Summary = "Todo sobre migraciones en EF Core: creación, aplicación y mejores prácticas.",
                    Author = "Carlos López",
                    IsPublished = false, // Post en borrador
                    Tags = "Migrations,EntityFramework,Database,DevOps"
                }
            };

            // Agregar posts al contexto
            await context.Posts.AddRangeAsync(posts);
            await context.SaveChangesAsync();

            // Crear comentarios de ejemplo para los posts publicados
            var publishedPosts = await context.Posts
                .Where(p => p.IsPublished)
                .ToListAsync();

            var comments = new List<Comment>();

            foreach (var post in publishedPosts)
            {
                comments.AddRange(new[]
                {
                    new Comment
                    {
                        PostId = post.Id,
                        Content = "Excelente artículo, muy bien explicado. Me ha ayudado mucho a entender estos conceptos.",
                        AuthorName = "Ana Martínez",
                        AuthorEmail = "ana.martinez@email.com",
                        IsApproved = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-2)
                    },
                    new Comment
                    {
                        PostId = post.Id,
                        Content = "¿Podrías agregar más ejemplos prácticos? Me gustaría ver casos de uso más complejos.",
                        AuthorName = "Roberto Silva",
                        AuthorEmail = "roberto.silva@email.com",
                        IsApproved = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-1)
                    }
                });
            }

            // Agregar algunos comentarios pendientes de aprobación
            if (publishedPosts.Any())
            {
                comments.Add(new Comment
                {
                    PostId = publishedPosts.First().Id,
                    Content = "Este comentario está pendiente de moderación.",
                    AuthorName = "Usuario Anónimo",
                    AuthorEmail = "anonimo@email.com",
                    IsApproved = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Agregar comentarios al contexto
            await context.Comments.AddRangeAsync(comments);
            await context.SaveChangesAsync();

            Console.WriteLine($"Base de datos inicializada con {posts.Count} posts y {comments.Count} comentarios.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al inicializar la base de datos: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Limpia todos los datos de la base de datos
    /// Útil para testing o reinicialización
    /// </summary>
    /// <param name="context">Contexto de la base de datos</param>
    /// <returns>Task para operación asíncrona</returns>
    public static async Task ClearDataAsync(BlogDbContext context)
    {
        try
        {
            // Eliminar comentarios primero (por la relación de clave foránea)
            context.Comments.RemoveRange(context.Comments);
            
            // Eliminar posts
            context.Posts.RemoveRange(context.Posts);
            
            await context.SaveChangesAsync();
            
            Console.WriteLine("Datos de la base de datos eliminados correctamente.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al limpiar la base de datos: {ex.Message}");
            throw;
        }
    }
}