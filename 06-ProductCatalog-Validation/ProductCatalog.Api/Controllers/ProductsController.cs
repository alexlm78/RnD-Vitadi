using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Api.DTOs;
using ProductCatalog.Api.Models;

namespace ProductCatalog.Api.Controllers;

/// <summary>
/// Controller for managing products
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly ILogger<ProductsController> _logger;

    // In-memory storage for demonstration purposes
    // In a real application, this would be replaced with a repository/database
    private static readonly List<Product> _products = new()
    {
        new Product 
        { 
            Id = 1, 
            Name = "iPhone 15", 
            Description = "Latest Apple smartphone", 
            Sku = "IPHONE-15-128GB", 
            Price = 999.99m, 
            StockQuantity = 50, 
            CategoryId = 1, 
            IsActive = true,
            Category = new Category { Id = 1, Name = "Electronics" }
        },
        new Product 
        { 
            Id = 2, 
            Name = "Cotton T-Shirt", 
            Description = "Comfortable cotton t-shirt", 
            Sku = "TSHIRT-COTTON-M", 
            Price = 29.99m, 
            StockQuantity = 100, 
            CategoryId = 2, 
            IsActive = true,
            Category = new Category { Id = 2, Name = "Clothing" }
        }
    };

    public ProductsController(IMapper mapper, ILogger<ProductsController> logger)
    {
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Get all products
    /// </summary>
    /// <param name="categoryId">Optional category filter</param>
    /// <returns>List of products</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductResponseDto>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<ProductResponseDto>> GetProducts([FromQuery] int? categoryId = null)
    {
        _logger.LogInformation("Getting products with category filter: {CategoryId}", categoryId);
        
        var products = _products.Where(p => p.IsActive);
        
        if (categoryId.HasValue)
        {
            products = products.Where(p => p.CategoryId == categoryId.Value);
        }

        var productDtos = _mapper.Map<IEnumerable<ProductResponseDto>>(products.ToList());
        
        return Ok(productDtos);
    }

    /// <summary>
    /// Get a specific product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ProductResponseDto> GetProduct(int id)
    {
        _logger.LogInformation("Getting product with ID: {ProductId}", id);
        
        var product = _products.FirstOrDefault(p => p.Id == id && p.IsActive);
        if (product == null)
        {
            _logger.LogWarning("Product with ID {ProductId} not found", id);
            return NotFound($"Product with ID {id} not found");
        }

        var productDto = _mapper.Map<ProductResponseDto>(product);
        return Ok(productDto);
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="createProductDto">Product creation data</param>
    /// <returns>Created product</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/products
    ///     {
    ///         "name": "Wireless Headphones",
    ///         "description": "High-quality wireless headphones with noise cancellation",
    ///         "sku": "WH-NOISE-001",
    ///         "price": 199.99,
    ///         "stockQuantity": 25,
    ///         "categoryId": 1,
    ///         "isActive": true
    ///     }
    /// 
    /// FluentValidation automatically validates:
    /// - Name: Required, 2-200 chars, no special characters except spaces/hyphens/apostrophes
    /// - SKU: Required, uppercase letters/numbers/hyphens/underscores, format ABC123
    /// - Price: Greater than 0, less than 1,000,000, max 2 decimal places
    /// - Stock: Greater than or equal to 0, less than 100,000
    /// - Conditional: Active products must have stock greater than 0
    /// - Cross-field: High-value products (greater than $1000) should have stock 1-50
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<ProductResponseDto> CreateProduct([FromBody] CreateProductDto createProductDto)
    {
        _logger.LogInformation("Creating new product: {ProductName}", createProductDto.Name);

        // Additional business validation beyond FluentValidation
        // FluentValidation handles input validation, controller handles business rules
        
        // Check if SKU already exists (business rule validation)
        if (_products.Any(p => p.Sku.Equals(createProductDto.Sku, StringComparison.OrdinalIgnoreCase) && p.IsActive))
        {
            _logger.LogWarning("Attempt to create product with duplicate SKU: {Sku}", createProductDto.Sku);
            return BadRequest(new { error = "A product with this SKU already exists", field = "sku" });
        }

        // Validate category exists (business rule validation)
        var categoryExists = createProductDto.CategoryId > 0 && createProductDto.CategoryId <= 3; // Mock validation
        if (!categoryExists)
        {
            _logger.LogWarning("Attempt to create product with invalid category: {CategoryId}", createProductDto.CategoryId);
            return BadRequest(new { error = "Invalid category ID", field = "categoryId" });
        }

        // AutoMapper automatically maps DTO to Entity
        var product = _mapper.Map<Product>(createProductDto);
        product.Id = _products.Count > 0 ? _products.Max(p => p.Id) + 1 : 1;
        
        // Set category for mapping (in real app, this would be loaded from database)
        product.Category = new Category 
        { 
            Id = createProductDto.CategoryId, 
            Name = GetCategoryName(createProductDto.CategoryId) 
        };
        
        _products.Add(product);

        // AutoMapper automatically maps Entity to Response DTO
        var productDto = _mapper.Map<ProductResponseDto>(product);
        
        _logger.LogInformation("Product created successfully with ID: {ProductId}", product.Id);
        
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productDto);
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="updateProductDto">Product update data</param>
    /// <returns>Updated product</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ProductResponseDto> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
    {
        _logger.LogInformation("Updating product with ID: {ProductId}", id);

        var product = _products.FirstOrDefault(p => p.Id == id && p.IsActive);
        if (product == null)
        {
            _logger.LogWarning("Product with ID {ProductId} not found for update", id);
            return NotFound($"Product with ID {id} not found");
        }

        // Check if new SKU conflicts with existing products (excluding current one)
        if (updateProductDto.Sku != null && _products.Any(p => p.Id != id && p.Sku.Equals(updateProductDto.Sku, StringComparison.OrdinalIgnoreCase) && p.IsActive))
        {
            return BadRequest("A product with this SKU already exists");
        }

        // Validate category exists
        if (updateProductDto.CategoryId.HasValue)
        {
            var categoryExists = updateProductDto.CategoryId > 0 && updateProductDto.CategoryId <= 3; // Mock validation
            if (!categoryExists)
            {
                return BadRequest("Invalid category ID");
            }
        }

        _mapper.Map(updateProductDto, product);
        
        // Update category for mapping if CategoryId was provided
        if (updateProductDto.CategoryId.HasValue)
        {
            product.Category = new Category 
            { 
                Id = updateProductDto.CategoryId.Value, 
                Name = GetCategoryName(updateProductDto.CategoryId.Value) 
            };
        }

        var productDto = _mapper.Map<ProductResponseDto>(product);
        
        _logger.LogInformation("Product with ID {ProductId} updated successfully", id);
        
        return Ok(productDto);
    }

    /// <summary>
    /// Delete a product (soft delete)
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteProduct(int id)
    {
        _logger.LogInformation("Deleting product with ID: {ProductId}", id);

        var product = _products.FirstOrDefault(p => p.Id == id && p.IsActive);
        if (product == null)
        {
            _logger.LogWarning("Product with ID {ProductId} not found for deletion", id);
            return NotFound($"Product with ID {id} not found");
        }

        // Soft delete
        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation("Product with ID {ProductId} deleted successfully", id);
        
        return NoContent();
    }

    /// <summary>
    /// Bulk create products with advanced validation
    /// </summary>
    /// <param name="createProductDtos">List of products to create</param>
    /// <returns>Results of bulk creation</returns>
    /// <remarks>
    /// This endpoint demonstrates:
    /// - Bulk validation with FluentValidation
    /// - Custom validation logic for bulk operations
    /// - Partial success handling
    /// - Advanced error reporting
    /// 
    /// Sample request:
    /// 
    ///     POST /api/products/bulk
    ///     [
    ///         {
    ///             "name": "Product 1",
    ///             "sku": "PROD001",
    ///             "price": 99.99,
    ///             "stockQuantity": 10,
    ///             "categoryId": 1
    ///         },
    ///         {
    ///             "name": "Product 2", 
    ///             "sku": "PROD002",
    ///             "price": 149.99,
    ///             "stockQuantity": 5,
    ///             "categoryId": 2
    ///         }
    ///     ]
    /// </remarks>
    [HttpPost("bulk")]
    [ProducesResponseType(typeof(BulkCreateResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<BulkCreateResultDto> BulkCreateProducts([FromBody] List<CreateProductDto> createProductDtos)
    {
        _logger.LogInformation("Bulk creating {Count} products", createProductDtos.Count);

        if (createProductDtos.Count == 0)
        {
            return BadRequest("At least one product must be provided");
        }

        if (createProductDtos.Count > 100)
        {
            return BadRequest("Cannot create more than 100 products at once");
        }

        var result = new BulkCreateResultDto
        {
            TotalRequested = createProductDtos.Count,
            SuccessfullyCreated = new List<ProductResponseDto>(),
            Errors = new List<BulkCreateErrorDto>()
        };

        // Check for duplicate SKUs within the batch
        var duplicateSkus = createProductDtos
            .GroupBy(p => p.Sku.ToUpperInvariant())
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateSkus.Any())
        {
            return BadRequest($"Duplicate SKUs found in batch: {string.Join(", ", duplicateSkus)}");
        }

        for (int i = 0; i < createProductDtos.Count; i++)
        {
            var dto = createProductDtos[i];
            
            try
            {
                // Check if SKU already exists
                if (_products.Any(p => p.Sku.Equals(dto.Sku, StringComparison.OrdinalIgnoreCase) && p.IsActive))
                {
                    result.Errors.Add(new BulkCreateErrorDto
                    {
                        Index = i,
                        Sku = dto.Sku,
                        Error = "SKU already exists"
                    });
                    continue;
                }

                // Validate category exists
                var categoryExists = dto.CategoryId > 0 && dto.CategoryId <= 3;
                if (!categoryExists)
                {
                    result.Errors.Add(new BulkCreateErrorDto
                    {
                        Index = i,
                        Sku = dto.Sku,
                        Error = "Invalid category ID"
                    });
                    continue;
                }

                var product = _mapper.Map<Product>(dto);
                product.Id = _products.Count > 0 ? _products.Max(p => p.Id) + 1 : 1;
                product.Category = new Category 
                { 
                    Id = dto.CategoryId, 
                    Name = GetCategoryName(dto.CategoryId) 
                };
                
                _products.Add(product);
                
                var productDto = _mapper.Map<ProductResponseDto>(product);
                result.SuccessfullyCreated.Add(productDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product at index {Index}", i);
                result.Errors.Add(new BulkCreateErrorDto
                {
                    Index = i,
                    Sku = dto.Sku,
                    Error = "Internal error occurred"
                });
            }
        }

        result.SuccessCount = result.SuccessfullyCreated.Count;
        result.ErrorCount = result.Errors.Count;

        _logger.LogInformation("Bulk create completed: {Success} successful, {Errors} errors", 
            result.SuccessCount, result.ErrorCount);

        return Ok(result);
    }

    /// <summary>
    /// Search products with advanced filtering and mapping
    /// </summary>
    /// <param name="searchRequest">Search criteria</param>
    /// <returns>Filtered products</returns>
    /// <remarks>
    /// This endpoint demonstrates:
    /// - Complex DTO with nested validation
    /// - Conditional mapping based on request parameters
    /// - Advanced filtering logic
    /// - Custom response formatting
    /// </remarks>
    [HttpPost("search")]
    [ProducesResponseType(typeof(ProductSearchResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<ProductSearchResultDto> SearchProducts([FromBody] ProductSearchRequestDto searchRequest)
    {
        _logger.LogInformation("Searching products with criteria: {@SearchRequest}", searchRequest);

        var query = _products.Where(p => p.IsActive).AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(searchRequest.Name))
        {
            query = query.Where(p => p.Name.Contains(searchRequest.Name, StringComparison.OrdinalIgnoreCase));
        }

        if (searchRequest.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == searchRequest.CategoryId.Value);
        }

        if (searchRequest.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= searchRequest.MinPrice.Value);
        }

        if (searchRequest.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= searchRequest.MaxPrice.Value);
        }

        if (searchRequest.InStock.HasValue)
        {
            if (searchRequest.InStock.Value)
            {
                query = query.Where(p => p.StockQuantity > 0);
            }
            else
            {
                query = query.Where(p => p.StockQuantity == 0);
            }
        }

        var totalCount = query.Count();
        
        // Apply pagination
        var skip = (searchRequest.Page - 1) * searchRequest.PageSize;
        var products = query.Skip(skip).Take(searchRequest.PageSize).ToList();

        // Conditional mapping based on request
        object productDtos = searchRequest.IncludeCategory 
            ? _mapper.Map<List<ProductResponseDto>>(products)
            : _mapper.Map<List<ProductSummaryDto>>(products);

        var result = new ProductSearchResultDto
        {
            Products = productDtos,
            TotalCount = totalCount,
            Page = searchRequest.Page,
            PageSize = searchRequest.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / searchRequest.PageSize)
        };

        return Ok(result);
    }

    /// <summary>
    /// Helper method to get category name by ID (mock implementation)
    /// </summary>
    private static string GetCategoryName(int categoryId) => categoryId switch
    {
        1 => "Electronics",
        2 => "Clothing", 
        3 => "Books",
        _ => "Unknown"
    };
}