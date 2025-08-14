using BlogApi.Data;
using BlogApi.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configuración de Entity Framework con Oracle
builder.Services.AddDbContext<BlogDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseOracle(connectionString);
    
    // Configuraciones adicionales para desarrollo
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Registro de repositorios en el contenedor de dependencias
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();

// Configuración de Controllers
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Blog API - Entity Framework Demo",
        Version = "v1",
        Description = "API de demostración que muestra el uso de Entity Framework Core con Oracle, " +
                     "implementando el patrón Repository y consultas complejas con LINQ y SQL directo.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Desarrollador",
            Email = "developer@example.com"
        }
    });

    // Incluir comentarios XML en la documentación
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Configurar el pipeline para controllers
app.MapControllers();

// Configurar endpoints usando Repository pattern
app.MapGet("/posts", async (IPostRepository postRepository) =>
{
    var posts = await postRepository.GetPublishedAsync();
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
    
    return Results.Ok(result);
})
.WithName("GetPosts")
.WithOpenApi()
.WithSummary("Obtiene todos los posts publicados usando Repository pattern")
.WithDescription("Retorna una lista de todos los posts publicados con el número de comentarios aprobados");

app.MapGet("/posts/{id:int}", async (int id, IPostRepository postRepository) =>
{
    var post = await postRepository.GetByIdWithCommentsAsync(id);
    
    if (post == null)
        return Results.NotFound($"Post con ID {id} no encontrado");
    
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
    
    return Results.Ok(result);
})
.WithName("GetPostById")
.WithOpenApi()
.WithSummary("Obtiene un post específico por ID usando Repository pattern")
.WithDescription("Retorna un post específico con todos sus comentarios aprobados");

// Endpoints adicionales para demostrar el Repository pattern
app.MapGet("/posts/author/{author}", async (string author, IPostRepository postRepository) =>
{
    var posts = await postRepository.GetByAuthorAsync(author);
    return Results.Ok(posts);
})
.WithName("GetPostsByAuthor")
.WithOpenApi()
.WithSummary("Obtiene posts por autor")
.WithDescription("Retorna todos los posts de un autor específico");

app.MapGet("/posts/search/{title}", async (string title, IPostRepository postRepository) =>
{
    var posts = await postRepository.SearchByTitleAsync(title);
    return Results.Ok(posts);
})
.WithName("SearchPostsByTitle")
.WithOpenApi()
.WithSummary("Busca posts por título")
.WithDescription("Busca posts que contengan el texto especificado en el título");

app.MapGet("/posts/tag/{tag}", async (string tag, IPostRepository postRepository) =>
{
    var posts = await postRepository.GetByTagAsync(tag);
    return Results.Ok(posts);
})
.WithName("GetPostsByTag")
.WithOpenApi()
.WithSummary("Obtiene posts por tag")
.WithDescription("Retorna posts que contengan el tag especificado");

app.MapGet("/posts/stats/authors", async (IPostRepository postRepository) =>
{
    var stats = await ((PostRepository)postRepository).GetPostStatsByAuthorAsync();
    return Results.Ok(stats);
})
.WithName("GetPostStatsByAuthor")
.WithOpenApi()
.WithSummary("Obtiene estadísticas de posts por autor usando SQL directo")
.WithDescription("Demuestra el uso de Oracle.ManagedDataAccess.Core para consultas complejas");

app.MapGet("/posts/with-comment-stats", async (IPostRepository postRepository) =>
{
    var stats = await ((PostRepository)postRepository).GetPostsWithCommentStatsAsync();
    return Results.Ok(stats);
})
.WithName("GetPostsWithCommentStats")
.WithOpenApi()
.WithSummary("Obtiene posts con estadísticas de comentarios usando SQL directo")
.WithDescription("Demuestra consultas complejas con JOIN usando Oracle.ManagedDataAccess.Core");

// Endpoints para comentarios usando Repository pattern
app.MapGet("/comments", async (ICommentRepository commentRepository) =>
{
    var comments = await commentRepository.GetApprovedAsync();
    return Results.Ok(comments);
})
.WithName("GetComments")
.WithOpenApi()
.WithSummary("Obtiene todos los comentarios aprobados")
.WithDescription("Retorna una lista de comentarios aprobados usando Repository pattern");

app.MapGet("/comments/post/{postId:int}", async (int postId, ICommentRepository commentRepository) =>
{
    var comments = await commentRepository.GetApprovedByPostIdAsync(postId);
    return Results.Ok(comments);
})
.WithName("GetCommentsByPost")
.WithOpenApi()
.WithSummary("Obtiene comentarios de un post específico")
.WithDescription("Retorna comentarios aprobados de un post usando Repository pattern");

app.MapGet("/comments/pending", async (ICommentRepository commentRepository) =>
{
    var comments = await commentRepository.GetPendingAsync();
    return Results.Ok(comments);
})
.WithName("GetPendingComments")
.WithOpenApi()
.WithSummary("Obtiene comentarios pendientes de aprobación")
.WithDescription("Retorna comentarios que requieren moderación");

app.MapGet("/comments/stats", async (ICommentRepository commentRepository) =>
{
    var stats = await ((CommentRepository)commentRepository).GetCommentStatsByPostAsync();
    return Results.Ok(stats);
})
.WithName("GetCommentStats")
.WithOpenApi()
.WithSummary("Obtiene estadísticas de comentarios por post usando LINQ")
.WithDescription("Demuestra el uso de GroupBy y agregaciones con LINQ");

app.MapPost("/comments/{id:int}/approve", async (int id, ICommentRepository commentRepository) =>
{
    var result = await commentRepository.ApproveAsync(id);
    return result ? Results.Ok("Comentario aprobado") : Results.NotFound("Comentario no encontrado");
})
.WithName("ApproveComment")
.WithOpenApi()
.WithSummary("Aprueba un comentario")
.WithDescription("Marca un comentario como aprobado para su visualización pública");

app.MapGet("/seed-data", async (BlogDbContext context) =>
{
    try
    {
        await BlogApi.Data.BlogDbContextSeed.SeedAsync(context);
        return Results.Ok("Datos de ejemplo creados correctamente");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error al crear datos de ejemplo: {ex.Message}");
    }
})
.WithName("SeedData")
.WithOpenApi()
.WithSummary("Inicializa la base de datos con datos de ejemplo")
.WithDescription("Crea posts y comentarios de ejemplo para probar la funcionalidad");

app.Run();
