using System.ComponentModel.DataAnnotations;

namespace ProductCatalogue.DTOs.Variants;

public class CreateVariantRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string VariantCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Colour { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Size { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Material { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Barcode { get; set; }
}