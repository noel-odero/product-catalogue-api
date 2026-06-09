using System.ComponentModel.DataAnnotations;

namespace ProductCatalogue.DTOs.Products;

public class UpdateProductRequest
{
    [MaxLength(200)]
    public string? Name { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Brand { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    [MaxLength(100)]
    public string? TargetMarket { get; set; }

    [MaxLength(50)]
    public string? Season { get; set; }
}