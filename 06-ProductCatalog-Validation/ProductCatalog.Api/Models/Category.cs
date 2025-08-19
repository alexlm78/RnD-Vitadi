namespace ProductCatalog.Api.Models;

/// <summary>
/// Represents a product category in the catalog
/// </summary>
public class Category
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
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Date when the category was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the category was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property for products in this category
    /// </summary>
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}