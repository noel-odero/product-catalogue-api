using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductCatalogue.DTOs.Products;
using ProductCatalogue.Services;

namespace ProductCatalogue.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(
        [FromQuery] ProductQueryParams query,
        CancellationToken cancellationToken)
    {
        var result = await _productService.GetAllAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _productService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _productService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _productService.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/submit-for-review")]
    public async Task<ActionResult> SubmitForReview(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _productService.SubmitForReviewAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult> Publish(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _productService.PublishAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<IActionResult> Archive(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _productService.ArchiveAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _productService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}