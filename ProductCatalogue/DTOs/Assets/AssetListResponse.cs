using ProductCatalogue.DTOs.Assets;

namespace ProductCatalogue.DTOs.Assets;

public class AssetListResponse
{
    public List<AssetResponse> Items { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}