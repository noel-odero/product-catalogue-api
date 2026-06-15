namespace ProductCatalogue.DTOs.Products;

public class ProductQueryParams
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? Brand { get; set; }
    public string? Category { get; set; }
    public string? Status { get; set; }
}