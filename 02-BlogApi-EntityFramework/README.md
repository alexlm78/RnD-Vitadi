# BlogApi - Entity Framework Core con Oracle

Este proyecto demuestra el uso completo de Entity Framework Core con Oracle Database, implementando el patrón Repository y mostrando diferentes técnicas de acceso a datos, desde consultas LINQ básicas hasta SQL directo y transacciones complejas.

## 🎯 Objetivos de Aprendizaje

- **Entity Framework Core**: Configuración, relaciones y consultas
- **Oracle Database**: Integración con .NET usando Oracle.EntityFrameworkCore
- **Repository Pattern**: Abstracción y testabilidad de la capa de datos
- **Migraciones**: Gestión de esquema y versionado de base de datos
- **LINQ**: Consultas tipadas y expresivas
- **SQL Directo**: Acceso directo usando Oracle.ManagedDataAccess.Core
- **Transacciones**: Operaciones atómicas y consistencia de datos
- **Web API Controllers**: Implementación de APIs RESTful completas

## 🏗️ Arquitectura

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Controllers   │───▶│   Repositories  │───▶│   DbContext     │
│                 │    │                 │    │                 │
│ - PostsController│    │ - IPostRepository│    │ - BlogDbContext │
│ - CommentsController│ │ - PostRepository │    │ - Entity Config │
└─────────────────┘    │ - ICommentRepository│ └─────────────────┘
                       │ - CommentRepository │           │
                       └─────────────────┘           │
                                                     ▼
                                            ┌─────────────────┐
                                            │  Oracle Database │
                                            │                 │
                                            │ - Posts Table   │
                                            │ - Comments Table│
                                            └─────────────────┘
```

## 📦 Características Implementadas

### ✅ Entity Framework Core
- **DbContext Configuration**: Configuración completa con Oracle
- **Entity Relationships**: Relaciones uno-a-muchos entre Posts y Comments
- **Fluent API**: Configuración avanzada de entidades
- **Change Tracking**: Seguimiento automático de cambios
- **Lazy/Eager Loading**: Carga diferida y explícita de datos relacionados

### ✅ Repository Pattern
- **Interfaces Bien Definidas**: Contratos claros para acceso a datos
- **Implementaciones Concretas**: Lógica de acceso encapsulada
- **Dependency Injection**: Inyección automática de dependencias
- **Testabilidad**: Fácil creación de mocks para testing

### ✅ Web API Controllers
- **PostsController**: CRUD completo con operaciones avanzadas
- **CommentsController**: Gestión de comentarios con moderación
- **Documentación Swagger**: Documentación automática de la API
- **Logging Estructurado**: Logging detallado de operaciones
- **Manejo de Errores**: Respuestas HTTP apropiadas

### ✅ Consultas Avanzadas
- **LINQ Queries**: Consultas tipadas y expresivas
- **Complex Joins**: Uniones complejas entre tablas
- **Aggregations**: Funciones de agregación (Count, Sum, Max, etc.)
- **Pagination**: Implementación de paginación eficiente
- **Raw SQL**: Consultas SQL directas para casos específicos

### ✅ Migraciones
- **Code-First**: Generación de esquema desde código
- **Version Control**: Control de versiones del esquema
- **Seed Data**: Datos iniciales para desarrollo y testing
- **Rollback Support**: Capacidad de revertir migraciones

## 🚀 Configuración y Uso

### Requisitos Previos

- .NET 8.0 SDK
- Oracle Database (21c o superior recomendado)
- Visual Studio 2022 o VS Code con extensión C#

### 1. Configuración de Base de Datos

Actualizar la cadena de conexión en `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=localhost:1521/XE;User Id=bloguser;Password=password123;Persist Security Info=True;"
  }
}
```

### 2. Ejecutar Migraciones

```bash
# Crear migración (si es necesario)
dotnet ef migrations add InitialCreate

# Aplicar migraciones a la base de datos
dotnet ef database update

