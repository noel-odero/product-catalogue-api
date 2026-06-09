namespace ProductCatalogue.DTOs.Variants;

public class VariantResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string VariantCode { get; set; } = string.Empty;
    public string Colour { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Material { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}