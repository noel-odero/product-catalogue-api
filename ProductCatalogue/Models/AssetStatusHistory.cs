namespace ProductCatalogue.Models;

public class AssetStatusHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AssetId { get; set; }
    public AssetStatus PreviousStatus { get; set; }
    public AssetStatus NewStatus { get; set; }
    public Guid ChangedBy { get; set; }
    public DateTimeOffset ChangedAt { get; set; } = DateTime.UtcNow;
    public string? Comment { get; set; }

    // navigation property
    public Asset Asset { get; set; } = null!;
}