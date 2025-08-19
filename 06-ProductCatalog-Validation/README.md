# ProductCatalog API - AutoMapper and FluentValidation Demo

This mini-application demonstrates advanced use of **AutoMapper** and **FluentValidation** in a .NET 8 Web API for managing products and categories. It showcases complex validation patterns, conditional mapping, and real-world scenarios.

## 🎯 Learning Objectives

- Master **AutoMapper** configuration and advanced mapping scenarios
- Implement comprehensive **FluentValidation** with complex business rules
- Understand conditional validation and cross-field validation
- Learn custom mapping logic and calculated properties
- Build robust APIs with automatic validation and error handling
- Explore bulk operations and advanced filtering patterns

## 🏗️ Architecture

```
ProductCatalog.Api/
├── Controllers/           # API Controllers with advanced validation examples
│   ├── CategoriesController.cs
│   └── ProductsController.cs
├── DTOs/                 # Data Transfer Objects (Create, Update, Response, Search)
│   ├── CategoryDto.cs
│   └── ProductDto.cs
├── Models/               # Domain Entities
│   ├── Category.cs
│   └── Product.cs
├── Mapping/              # AutoMapper Profiles with custom logic
│   └── MappingProfile.cs
├── Validators/           # FluentValidation Validators with complex rules
│   ├── CategoryValidators.cs
│   └── ProductValidators.cs
└── Program.cs           # Application Configuration
```

## 📦 Key Libraries Used

- **AutoMapper** (12.0.1) - Object-to-object mapping with custom logic
- **AutoMapper.Extensions.Microsoft.DependencyInjection** (12.0.1) - DI integration
- **FluentValidation** (12.0.0) - Fluent validation with complex rules
- **FluentValidation.DependencyInjectionExtensions** (12.0.0) - DI integration
- **Swashbuckle.AspNetCore** (6.6.2) - API documentation with examples

## 🚀 Getting Started

### Prerequisites

- .NET 8 SDK
- Visual Studio 2022 or VS Code

### Running the Application

1. Navigate to the project directory:
   ```bash
   cd 06-ProductCatalog-Validation/ProductCatalog.Api
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Run the application:
   ```bash
   dotnet run
   ```

4. Open your browser and navigate to `https://localhost:5001` to access the Swagger UI

## 🔧 Advanced Features Demonstrated

### 1. AutoMapper Advanced Patterns

#### Custom Mapping Logic
```csharp
CreateMap<Product, ProductSummaryDto>()
    .ForMember(dest => dest.TotalValue, opt => opt.MapFrom(src => src.Price * src.StockQuantity))
    .ForMember(dest => dest.StockStatus, opt => opt.MapFrom(src => GetStockStatus(src.StockQuantity)));
```

#### Conditional Mapping
- Different DTOs based on request parameters (`ProductResponseDto` vs `ProductSummaryDto`)
- Navigation property mapping with null handling
- Calculated fields and business logic in mapping

#### Update Mapping with Null Handling
```csharp
CreateMap<UpdateProductDto, Product>()
    .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
```

### 2. FluentValidation Complex Patterns

#### Cross-Field Validation
```csharp
RuleFor(x => x)
    .Must(HaveReasonableStockForPrice)
    .WithMessage("High-value products (>$1000) should have stock quantity between 1-50")
    .When(x => x.Price > 1000);
```

#### Conditional Validation
```csharp
RuleFor(x => x.StockQuantity)
    .GreaterThan(0)
    .WithMessage("Stock quantity must be greater than 0 for active products")
    .When(x => x.IsActive);
```

#### Custom Validation Methods
```csharp
private static bool BeValidSkuFormat(string sku)
{
    return System.Text.RegularExpressions.Regex.IsMatch(sku, @"^[A-Z]{3,}[0-9]+$");
}
```

#### Business Rule Validation
```csharp
RuleFor(x => x.Name)
    .Must(BeTitleCase)
    .WithMessage("Category name should be in title case")
    .When(x => !string.IsNullOrEmpty(x.Name));
```

### 3. Advanced API Patterns

