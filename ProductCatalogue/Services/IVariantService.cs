using ProductCatalogue.DTOs.Variants;

namespace ProductCatalogue.Services;

public interface IVariantService
{
    Task<List<VariantResponse>> GetByProductAsync(
        Guid ProductId,
        CancellationToken cancellationToken = default
    );


    Task<VariantResponse> AddAsync(
        Guid ProductId,
        CreateVariantRequest request,
        CancellationToken cancellationToken = default
    );

    Task<VariantResponse> UpdateAsync(
        Guid productId,
        Guid variantId,
        UpdateVariantRequest request,
        CancellationToken cancellationToken = default);


    Task DeleteAsync(
        Guid productId,
        Guid variantId,
        CancellationToken cancellationToken = default);
}