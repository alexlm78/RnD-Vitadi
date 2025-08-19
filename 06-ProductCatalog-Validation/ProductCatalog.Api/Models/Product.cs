namespace ProductCatalog.Api.Models;

/// <summary>
/// Represents a product in the catalog
/// </summary>
public class Product
{
    /// <summary>
    /// Unique identifier for the product
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the product
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the product
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// SKU (Stock Keeping Unit) for the product
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// Price of the product
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Quantity available in stock
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Indicates if the product is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Foreign key to the category
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Date when the product was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the product was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property to the category
    /// </summary>
    public virtual Category Category { get; set; } = null!;
}