using System.ComponentModel.DataAnnotations;

namespace ProductCatalogue.DTOs.Products;

public class CreateProductRequest
{
    [Required]
    [MaxLength(200)]
    public string Name {get; init;} = string.Empty;

    [Required]
    [MaxLength(50)]
    public string ProductCode {get; init;} = string.Empty;

    [MaxLength(2000)]
    public string Description {get; init;} = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Brand {get; init;} = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Category {get; init;} = string.Empty;

    [Required]
    [MaxLength(100)]
    public string TargetMarket {get; init;} = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Season {get; init;} = string.Empty;
}