# Ver historial de migraciones
dotnet ef migrations list
```

### 3. Ejecutar la Aplicación

```bash
# Ejecutar la aplicación
dotnet run

# Navegar a Swagger UI
# https://localhost:7001/swagger
```

### 4. Poblar con Datos de Ejemplo

```bash
# Usando el endpoint de seed
curl -X GET https://localhost:7001/seed-data
```

## 📚 Conceptos de Entity Framework Core Explicados

### 1. DbContext - El Corazón de EF Core

El `BlogDbContext` es el punto de entrada principal para interactuar con la base de datos:

```csharp
public class BlogDbContext : DbContext
{
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuración de relaciones
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Post)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.PostId);
    }
}
```

**Conceptos clave:**
- **DbSet<T>**: Representa una tabla en la base de datos
- **OnModelCreating**: Configuración avanzada de entidades
- **Change Tracking**: EF Core rastrea automáticamente los cambios

### 2. Migraciones - Evolución del Esquema

Las migraciones permiten versionar y evolucionar el esquema de la base de datos:

```csharp
// Archivo de migración generado automáticamente
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Posts",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                Title = table.Column<string>(maxLength: 200, nullable: false),
                Content = table.Column<string>(nullable: false),
                // ... más columnas
            });
    }
}
```

**Ventajas de las migraciones:**
- **Versionado**: Cada cambio tiene una versión específica
- **Colaboración**: Los cambios se pueden compartir entre desarrolladores
- **Deployment**: Aplicación automática en diferentes entornos
- **Rollback**: Posibilidad de revertir cambios

### 3. Repository Pattern - Abstracción de Datos

El patrón Repository proporciona una abstracción sobre la capa de acceso a datos:

```csharp
// Interfaz que define el contrato
public interface IPostRepository
{
    Task<IEnumerable<Post>> GetAllAsync();
    Task<Post?> GetByIdAsync(int id);
    Task<Post> CreateAsync(Post post);
    // ... más métodos
}

// Implementación concreta
public class PostRepository : IPostRepository
{
    private readonly BlogDbContext _context;
    
