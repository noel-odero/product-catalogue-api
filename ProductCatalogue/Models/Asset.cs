namespace ProductCatalogue.Models;

public class Asset
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public AssetType AssetType { get; set; }
    public AssetStatus Status { get; set; } = AssetStatus.Uploaded;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new List<string>();
    public string? RejectionReason { get; set; }
    public Guid UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // navigation properties
    public Product Product { get; set; } = null!;
    public Variant? Variant { get; set; }
    public ICollection<AssetStatusHistory> StatusHistory { get; set; } = new List<AssetStatusHistory>();
}