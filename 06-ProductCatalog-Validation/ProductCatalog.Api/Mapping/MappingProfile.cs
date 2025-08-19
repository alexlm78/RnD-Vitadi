using AutoMapper;
using ProductCatalog.Api.DTOs;
using ProductCatalog.Api.Models;

namespace ProductCatalog.Api.Mapping;

/// <summary>
/// AutoMapper profile for mapping between entities and DTOs
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Category mappings
        CreateMap<Category, CategoryResponseDto>()
            .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products.Count));
        
        CreateMap<CreateCategoryDto, Category>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Products, opt => opt.Ignore());
        
        CreateMap<UpdateCategoryDto, Category>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Products, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Product mappings
        CreateMap<Product, ProductResponseDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category));
        
        CreateMap<CreateProductDto, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Category, opt => opt.Ignore());
        
        CreateMap<UpdateProductDto, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Advanced product mappings with custom logic
        CreateMap<Product, ProductSummaryDto>()
            .ForMember(dest => dest.TotalValue, opt => opt.MapFrom(src => src.Price * src.StockQuantity))
            .ForMember(dest => dest.StockStatus, opt => opt.MapFrom(src => GetStockStatus(src.StockQuantity)));
    }

    /// <summary>
    /// Custom mapping logic for stock status
    /// Demonstrates conditional mapping based on business rules
    /// </summary>
    private static string GetStockStatus(int stockQuantity)
    {
        return stockQuantity switch
        {
            0 => "Out of Stock",
            <= 5 => "Low Stock",
            <= 20 => "In Stock",
            _ => "Well Stocked"
        };
    }
}