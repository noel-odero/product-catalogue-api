using ProductCatalogue.Models;

namespace ProductCatalogue.DTOs.Assets;

public class AssetResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public AssetType AssetType { get; set; }
    public AssetStatus Status { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public DateTimeOffset UploadedAt { get; set; }
    public List<AssetStatusHistoryResponse> StatusHistory { get; set; } = new();
}

public class AssetStatusHistoryResponse
{
    public AssetStatus PreviousStatus { get; set; }
    public AssetStatus NewStatus { get; set; }
    public string? Comment { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
}