    public async Task<IEnumerable<Post>> GetAllAsync()
    {
        return await _context.Posts
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
}
```

**Beneficios del Repository Pattern:**
- **Testabilidad**: Fácil creación de mocks
- **Separación de Responsabilidades**: Lógica de datos separada
- **Flexibilidad**: Cambio de implementación sin afectar controladores
- **Reutilización**: Lógica común compartida

## 🔍 Ejemplos de Consultas Complejas

### 1. Consultas LINQ Avanzadas

```csharp
// Búsqueda con múltiples condiciones
public async Task<IEnumerable<Post>> GetPostsWithCriteria(string author, bool? isPublished, string tag)
{
    var query = _context.Posts.AsQueryable();
    
    if (!string.IsNullOrEmpty(author))
        query = query.Where(p => p.Author.Contains(author));
        
    if (isPublished.HasValue)
        query = query.Where(p => p.IsPublished == isPublished.Value);
        
    if (!string.IsNullOrEmpty(tag))
        query = query.Where(p => p.Tags != null && p.Tags.Contains(tag));
    
    return await query
        .OrderByDescending(p => p.CreatedAt)
        .ToListAsync();
}

// Agregaciones complejas
public async Task<IEnumerable<dynamic>> GetCommentStatsByPost()
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
            AverageLength = g.Average(c => c.Content.Length)
        })
        .OrderByDescending(x => x.TotalComments)
        .ToListAsync();
}
```

### 2. SQL Directo con Oracle.ManagedDataAccess.Core

```csharp
// Consulta compleja con JOIN y funciones de Oracle
public async Task<IEnumerable<dynamic>> GetPostStatsByAuthor()
{
    var sql = @"
        SELECT 
            AUTHOR,
            COUNT(*) as TOTAL_POSTS,
            COUNT(CASE WHEN IS_PUBLISHED = 1 THEN 1 END) as PUBLISHED_POSTS,
            ROUND(AVG(LENGTH(CONTENT)), 2) as AVG_CONTENT_LENGTH,
            MIN(CREATED_AT) as FIRST_POST_DATE,
            MAX(CREATED_AT) as LAST_POST_DATE,
            EXTRACT(DAY FROM (MAX(CREATED_AT) - MIN(CREATED_AT))) as DAYS_ACTIVE
        FROM POSTS 
        GROUP BY AUTHOR 
        HAVING COUNT(*) > 1
        ORDER BY TOTAL_POSTS DESC";

    using var connection = new OracleConnection(connectionString);
    await connection.OpenAsync();
    
    using var command = new OracleCommand(sql, connection);
    using var reader = await command.ExecuteReaderAsync();
    
    var results = new List<dynamic>();
    while (await reader.ReadAsync())
    {
        results.Add(new
        {
            Author = reader.GetString("AUTHOR"),
            TotalPosts = reader.GetInt32("TOTAL_POSTS"),
            PublishedPosts = reader.GetInt32("PUBLISHED_POSTS"),
            AvgContentLength = reader.GetDecimal("AVG_CONTENT_LENGTH"),
            FirstPostDate = reader.GetDateTime("FIRST_POST_DATE"),
            LastPostDate = reader.GetDateTime("LAST_POST_DATE"),
            DaysActive = reader.GetInt32("DAYS_ACTIVE")
        });
    }
    
    return results;
}
```

### 3. Transacciones y Operaciones Atómicas

```csharp
// Operación compleja con transacción
public async Task<bool> CreatePostWithComments(Post post, IEnumerable<Comment> comments)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        // 1. Crear el post
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        
        // 2. Crear los comentarios asociados
        foreach (var comment in comments)
        {
            comment.PostId = post.Id;
            _context.Comments.Add(comment);
        }
        await _context.SaveChangesAsync();
        
        // 3. Actualizar estadísticas (ejemplo)
        await UpdatePostStatistics(post.Id);
        
        await transaction.CommitAsync();
        return true;
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

## 🌐 API Endpoints Documentados

### Posts Controller

| Método | Endpoint | Descripción | Ejemplo |
|--------|----------|-------------|---------|
| GET | `/api/posts` | Obtener posts publicados | `curl -X GET /api/posts` |
| GET | `/api/posts/{id}` | Obtener post específico | `curl -X GET /api/posts/1` |
| POST | `/api/posts` | Crear nuevo post | `curl -X POST /api/posts -d @post.json` |
| PUT | `/api/posts/{id}` | Actualizar post | `curl -X PUT /api/posts/1 -d @post.json` |
| DELETE | `/api/posts/{id}` | Eliminar post | `curl -X DELETE /api/posts/1` |
| POST | `/api/posts/{id}/publish` | Publicar post | `curl -X POST /api/posts/1/publish` |
| POST | `/api/posts/{id}/unpublish` | Despublicar post | `curl -X POST /api/posts/1/unpublish` |
| GET | `/api/posts/search?title=entity` | Buscar por título | `curl -X GET /api/posts/search?title=entity` |
| GET | `/api/posts/author/{author}` | Posts por autor | `curl -X GET /api/posts/author/john` |
| GET | `/api/posts/tag/{tag}` | Posts por tag | `curl -X GET /api/posts/tag/dotnet` |
| GET | `/api/posts/paged?pageNumber=1&pageSize=10` | Posts paginados | `curl -X GET /api/posts/paged?pageNumber=1&pageSize=10` |
| GET | `/api/posts/stats` | Estadísticas generales | `curl -X GET /api/posts/stats` |

### Comments Controller

