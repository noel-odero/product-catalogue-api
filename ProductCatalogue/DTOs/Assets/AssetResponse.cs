using ProductCatalogue.Models;

namespace ProductCatalogue.DTOs.Assets;

public class AssetResponse
{
    public Guid Id {get; init;}
    public Guid ProductId {get; init;}
    public Guid? VariantId {get; init;}
    public AssetType AssetType {get; init;}
    public AssetStatus Status {get; init;}
    public string Title {get; init;} = string.Empty;
    public string Description {get; init;} = string.Empty;
    public List<string> Tags {get; init;} = new();
    public string FileName {get; init;} = string.Empty;
    public string FileUrl {get; init;} = string.Empty;
    public string? RejectionReason {get; init;}
    public DateTimeOffset UploadedAt {get; init;}
    public List<AssetStatusHistoryResponse> StatusHistory {get; init;} = new();
}

public class AssetStatusHistoryResponse
{
    public AssetStatus PreviousStatus {get; init;}
    public AssetStatus NewStatus {get; init;}
    public string? Comment {get; init;}
    public DateTimeOffset ChangedAt {get; init;}
}