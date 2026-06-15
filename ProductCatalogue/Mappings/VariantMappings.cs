using System.Linq.Expressions;
using ProductCatalogue.DTOs.Variants;
using ProductCatalogue.Models;

namespace ProductCatalogue.Mappings;

public static class VariantMappings
{
    public static Expression<Func<Variant, VariantResponse>> ToResponseExpression() =>
        v => new VariantResponse
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
        };

    public static VariantResponse ToResponse(Variant v) => new()
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
    };
}