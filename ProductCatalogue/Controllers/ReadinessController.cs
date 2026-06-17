using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductCatalogue.DTOs.Products;
using ProductCatalogue.Services;

namespace ProductCatalogue.Controllers;

[ApiController]
[Route("api/products/{productId:guid}/readiness")]
[Authorize]
public class ReadinessController : ControllerBase
{
    private readonly IReadinessService _readinessService;

    public ReadinessController(IReadinessService readinessService)
    {
        _readinessService = readinessService;
    }

    [HttpGet]
    public async Task<ActionResult<ReadinessResponse>> Get(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var result = await _readinessService.EvaluateAsync(productId, cancellationToken);
        return Ok(result);
    }
}