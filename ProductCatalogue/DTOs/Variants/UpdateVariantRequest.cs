using System.ComponentModel.DataAnnotations;

namespace ProductCatalogue.DTOs.Variants;

public class UpdateVariantRequest
{
    [MaxLength(200)]
    public string? Name { get; set; }

    [MaxLength(50)]
    public string? Colour { get; set; }

    [MaxLength(50)]
    public string? Size { get; set; }

    [MaxLength(100)]
    public string? Material { get; set; }

    [MaxLength(100)]
    public string? Barcode { get; set; }
}