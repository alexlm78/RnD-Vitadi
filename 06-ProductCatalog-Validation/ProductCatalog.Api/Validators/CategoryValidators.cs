using FluentValidation;
using ProductCatalog.Api.DTOs;

namespace ProductCatalog.Api.Validators;

/// <summary>
/// Validator for CreateCategoryDto with complex validation rules
/// </summary>
public class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Category name is required")
            .MaximumLength(100)
            .WithMessage("Category name cannot exceed 100 characters")
            .MinimumLength(2)
            .WithMessage("Category name must be at least 2 characters long")
            .Must(BeValidCategoryName)
            .WithMessage("Category name can only contain letters, numbers, spaces, and hyphens")
            .Must(NotStartOrEndWithSpace)
            .WithMessage("Category name cannot start or end with spaces");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Category description cannot exceed 500 characters")
            .Must(NotContainHtmlTags)
            .WithMessage("Category description cannot contain HTML tags")
            .When(x => !string.IsNullOrEmpty(x.Description));

        // Business rule: Category name should be title case for consistency
        RuleFor(x => x.Name)
            .Must(BeTitleCase)
            .WithMessage("Category name should be in title case (e.g., 'Electronics', 'Home & Garden')")
            .When(x => !string.IsNullOrEmpty(x.Name));
    }

    private static bool BeValidCategoryName(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        return System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-Z0-9\s\-&]+$");
    }

    private static bool NotStartOrEndWithSpace(string name)
    {
        if (string.IsNullOrEmpty(name)) return true;
        return !name.StartsWith(' ') && !name.EndsWith(' ');
    }

    private static bool NotContainHtmlTags(string? description)
    {
        if (string.IsNullOrEmpty(description)) return true;
        return !System.Text.RegularExpressions.Regex.IsMatch(description, @"<[^>]+>");
    }

    private static bool BeTitleCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return true;
        
        var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var word in words)
        {
            if (word.Length == 0) continue;
            
            // Skip common prepositions and conjunctions that should be lowercase (except at the beginning)
            var lowercaseWords = new[] { "and", "or", "of", "in", "on", "at", "to", "for", "with", "by" };
            if (words[0] != word && lowercaseWords.Contains(word.ToLower()))
            {
                if (word != word.ToLower()) return false;
            }
            else
            {
                // First letter should be uppercase, rest lowercase (except for acronyms)
                if (char.IsLetter(word[0]) && !char.IsUpper(word[0])) return false;
            }
        }
        return true;
    }
}

/// <summary>
/// Validator for UpdateCategoryDto with complex validation rules for nullable properties
/// </summary>
public class UpdateCategoryDtoValidator : AbstractValidator<UpdateCategoryDto>
{
    public UpdateCategoryDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Category name cannot be empty")
            .MaximumLength(100)
            .WithMessage("Category name cannot exceed 100 characters")
            .MinimumLength(2)
            .WithMessage("Category name must be at least 2 characters long")
            .Must(BeValidCategoryName)
            .WithMessage("Category name can only contain letters, numbers, spaces, and hyphens")
            .Must(NotStartOrEndWithSpace)
            .WithMessage("Category name cannot start or end with spaces")
            .Must(BeTitleCase)
            .WithMessage("Category name should be in title case (e.g., 'Electronics', 'Home & Garden')")
            .When(x => x.Name != null);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Category description cannot exceed 500 characters")
            .Must(NotContainHtmlTags)
            .WithMessage("Category description cannot contain HTML tags")
            .When(x => x.Description != null);

        // At least one field must be provided for update
        RuleFor(x => x)
            .Must(HaveAtLeastOneField)
            .WithMessage("At least one field must be provided for update");
    }

    private static bool BeValidCategoryName(string? name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        return System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-Z0-9\s\-&]+$");
    }

    private static bool NotStartOrEndWithSpace(string? name)
    {
        if (string.IsNullOrEmpty(name)) return true;
        return !name.StartsWith(' ') && !name.EndsWith(' ');
    }

    private static bool NotContainHtmlTags(string? description)
    {
        if (string.IsNullOrEmpty(description)) return true;
        return !System.Text.RegularExpressions.Regex.IsMatch(description, @"<[^>]+>");
    }

    private static bool BeTitleCase(string? name)
    {
        if (string.IsNullOrEmpty(name)) return true;
        
        var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var word in words)
        {
            if (word.Length == 0) continue;
            
            // Skip common prepositions and conjunctions that should be lowercase (except at the beginning)
            var lowercaseWords = new[] { "and", "or", "of", "in", "on", "at", "to", "for", "with", "by" };
            if (words[0] != word && lowercaseWords.Contains(word.ToLower()))
            {
                if (word != word.ToLower()) return false;
            }
            else
            {
                // First letter should be uppercase, rest lowercase (except for acronyms)
                if (char.IsLetter(word[0]) && !char.IsUpper(word[0])) return false;
            }
        }
        return true;
    }

    private static bool HaveAtLeastOneField(UpdateCategoryDto category)
    {
        return category.Name != null ||
               category.Description != null ||
               category.IsActive.HasValue;
    }
}