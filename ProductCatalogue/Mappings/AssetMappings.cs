using ProductCatalogue.DTOs.Assets;
using ProductCatalogue.Models;
using ProductCatalogue.Services.Storage;

namespace ProductCatalogue.Mappings;

public static class AssetMappings
{
    public static AssetResponse ToResponse(Asset asset, IStorageService storage) => new()
    {
        Id = asset.Id,
        ProductId = asset.ProductId,
        VariantId = asset.VariantId,
        AssetType = asset.AssetType,
        Status = asset.Status,
        Title = asset.Title,
        Description = asset.Description,
        Tags = asset.Tags.Select(t => t.Tag).ToList(),
        FileName = asset.FileName,
        FileUrl = storage.GetFileUrl(asset.FileName),
        RejectionReason = asset.RejectionReason,
        UploadedAt = asset.UploadedAt,
        StatusHistory = asset.StatusHistory
            .OrderBy(h => h.ChangedAt)
            .Select(h => new AssetStatusHistoryResponse
            {
                PreviousStatus = h.PreviousStatus,
                NewStatus = h.NewStatus,
                Comment = h.Comment,
                ChangedAt = h.ChangedAt,
            })
            .ToList(),
    };
}