using FluentValidation;
using ProductCatalog.Api.DTOs;

namespace ProductCatalog.Api.Validators;

/// <summary>
/// Validator for CreateProductDto with complex validation rules
/// </summary>
public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Product name is required")
            .MaximumLength(200)
            .WithMessage("Product name cannot exceed 200 characters")
            .MinimumLength(2)
            .WithMessage("Product name must be at least 2 characters long")
            .Must(NotContainSpecialCharacters)
            .WithMessage("Product name cannot contain special characters except spaces, hyphens, and apostrophes");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Product description cannot exceed 1000 characters")
            .Must(NotContainHtmlTags)
            .WithMessage("Product description cannot contain HTML tags");

        RuleFor(x => x.Sku)
            .NotEmpty()
            .WithMessage("SKU is required")
            .MaximumLength(50)
            .WithMessage("SKU cannot exceed 50 characters")
            .Matches(@"^[A-Z0-9\-_]+$")
            .WithMessage("SKU can only contain uppercase letters, numbers, hyphens, and underscores")
            .Must(BeValidSkuFormat)
            .WithMessage("SKU must follow format: 3+ letters followed by numbers (e.g., ABC123)");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0")
            .LessThan(1000000)
            .WithMessage("Price cannot exceed 1,000,000")
            .Must(HaveValidDecimalPlaces)
            .WithMessage("Price can have at most 2 decimal places");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Stock quantity cannot be negative")
            .LessThan(100000)
            .WithMessage("Stock quantity cannot exceed 100,000");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithMessage("Valid category ID is required");

        // Complex conditional validation
        RuleFor(x => x.StockQuantity)
            .GreaterThan(0)
            .WithMessage("Stock quantity must be greater than 0 for active products")
            .When(x => x.IsActive);

        // Cross-field validation
        RuleFor(x => x)
            .Must(HaveReasonableStockForPrice)
            .WithMessage("High-value products (>$1000) should have stock quantity between 1-50")
            .When(x => x.Price > 1000);
    }

    private static bool NotContainSpecialCharacters(string name)
    {
        if (string.IsNullOrEmpty(name)) return true;
        return System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-Z0-9\s\-']+$");
    }

    private static bool NotContainHtmlTags(string? description)
    {
        if (string.IsNullOrEmpty(description)) return true;
        return !System.Text.RegularExpressions.Regex.IsMatch(description, @"<[^>]+>");
    }

    private static bool BeValidSkuFormat(string sku)
    {
        if (string.IsNullOrEmpty(sku)) return false;
        return System.Text.RegularExpressions.Regex.IsMatch(sku, @"^[A-Z]{3,}[0-9]+$");
    }

    private static bool HaveValidDecimalPlaces(decimal price)
    {
        return decimal.Round(price, 2) == price;
    }

    private static bool HaveReasonableStockForPrice(CreateProductDto product)
    {
        if (product.Price <= 1000) return true;
        return product.StockQuantity >= 1 && product.StockQuantity <= 50;
    }
}

/// <summary>
/// Validator for UpdateProductDto with complex validation rules for nullable properties
/// </summary>
public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Product name cannot be empty")
            .MaximumLength(200)
            .WithMessage("Product name cannot exceed 200 characters")
            .MinimumLength(2)
            .WithMessage("Product name must be at least 2 characters long")
            .Must(NotContainSpecialCharacters)
            .WithMessage("Product name cannot contain special characters except spaces, hyphens, and apostrophes")
            .When(x => x.Name != null);

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Product description cannot exceed 1000 characters")
            .Must(NotContainHtmlTags)
            .WithMessage("Product description cannot contain HTML tags")
            .When(x => x.Description != null);

        RuleFor(x => x.Sku)
            .NotEmpty()
            .WithMessage("SKU cannot be empty")
            .MaximumLength(50)
            .WithMessage("SKU cannot exceed 50 characters")
            .Matches(@"^[A-Z0-9\-_]+$")
            .WithMessage("SKU can only contain uppercase letters, numbers, hyphens, and underscores")
            .Must(BeValidSkuFormat)
            .WithMessage("SKU must follow format: 3+ letters followed by numbers (e.g., ABC123)")
            .When(x => x.Sku != null);

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0")
            .LessThan(1000000)
            .WithMessage("Price cannot exceed 1,000,000")
            .Must(HaveValidDecimalPlaces)
            .WithMessage("Price can have at most 2 decimal places")
            .When(x => x.Price.HasValue);

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Stock quantity cannot be negative")
            .LessThan(100000)
            .WithMessage("Stock quantity cannot exceed 100,000")
            .When(x => x.StockQuantity.HasValue);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithMessage("Valid category ID is required")
            .When(x => x.CategoryId.HasValue);

        // Complex conditional validation for updates
        RuleFor(x => x.StockQuantity)
            .GreaterThan(0)
            .WithMessage("Stock quantity must be greater than 0 for active products")
            .When(x => x.IsActive == true && x.StockQuantity.HasValue);

        // At least one field must be provided for update
        RuleFor(x => x)
            .Must(HaveAtLeastOneField)
            .WithMessage("At least one field must be provided for update");
    }

    private static bool NotContainSpecialCharacters(string? name)
    {
        if (string.IsNullOrEmpty(name)) return true;
        return System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-Z0-9\s\-']+$");
    }

    private static bool NotContainHtmlTags(string? description)
    {
        if (string.IsNullOrEmpty(description)) return true;
        return !System.Text.RegularExpressions.Regex.IsMatch(description, @"<[^>]+>");
    }

    private static bool BeValidSkuFormat(string? sku)
    {
        if (string.IsNullOrEmpty(sku)) return false;
        return System.Text.RegularExpressions.Regex.IsMatch(sku, @"^[A-Z]{3,}[0-9]+$");
    }

    private static bool HaveValidDecimalPlaces(decimal? price)
    {
        if (!price.HasValue) return true;
        return decimal.Round(price.Value, 2) == price.Value;
    }

    private static bool HaveAtLeastOneField(UpdateProductDto product)
    {
        return product.Name != null ||
               product.Description != null ||
               product.Sku != null ||
               product.Price.HasValue ||
               product.StockQuantity.HasValue ||
               product.IsActive.HasValue ||
               product.CategoryId.HasValue;
    }
}

/// <summary>
/// Validator for ProductSearchRequestDto with advanced filtering validation
/// </summary>
public class ProductSearchRequestDtoValidator : AbstractValidator<ProductSearchRequestDto>
{
    public ProductSearchRequestDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100)
            .WithMessage("Search name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithMessage("Category ID must be greater than 0")
            .When(x => x.CategoryId.HasValue);

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Minimum price cannot be negative")
            .When(x => x.MinPrice.HasValue);

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Maximum price cannot be negative")
            .When(x => x.MaxPrice.HasValue);

        // Cross-field validation: MinPrice should be less than MaxPrice
        RuleFor(x => x)
            .Must(HaveValidPriceRange)
            .WithMessage("Minimum price must be less than or equal to maximum price")
            .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue);

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100");

        // Business rule: Expensive searches (wide price range) should have smaller page sizes
        RuleFor(x => x.PageSize)
            .LessThanOrEqualTo(20)
            .WithMessage("Page size should be 20 or less for wide price range searches")
            .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue && 
                      (x.MaxPrice.Value - x.MinPrice.Value) > 1000);
    }

    private static bool HaveValidPriceRange(ProductSearchRequestDto request)
    {
        if (!request.MinPrice.HasValue || !request.MaxPrice.HasValue)
            return true;
        
        return request.MinPrice.Value <= request.MaxPrice.Value;
    }
}