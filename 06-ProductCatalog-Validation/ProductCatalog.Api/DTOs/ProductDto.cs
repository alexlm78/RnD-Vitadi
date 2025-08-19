namespace ProductCatalog.Api.DTOs;

/// <summary>
/// DTO for product response
/// </summary>
public class ProductResponseDto
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
    public bool IsActive { get; set; }

    /// <summary>
    /// Foreign key to the category
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Name of the category this product belongs to
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Category information
    /// </summary>
    public CategoryResponseDto? Category { get; set; }

    /// <summary>
    /// Date when the product was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date when the product was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating a new product
/// </summary>
public class CreateProductDto
{
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
}

/// <summary>
/// DTO for updating an existing product
/// </summary>
public class UpdateProductDto
{
    /// <summary>
    /// Name of the product
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Description of the product
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// SKU (Stock Keeping Unit) for the product
    /// </summary>
    public string? Sku { get; set; }

    /// <summary>
    /// Price of the product
    /// </summary>
    public decimal? Price { get; set; }

    /// <summary>
    /// Quantity available in stock
    /// </summary>
    public int? StockQuantity { get; set; }

    /// <summary>
    /// Indicates if the product is active
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Foreign key to the category
    /// </summary>
    public int? CategoryId { get; set; }
}

/// <summary>
/// DTO for product summary (lightweight version)
/// </summary>
public class ProductSummaryDto
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
    /// Calculated total value (Price × StockQuantity)
    /// </summary>
    public decimal TotalValue { get; set; }

    /// <summary>
    /// Stock status indicator
    /// </summary>
    public string StockStatus { get; set; } = string.Empty;
}

/// <summary>
/// DTO for product search request with advanced filtering
/// </summary>
public class ProductSearchRequestDto
{
    /// <summary>
    /// Product name to search for (partial match)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Category ID to filter by
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Minimum price filter
    /// </summary>
    public decimal? MinPrice { get; set; }

    /// <summary>
    /// Maximum price filter
    /// </summary>
    public decimal? MaxPrice { get; set; }

    /// <summary>
    /// Filter by stock availability
    /// </summary>
    public bool? InStock { get; set; }

    /// <summary>
    /// Page number for pagination (default: 1)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Page size for pagination (default: 10, max: 100)
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Whether to include category information in response
    /// </summary>
    public bool IncludeCategory { get; set; } = false;
}

/// <summary>
/// DTO for product search results
/// </summary>
public class ProductSearchResultDto
{
    /// <summary>
    /// List of products (type depends on IncludeCategory flag)
    /// </summary>
    public object Products { get; set; } = new List<object>();

    /// <summary>
    /// Total number of products matching the criteria
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }
}

/// <summary>
/// DTO for bulk create operation results
/// </summary>
public class BulkCreateResultDto
{
    /// <summary>
    /// Total number of products requested to be created
    /// </summary>
    public int TotalRequested { get; set; }

    /// <summary>
    /// Number of products successfully created
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Number of products that failed to be created
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// List of successfully created products
    /// </summary>
    public List<ProductResponseDto> SuccessfullyCreated { get; set; } = new();

    /// <summary>
    /// List of errors that occurred during creation
    /// </summary>
    public List<BulkCreateErrorDto> Errors { get; set; } = new();
}

/// <summary>
/// DTO for bulk create operation errors
/// </summary>
public class BulkCreateErrorDto
{
    /// <summary>
    /// Index of the product in the original request array
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// SKU of the product that failed
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// Error message describing what went wrong
    /// </summary>
    public string Error { get; set; } = string.Empty;
}