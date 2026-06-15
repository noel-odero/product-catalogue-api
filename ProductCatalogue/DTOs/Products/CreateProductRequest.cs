using System.ComponentModel.DataAnnotations;

namespace ProductCatalogue.DTOs.Products;

public class CreateProductRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string ProductCode { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Brand { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string TargetMarket { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Season { get; set; } = string.Empty;
}