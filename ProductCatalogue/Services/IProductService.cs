using ProductCatalogue.DTOs.Products;

namespace ProductCatalogue.Services;

public interface IProductService
{
    Task<ProductListResponse> GetAllAsync(ProductQueryParams query, CancellationToken cancellationToken = default);
    Task<ProductDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductResponse> SubmitForReviewAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductResponse> PublishAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductResponse> ArchiveAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}