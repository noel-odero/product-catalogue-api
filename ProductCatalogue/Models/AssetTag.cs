namespace ProductCatalogue.Models;

public class AssetTag
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public string Tag { get; set; } = string.Empty;

    public Asset Asset { get; set; } = null!;
}