#### Bulk Operations with Partial Success
- `POST /api/products/bulk` - Create multiple products with error handling
- Individual validation for each item
- Detailed error reporting with index and SKU information

#### Advanced Search with Filtering
- `POST /api/products/search` - Complex search with multiple criteria
- Pagination with validation
- Conditional response formatting

#### Validation Separation
- **FluentValidation**: Input format and structure validation
- **Controller Logic**: Business rules and data consistency validation

## 📋 API Endpoints

### Categories
- `GET /api/categories` - Get all categories
- `GET /api/categories/{id}` - Get category by ID
- `POST /api/categories` - Create new category
- `PUT /api/categories/{id}` - Update category
- `DELETE /api/categories/{id}` - Delete category (soft delete)

### Products (Basic)
- `GET /api/products` - Get all products (with optional category filter)
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create new product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product (soft delete)

### Products (Advanced)
- `POST /api/products/bulk` - Bulk create products with error handling
- `POST /api/products/search` - Advanced search with filtering and pagination

## 🧪 Testing Advanced Features

### Bulk Product Creation
```json
POST /api/products/bulk
[
  {
    "name": "Gaming Mouse",
    "sku": "MOUSE001",
    "price": 79.99,
    "stockQuantity": 15,
    "categoryId": 1
  },
  {
    "name": "Mechanical Keyboard", 
    "sku": "KEYB001",
    "price": 149.99,
    "stockQuantity": 8,
    "categoryId": 1
  }
]
```

### Advanced Product Search
```json
POST /api/products/search
{
  "name": "wireless",
  "categoryId": 1,
  "minPrice": 50.00,
  "maxPrice": 300.00,
  "inStock": true,
  "page": 1,
  "pageSize": 10,
  "includeCategory": true
}
```

### Validation Examples

#### Valid Product (passes all validation)
```json
{
  "name": "Premium Headphones",
  "description": "High-quality wireless headphones",
  "sku": "HEAD123",
  "price": 199.99,
  "stockQuantity": 25,
  "categoryId": 1,
  "isActive": true
}
```

#### Invalid Product (demonstrates validation errors)
```json
{
  "name": "A",                    // Too short (min 2 chars)
  "description": "<script>alert('xss')</script>", // Contains HTML
  "sku": "invalid-sku",           // Invalid format (should be ABC123)
  "price": -10.99,                // Negative price
  "stockQuantity": -5,            // Negative stock
  "categoryId": 0,                // Invalid category
  "isActive": true
}
```

## 🎓 Advanced Learning Exercises

### Exercise 1: Category-Specific Validation
Implement conditional validation where electronics require warranty information, books require ISBN, and clothing requires size information.

```csharp
RuleFor(x => x.WarrantyMonths)
    .GreaterThan(0)
    .WithMessage("Electronics must have warranty information")
    .When(x => IsElectronicsCategory(x.CategoryId));
```

### Exercise 2: Custom AutoMapper Resolvers
Create a custom value resolver for complex mapping scenarios:

```csharp
public class PriceFormattingResolver : IValueResolver<Product, ProductResponseDto, string>
{
    public string Resolve(Product source, ProductResponseDto destination, string destMember, ResolutionContext context)
    {
        return source.Price.ToString("C2", CultureInfo.GetCultureInfo("en-US"));
    }
}
```

### Exercise 3: Async Validation
Implement async validators that check external services:

```csharp
RuleFor(x => x.Sku)
    .MustAsync(BeUniqueSkuAsync)
    .WithMessage("SKU must be unique across all products");
```

### Exercise 4: Nested Object Validation
Create complex DTOs with nested validation:

```csharp
public class ProductWithSpecificationsDto
{
    public string Name { get; set; }
    public List<ProductSpecificationDto> Specifications { get; set; }
}

RuleForEach(x => x.Specifications)
    .SetValidator(new ProductSpecificationValidator());
```

## 🔍 Key Patterns Explained

### 1. Validation Layering Strategy

