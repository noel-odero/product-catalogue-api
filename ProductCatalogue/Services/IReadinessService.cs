using ProductCatalogue.DTOs.Products;
using ProductCatalogue.Models;

namespace ProductCatalogue.Services;

public interface IReadinessService
{
    Task<ReadinessResponse> EvaluateAsync(
        Guid productId,
        CancellationToken cancellationToken = default);

    ReadinessResponse Evaluate(Product product);
}