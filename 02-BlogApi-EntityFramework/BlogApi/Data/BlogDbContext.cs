using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Data;

/// <summary>
/// Contexto de Entity Framework para la aplicación de Blog
/// Configura las entidades y sus relaciones usando Fluent API
/// </summary>
public class BlogDbContext : DbContext
{
    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// DbSet para la entidad Post
    /// </summary>
    public DbSet<Post> Posts { get; set; }

    /// <summary>
    /// DbSet para la entidad Comment
    /// </summary>
    public DbSet<Comment> Comments { get; set; }

    /// <summary>
    /// Configuración de entidades usando Fluent API
    /// Este método se ejecuta cuando se crea el modelo de datos
    /// </summary>
    /// <param name="modelBuilder">Constructor del modelo de EF Core</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de la entidad Post
        ConfigurePostEntity(modelBuilder);

        // Configuración de la entidad Comment
        ConfigureCommentEntity(modelBuilder);

        // Configuración de relaciones
        ConfigureRelationships(modelBuilder);
    }

    /// <summary>
    /// Configuración específica para la entidad Post usando Fluent API
    /// </summary>
    /// <param name="modelBuilder">Constructor del modelo</param>
    private static void ConfigurePostEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>(entity =>
        {
            // Configuración de tabla
            entity.ToTable("POSTS");

            // Configuración de clave primaria
            entity.HasKey(p => p.Id);

            // Configuración de propiedades
            entity.Property(p => p.Id)
                .HasColumnName("ID")
                .ValueGeneratedOnAdd(); // Auto-increment

            entity.Property(p => p.Title)
                .HasColumnName("TITLE")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(p => p.Content)
                .HasColumnName("CONTENT")
                .HasColumnType("CLOB") // Oracle CLOB para texto largo
                .IsRequired();

            entity.Property(p => p.Summary)
                .HasColumnName("SUMMARY")
                .HasMaxLength(500)
                .IsRequired(false);

            entity.Property(p => p.Author)
                .HasColumnName("AUTHOR")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(p => p.CreatedAt)
                .HasColumnName("CREATED_AT")
                .HasColumnType("TIMESTAMP")
                .IsRequired();

            entity.Property(p => p.UpdatedAt)
                .HasColumnName("UPDATED_AT")
                .HasColumnType("TIMESTAMP")
                .IsRequired();

            entity.Property(p => p.IsPublished)
                .HasColumnName("IS_PUBLISHED")
                .HasColumnType("NUMBER(1)")
                .IsRequired();

            entity.Property(p => p.PublishedAt)
                .HasColumnName("PUBLISHED_AT")
                .HasColumnType("TIMESTAMP")
                .IsRequired(false);

            entity.Property(p => p.Tags)
                .HasColumnName("TAGS")
                .HasMaxLength(500)
                .IsRequired(false);

            // Índices para mejorar performance
            entity.HasIndex(p => p.Title)
                .HasDatabaseName("IX_POSTS_TITLE");

            entity.HasIndex(p => p.CreatedAt)
                .HasDatabaseName("IX_POSTS_CREATED_AT");

            entity.HasIndex(p => p.IsPublished)
                .HasDatabaseName("IX_POSTS_IS_PUBLISHED");
        });
    }

    /// <summary>
    /// Configuración específica para la entidad Comment usando Fluent API
    /// </summary>
    /// <param name="modelBuilder">Constructor del modelo</param>
    private static void ConfigureCommentEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>(entity =>
        {
            // Configuración de tabla
            entity.ToTable("COMMENTS");

            // Configuración de clave primaria
            entity.HasKey(c => c.Id);

            // Configuración de propiedades
            entity.Property(c => c.Id)
                .HasColumnName("ID")
                .ValueGeneratedOnAdd(); // Auto-increment

            entity.Property(c => c.Content)
                .HasColumnName("CONTENT")
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(c => c.AuthorName)
                .HasColumnName("AUTHOR_NAME")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(c => c.AuthorEmail)
                .HasColumnName("AUTHOR_EMAIL")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(c => c.CreatedAt)
                .HasColumnName("CREATED_AT")
                .HasColumnType("TIMESTAMP")
                .IsRequired();

            entity.Property(c => c.IsApproved)
                .HasColumnName("IS_APPROVED")
                .HasColumnType("NUMBER(1)")
                .IsRequired();

            entity.Property(c => c.PostId)
                .HasColumnName("POST_ID")
                .IsRequired();

            // Índices para mejorar performance
            entity.HasIndex(c => c.PostId)
                .HasDatabaseName("IX_COMMENTS_POST_ID");

            entity.HasIndex(c => c.CreatedAt)
                .HasDatabaseName("IX_COMMENTS_CREATED_AT");

            entity.HasIndex(c => c.IsApproved)
                .HasDatabaseName("IX_COMMENTS_IS_APPROVED");
        });
    }

    /// <summary>
    /// Configuración de relaciones entre entidades
    /// </summary>
    /// <param name="modelBuilder">Constructor del modelo</param>
    private static void ConfigureRelationships(ModelBuilder modelBuilder)
    {
        // Relación uno a muchos: Post -> Comments
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Post)           // Un comentario tiene un post
            .WithMany(p => p.Comments)     // Un post tiene muchos comentarios
            .HasForeignKey(c => c.PostId)  // Clave foránea
            .HasConstraintName("FK_COMMENTS_POST_ID")
            .OnDelete(DeleteBehavior.Cascade); // Si se elimina un post, se eliminan sus comentarios
    }

    /// <summary>
    /// Configuración adicional que se ejecuta antes de guardar cambios
    /// Actualiza automáticamente las fechas de modificación
    /// </summary>
    /// <returns>Número de entidades afectadas</returns>
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// Versión asíncrona de SaveChanges con actualización automática de timestamps
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número de entidades afectadas</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Actualiza automáticamente las fechas de creación y modificación
    /// </summary>
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Post post)
            {
                if (entry.State == EntityState.Added)
                {
                    post.CreatedAt = DateTime.UtcNow;
                }
                post.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is Comment comment && entry.State == EntityState.Added)
            {
                comment.CreatedAt = DateTime.UtcNow;
            }
        }
    }
}