```
┌─────────────────────────────────────┐
│           Controller Layer          │ ← Business Rules & Data Consistency
├─────────────────────────────────────┤
│        FluentValidation Layer       │ ← Input Format & Structure
├─────────────────────────────────────┤
│         Data Annotation Layer       │ ← Basic Constraints (if needed)
└─────────────────────────────────────┘
```

**FluentValidation handles:**
- Input format validation (regex, length, range)
- Data type validation
- Cross-field validation within the same object
- Custom business rule validation

**Controller handles:**
- Data consistency validation (unique constraints)
- External service validation
- Complex business rules involving multiple entities

### 2. AutoMapper Configuration Patterns

#### Profile Organization
```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Group related mappings
        ConfigureCategoryMappings();
        ConfigureProductMappings();
        ConfigureAdvancedMappings();
    }
    
    private void ConfigureCategoryMappings() { /* ... */ }
    private void ConfigureProductMappings() { /* ... */ }
    private void ConfigureAdvancedMappings() { /* ... */ }
}
```

#### Conditional Mapping
```csharp
CreateMap<Product, ProductDto>()
    .ForMember(dest => dest.CategoryName, 
        opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : "Unknown"));
```

#### Custom Value Resolvers
```csharp
CreateMap<Product, ProductSummaryDto>()
    .ForMember(dest => dest.StockStatus, opt => opt.MapFrom<StockStatusResolver>());
```

### 3. Error Handling Patterns

#### Structured Error Responses
```csharp
return BadRequest(new 
{ 
    error = "A product with this SKU already exists", 
    field = "sku",
    code = "DUPLICATE_SKU"
});
```

#### Bulk Operation Error Handling
```csharp
public class BulkCreateResultDto
{
    public int TotalRequested { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public List<ProductResponseDto> SuccessfullyCreated { get; set; }
    public List<BulkCreateErrorDto> Errors { get; set; }
}
```

## 🚨 Advanced Pitfalls to Avoid

### 1. AutoMapper Performance Issues
```csharp
// ❌ Bad: Mapping in loops without projection
foreach (var product in products)
{
    var dto = _mapper.Map<ProductDto>(product);
    // Process dto
}

// ✅ Good: Use ProjectTo for IQueryable
var dtos = products.ProjectTo<ProductDto>(_mapper.ConfigurationProvider);
```

### 2. Validation Performance
```csharp
// ❌ Bad: Expensive validation in loops
RuleFor(x => x.Sku)
    .MustAsync(async (sku, cancellation) => await CheckSkuInDatabaseAsync(sku));

// ✅ Good: Batch validation or caching
RuleFor(x => x.Sku)
    .Must(BeValidSkuFormat)  // Fast regex check first
    .MustAsync(BeUniqueSkuAsync).When(x => BeValidSkuFormat(x.Sku));
```

### 3. Circular Reference Issues
```csharp
// ❌ Bad: Can cause infinite loops
CreateMap<Category, CategoryDto>()
    .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Products));

CreateMap<Product, ProductDto>()
    .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category));

// ✅ Good: Use different DTOs or ignore navigation properties
CreateMap<Category, CategorySummaryDto>()
    .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products.Count));
```

## 📚 Further Reading

- [AutoMapper Documentation](https://docs.automapper.org/)
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
- [ASP.NET Core Model Validation](https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation)
- [DTO Pattern Best Practices](https://martinfowler.com/eaaCatalog/dataTransferObject.html)
- [AutoMapper Performance Tips](https://docs.automapper.org/en/stable/Performance.html)
- [FluentValidation Advanced Scenarios](https://docs.fluentvalidation.net/en/latest/advanced.html)

## 🎯 Next Steps

After mastering this example, consider:
1. **Entity Framework Integration** - Real database with complex relationships
2. **Repository Pattern** - Abstracted data access layer
3. **CQRS Pattern** - Separate read/write models with different validation
4. **Authentication & Authorization** - Role-based validation rules
5. **Integration Testing** - Test validation and mapping in real scenarios
6. **Performance Optimization** - Profiling and optimizing mapping operations
7. **Localization** - Multi-language validation messages
8. **Custom Validation Attributes** - Combining FluentValidation with attributes