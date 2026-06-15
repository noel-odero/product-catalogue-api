namespace ProductCatalogue.DTOs.Products;

public class ProductListResponse
{
    public List<ProductResponse> Items { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}