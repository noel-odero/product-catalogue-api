using System.Linq.Expressions;
using ProductCatalogue.DTOs.Assets;
using ProductCatalogue.DTOs.Products;
using ProductCatalogue.DTOs.Variants;
using ProductCatalogue.Models;

namespace ProductCatalogue.Mappings;

public static class ProductMappings
{
    public static Expression<Func<Product, ProductResponse>> ToResponseExpression() =>
        p => new ProductResponse
        {
            Id = p.Id,
            Name = p.Name,
            ProductCode = p.ProductCode,
            Description = p.Description,
            Brand = p.Brand,
            Category = p.Category,
            TargetMarket = p.TargetMarket,
            Season = p.Season,
            Status = p.Status,
            CreatedAt = p.CreatedAt.UtcDateTime,
            UpdatedAt = p.UpdatedAt.UtcDateTime,
        };

    public static Expression<Func<Product, ProductDetailResponse>> ToDetailResponseExpression() =>
        p => new ProductDetailResponse
        {
            Id = p.Id,
            Name = p.Name,
            ProductCode = p.ProductCode,
            Description = p.Description,
            Brand = p.Brand,
            Category = p.Category,
            TargetMarket = p.TargetMarket,
            Season = p.Season,
            Status = p.Status,
            CreatedAt = p.CreatedAt.UtcDateTime,
            UpdatedAt = p.UpdatedAt.UtcDateTime,
            Variants = p.Variants.Select(v => new VariantResponse
            {
                Id = v.Id,
                ProductId = v.ProductId,
                Name = v.Name,
                VariantCode = v.VariantCode,
                Colour = v.Colour,
                Size = v.Size,
                Material = v.Material,
                Barcode = v.Barcode,
                Status = v.Status,
                CreatedAt = v.CreatedAt,
            }).ToList(),
            Assets = p.Assets.Select(a => new AssetResponse
            {
                Id = a.Id,
                ProductId = a.ProductId,
                VariantId = a.VariantId,
                AssetType = a.AssetType,
                Status = a.Status,
                Title = a.Title,
                Description = a.Description,
                Tags = a.Tags,
                FileName = a.FileName,
                FileUrl = "/uploads/" + a.FileName,
                RejectionReason = a.RejectionReason,
                UploadedAt = a.UploadedAt,
                StatusHistory = a.StatusHistory.Select(h => new AssetStatusHistoryResponse
                {
                    PreviousStatus = h.PreviousStatus,
                    NewStatus = h.NewStatus,
                    Comment = h.Comment,
                    ChangedAt = h.ChangedAt,
                }).ToList(),
            }).ToList(),
        };

    // plain version — for already-materialized entities (create, update, transitions)
    public static ProductResponse ToResponse(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        ProductCode = p.ProductCode,
        Description = p.Description,
        Brand = p.Brand,
        Category = p.Category,
        TargetMarket = p.TargetMarket,
        Season = p.Season,
        Status = p.Status,
        CreatedAt = p.CreatedAt.UtcDateTime,
        UpdatedAt = p.UpdatedAt.UtcDateTime,
    };
}