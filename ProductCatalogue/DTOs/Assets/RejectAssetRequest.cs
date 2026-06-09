using System.ComponentModel.DataAnnotations;

namespace ProductCatalogue.DTOs.Assets;

public class RejectAssetRequest
{
    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}