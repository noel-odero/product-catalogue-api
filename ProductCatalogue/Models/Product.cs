using Microsoft.VisualBasic;

namespace ProductCatalogue.Models;

public class Product
{
    public Guid Id {get; set;} = Guid.NewGuid();
    public string Name {get; set;} = string.Empty;
    public string ProductionCode {get; set;} = string.Empty;
    public string Description {get; set;} = string.Empty;
    public string Brand {get; set;} = string.Empty;
    public string Category {get; set;} = string.Empty;
    public string TargetMarket {get; set;} = string.Empty;
    public string Season {get; set;} = string.Empty;
    public ProductStatus Status {get; set;} = ProductStatus.Draft;
    public DateTimeOffset CreatedAt {get; set;} = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt {get; set;} = DateTimeOffset.UtcNow;

    // Navigation
    public ICollection<Variant> Variants {get; set;} = new List<Variant>();
    public ICollection<Asset> Assets {get; set;} = new List<Asset>();

} 