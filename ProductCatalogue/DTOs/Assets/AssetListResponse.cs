using ProductCatalogue.DTOs.Assets;

namespace ProductCatalogue.DTOs.Assets;

public class AssetListResponse
{
    public List<AssetResponse> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}