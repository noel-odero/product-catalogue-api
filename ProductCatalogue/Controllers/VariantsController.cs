using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductCatalogue.DTOs.Variants;
using ProductCatalogue.Services;

namespace ProductCatalogue.Controllers;

[ApiController]
[Route("api/products/{productId:guid}/variants")]
[Authorize]
public class VariantsController : ControllerBase
{
    private readonly IVariantService _variantService;

    public VariantsController(IVariantService variantService)
    {
        _variantService = variantService;
    }

    [HttpGet]
    public async Task<ActionResult> GetByProduct(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var result = await _variantService.GetByProductAsync(productId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> Add(
        Guid productId,
        [FromBody] CreateVariantRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _variantService.AddAsync(productId, request, cancellationToken);
        return CreatedAtAction(
            nameof(GetByProduct),
            new { productId },
            result);
    }

    [HttpPut("{variantId:guid}")]
    public async Task<IActionResult> Update(
        Guid productId,
        Guid variantId,
        [FromBody] UpdateVariantRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _variantService.UpdateAsync(
            productId, variantId, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{variantId:guid}")]
    public async Task<IActionResult> Delete(
        Guid productId,
        Guid variantId,
        CancellationToken cancellationToken)
    {
        await _variantService.DeleteAsync(productId, variantId, cancellationToken);
        return NoContent();
    }
}