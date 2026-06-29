using System.ComponentModel.DataAnnotations;
using ProductCatalogue.Models;

namespace ProductCatalogue.DTOs.Assets;

public class UploadAssetRequest
{
    [Required]
    public IFormFile File { get; set; } = null!;

    public Guid? VariantId { get; set; }

    [Required]
    public AssetType AssetType { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    public List<string> Tags { get; set; } = new();
}