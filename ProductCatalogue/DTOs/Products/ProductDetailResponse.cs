using ProductCatalogue.DTOs.Assets;
using ProductCatalogue.DTOs.Variants;
using ProductCatalogue.Models;

namespace ProductCatalogue.DTOs.Products;

public class ProductDetailResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string TargetMarket { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty;
    public ProductStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<VariantResponse> Variants { get; set; } = new();
    public List<AssetResponse> Assets { get; set; } = new();
    public ReadinessResponse Readiness { get; set; } = null!;
}

public class ReadinessResponse
{
    public bool HasRequiredInfo { get; set; }
    public bool HasMainImage { get; set; }
    public bool AllVariantsHaveImages { get; set; }
    public bool NoRejectedAssets { get; set; }
    public bool AllRequiredAssetsApproved { get; set; }
    public bool CanPublish { get; set; }
    public int PassedCount { get; set; }
    public int TotalCount { get; set; }
}