| Método | Endpoint | Descripción | Ejemplo |
|--------|----------|-------------|---------|
| GET | `/api/comments` | Comentarios aprobados | `curl -X GET /api/comments` |
| GET | `/api/comments/{id}` | Comentario específico | `curl -X GET /api/comments/1` |
| GET | `/api/comments/post/{postId}` | Comentarios de un post | `curl -X GET /api/comments/post/1` |
| POST | `/api/comments` | Crear comentario | `curl -X POST /api/comments -d @comment.json` |
| PUT | `/api/comments/{id}` | Actualizar comentario | `curl -X PUT /api/comments/1 -d @comment.json` |
| DELETE | `/api/comments/{id}` | Eliminar comentario | `curl -X DELETE /api/comments/1` |
| POST | `/api/comments/{id}/approve` | Aprobar comentario | `curl -X POST /api/comments/1/approve` |
| POST | `/api/comments/{id}/reject` | Rechazar comentario | `curl -X POST /api/comments/1/reject` |
| GET | `/api/comments/pending` | Comentarios pendientes | `curl -X GET /api/comments/pending` |
| GET | `/api/comments/author/{email}` | Comentarios por autor | `curl -X GET /api/comments/author/user@example.com` |
| GET | `/api/comments/stats` | Estadísticas | `curl -X GET /api/comments/stats` |
| GET | `/api/comments/stats/by-post` | Estadísticas por post | `curl -X GET /api/comments/stats/by-post` |

### Ejemplos de Payloads JSON

#### Crear Post
```json
{
  "title": "Mi Primer Post sobre Entity Framework",
  "content": "Este es el contenido completo del post...",
  "summary": "Un resumen del post",
  "author": "Juan Pérez",
  "tags": "entity-framework,oracle,dotnet",
  "isPublished": false
}
```

#### Crear Comentario
```json
{
  "content": "Excelente explicación sobre Entity Framework!",
  "authorName": "María García",
  "authorEmail": "maria@example.com",
  "postId": 1
}
```

## 🧪 Optimización y Mejores Prácticas

### 1. Optimización de Consultas

```csharp
// ❌ Problema N+1
public async Task<IEnumerable<Post>> GetPostsWithComments_Bad()
{
    var posts = await _context.Posts.ToListAsync();
    foreach (var post in posts)
    {
        // Esto genera una consulta por cada post
        post.Comments = await _context.Comments
            .Where(c => c.PostId == post.Id)
            .ToListAsync();
    }
    return posts;
}

// ✅ Solución con Include
public async Task<IEnumerable<Post>> GetPostsWithComments_Good()
{
    return await _context.Posts
        .Include(p => p.Comments.Where(c => c.IsApproved))
        .ToListAsync();
}

// ✅ Proyección para mejor rendimiento
public async Task<IEnumerable<object>> GetPostsSummary()
{
    return await _context.Posts
        .Select(p => new
        {
            p.Id,
            p.Title,
            p.Author,
            p.CreatedAt,
            CommentsCount = p.Comments.Count(c => c.IsApproved)
        })
        .ToListAsync();
}
```

### 2. Configuración de Performance

```csharp
// En Program.cs
builder.Services.AddDbContext<BlogDbContext>(options =>
{
    options.UseOracle(connectionString, oracleOptions =>
    {
        oracleOptions.CommandTimeout(30);
        oracleOptions.UseOracleSQLCompatibility("11");
    });
    
    // Configuraciones de desarrollo
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
        options.LogTo(Console.WriteLine, LogLevel.Information);
    }
    
    // Configuraciones de producción
    else
    {
        options.EnableServiceProviderCaching();
        options.EnableSensitiveDataLogging(false);
    }
});
```

### 3. Manejo de Conexiones

```csharp
// Configuración de connection pooling
builder.Services.AddDbContextPool<BlogDbContext>(options =>
    options.UseOracle(connectionString), 
    poolSize: 128);

// Configuración de retry policy
builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseOracle(connectionString, oracleOptions =>
        oracleOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null)));
```

## 📊 Monitoreo y Logging

### Logging de Consultas EF Core

```csharp
// En appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information",
      "Microsoft.EntityFrameworkCore.Infrastructure": "Warning"
    }
  }
}
```

### Métricas de Performance

