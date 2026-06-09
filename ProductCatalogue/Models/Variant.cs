namespace ProductCatalogue.Models;

public class Variant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string VariantCode { get; set; } = string.Empty;
    public string Colour { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Material { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public VariantStatus Status { get; set; } = VariantStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // navigation properties
    public Product Product { get; set; } = null!;
    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
}