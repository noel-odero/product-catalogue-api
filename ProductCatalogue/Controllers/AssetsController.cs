using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductCatalogue.DTOs.Assets;
using ProductCatalogue.Services;

namespace ProductCatalogue.Controllers;

[ApiController]
[Route("api/products/{productId}/assets")]
[Authorize]
public class AssetsController : ControllerBase
{
    private readonly IAssetService _assetService;

    public AssetsController(IAssetService assetService)
    {
        _assetService = assetService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AssetResponse>>> GetByProduct(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var result = await _assetService.GetByProductAsync(productId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{assetId}")]
    public async Task<ActionResult<AssetResponse>> GetById(
        Guid productId,
        Guid assetId,
        CancellationToken cancellationToken)
    {
        var result = await _assetService.GetByIdAsync(productId, assetId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<AssetResponse>> Upload(
        Guid productId,
        [FromForm] UploadAssetRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _assetService.UploadAsync(
            productId, request, GetUserId(), cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { productId, assetId = result.Id },
            result);
    }

    [HttpPost("{assetId}/approve")]
    public async Task<ActionResult<AssetResponse>> Approve(
        Guid productId,
        Guid assetId,
        CancellationToken cancellationToken)
    {
        var result = await _assetService.ApproveAsync(
            productId, assetId, GetUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{assetId}/reject")]
    public async Task<ActionResult<AssetResponse>> Reject(
        Guid productId,
        Guid assetId,
        [FromBody] RejectAssetRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _assetService.RejectAsync(
            productId, assetId, request, GetUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{assetId}")]
    public async Task<IActionResult> Delete(
        Guid productId,
        Guid assetId,
        CancellationToken cancellationToken)
    {
        await _assetService.DeleteAsync(productId, assetId, cancellationToken);
        return NoContent();
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(sub, out var id)
            ? id
            : throw new UnauthorizedAccessException("User id missing from token");
    }
}