```csharp
// Ejemplo de logging personalizado en controlador
[HttpGet("{id:int}")]
public async Task<ActionResult<object>> GetPost(int id)
{
    _logger.LogInformation("Obteniendo post con ID: {PostId}", id);
    
    var stopwatch = Stopwatch.StartNew();
    var post = await _postRepository.GetByIdWithCommentsAsync(id);
    stopwatch.Stop();
    
    if (post == null)
    {
        _logger.LogWarning("Post con ID {PostId} no encontrado", id);
        return NotFound($"Post con ID {id} no encontrado");
    }

    _logger.LogInformation("Post {PostId} encontrado en {ElapsedMs}ms con {CommentsCount} comentarios", 
        id, stopwatch.ElapsedMilliseconds, post.Comments.Count);
    
    return Ok(result);
}
```

## 🔧 Troubleshooting Común

### Problemas de Conexión Oracle

```bash
# Verificar conectividad
tnsping localhost:1521

# Verificar usuario y permisos
sqlplus bloguser/password123@localhost:1521/XE
```

### Problemas de Migraciones

```bash
# Revertir última migración
dotnet ef database update PreviousMigrationName

# Eliminar migración no aplicada
dotnet ef migrations remove

# Generar script SQL
dotnet ef migrations script
```

### Problemas de Performance

```csharp
// Habilitar logging detallado temporalmente
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information)
                  .EnableSensitiveDataLogging()
                  .EnableDetailedErrors();
}
```

## 📁 Estructura del Proyecto

```
BlogApi/
├── Controllers/              # Controladores Web API
│   ├── PostsController.cs   # CRUD completo para posts
│   └── CommentsController.cs # CRUD completo para comentarios
├── Data/                    # Configuración de datos
│   ├── BlogDbContext.cs     # Contexto principal de EF Core
│   └── BlogDbContextSeed.cs # Datos de ejemplo
├── Models/                  # Entidades del dominio
│   ├── Post.cs             # Entidad Post con anotaciones
│   └── Comment.cs          # Entidad Comment con relaciones
├── Repositories/            # Patrón Repository
│   ├── IPostRepository.cs   # Interfaz para posts
│   ├── PostRepository.cs    # Implementación con EF Core y SQL directo
│   ├── ICommentRepository.cs # Interfaz para comentarios
│   └── CommentRepository.cs # Implementación con LINQ avanzado
├── Migrations/              # Migraciones de EF Core
│   ├── 20240814184910_InitialCreate.cs
│   └── BlogDbContextModelSnapshot.cs
├── Program.cs              # Configuración de la aplicación
├── BlogApi.csproj          # Configuración del proyecto
└── README.md              # Esta documentación
```

## 🎓 Ejercicios Prácticos

### Ejercicio 1: Consulta Compleja
Implementar un endpoint que retorne posts con:
- Número de comentarios aprobados
- Número de comentarios pendientes
- Fecha del último comentario
- Promedio de longitud de comentarios

### Ejercicio 2: Optimización
Identificar y optimizar consultas N+1 en el código existente.

### Ejercicio 3: Transacciones
Implementar una operación que:
- Cree un post
- Cree múltiples comentarios
- Actualice estadísticas
- Todo en una transacción atómica

### Ejercicio 4: Migración Avanzada
Crear una migración que:
- Agregue una tabla de categorías
- Relacione posts con categorías
- Migre datos existentes

## 📖 Recursos Adicionales

- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [Oracle Entity Framework Core Provider](https://docs.oracle.com/en/database/oracle/oracle-database/21/odpnt/EntityFrameworkCore.html)
- [Repository Pattern in .NET](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- [LINQ Query Syntax](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/query-syntax-and-method-syntax-in-linq)

---

Este proyecto demuestra un uso completo y profesional de Entity Framework Core con Oracle, mostrando desde conceptos básicos hasta técnicas avanzadas de optimización y consultas complejas, implementado a través de controladores Web API completamente funcionales.