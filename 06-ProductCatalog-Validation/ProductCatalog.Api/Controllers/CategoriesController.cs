using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Api.DTOs;
using ProductCatalog.Api.Models;

namespace ProductCatalog.Api.Controllers;

/// <summary>
/// Controller for managing product categories
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly ILogger<CategoriesController> _logger;

    // In-memory storage for demonstration purposes
    // In a real application, this would be replaced with a repository/database
    private static readonly List<Category> _categories = new()
    {
        new Category { Id = 1, Name = "Electronics", Description = "Electronic devices and accessories", IsActive = true },
        new Category { Id = 2, Name = "Clothing", Description = "Apparel and fashion items", IsActive = true },
        new Category { Id = 3, Name = "Books", Description = "Books and educational materials", IsActive = true }
    };

    public CategoriesController(IMapper mapper, ILogger<CategoriesController> logger)
    {
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    /// <returns>List of categories</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryResponseDto>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<CategoryResponseDto>> GetCategories()
    {
        _logger.LogInformation("Getting all categories");
        
        var categories = _categories.Where(c => c.IsActive).ToList();
        var categoryDtos = _mapper.Map<IEnumerable<CategoryResponseDto>>(categories);
        
        return Ok(categoryDtos);
    }

    /// <summary>
    /// Get a specific category by ID
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Category details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<CategoryResponseDto> GetCategory(int id)
    {
        _logger.LogInformation("Getting category with ID: {CategoryId}", id);
        
        var category = _categories.FirstOrDefault(c => c.Id == id && c.IsActive);
        if (category == null)
        {
            _logger.LogWarning("Category with ID {CategoryId} not found", id);
            return NotFound($"Category with ID {id} not found");
        }

        var categoryDto = _mapper.Map<CategoryResponseDto>(category);
        return Ok(categoryDto);
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    /// <param name="createCategoryDto">Category creation data</param>
    /// <returns>Created category</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<CategoryResponseDto> CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
    {
        _logger.LogInformation("Creating new category: {CategoryName}", createCategoryDto.Name);

        // Check if category name already exists
        if (_categories.Any(c => c.Name.Equals(createCategoryDto.Name, StringComparison.OrdinalIgnoreCase) && c.IsActive))
        {
            return BadRequest("A category with this name already exists");
        }

        var category = _mapper.Map<Category>(createCategoryDto);
        category.Id = _categories.Count > 0 ? _categories.Max(c => c.Id) + 1 : 1;
        
        _categories.Add(category);

        var categoryDto = _mapper.Map<CategoryResponseDto>(category);
        
        _logger.LogInformation("Category created successfully with ID: {CategoryId}", category.Id);
        
        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, categoryDto);
    }

    /// <summary>
    /// Update an existing category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="updateCategoryDto">Category update data</param>
    /// <returns>Updated category</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<CategoryResponseDto> UpdateCategory(int id, [FromBody] UpdateCategoryDto updateCategoryDto)
    {
        _logger.LogInformation("Updating category with ID: {CategoryId}", id);

        var category = _categories.FirstOrDefault(c => c.Id == id && c.IsActive);
        if (category == null)
        {
            _logger.LogWarning("Category with ID {CategoryId} not found for update", id);
            return NotFound($"Category with ID {id} not found");
        }

        // Check if new name conflicts with existing categories (excluding current one)
        if (updateCategoryDto.Name != null && _categories.Any(c => c.Id != id && c.Name.Equals(updateCategoryDto.Name, StringComparison.OrdinalIgnoreCase) && c.IsActive))
        {
            return BadRequest("A category with this name already exists");
        }

        _mapper.Map(updateCategoryDto, category);

        var categoryDto = _mapper.Map<CategoryResponseDto>(category);
        
        _logger.LogInformation("Category with ID {CategoryId} updated successfully", id);
        
        return Ok(categoryDto);
    }

    /// <summary>
    /// Delete a category (soft delete)
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteCategory(int id)
    {
        _logger.LogInformation("Deleting category with ID: {CategoryId}", id);

        var category = _categories.FirstOrDefault(c => c.Id == id && c.IsActive);
        if (category == null)
        {
            _logger.LogWarning("Category with ID {CategoryId} not found for deletion", id);
            return NotFound($"Category with ID {id} not found");
        }

        // Soft delete
        category.IsActive = false;
        category.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation("Category with ID {CategoryId} deleted successfully", id);
        
        return NoContent();
    }
}