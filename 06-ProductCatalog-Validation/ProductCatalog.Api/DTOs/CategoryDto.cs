namespace ProductCatalog.Api.DTOs;

/// <summary>
/// DTO for category response
/// </summary>
public class CategoryResponseDto
{
    /// <summary>
    /// Unique identifier for the category
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the category
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the category
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the category is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Date when the category was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date when the category was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Number of products in this category
    /// </summary>
    public int ProductCount { get; set; }
}

/// <summary>
/// DTO for creating a new category
/// </summary>
public class CreateCategoryDto
{
    /// <summary>
    /// Name of the category
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the category
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the category is active
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO for updating an existing category
/// </summary>
public class UpdateCategoryDto
{
    /// <summary>
    /// Name of the category
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Description of the category
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Indicates if the category is active
    /// </summary>
    public bool? IsActive { get; set; }
}