using ProductCatalogue.DTOs.Assets;

namespace ProductCatalogue.Services;

public interface IAssetService
{
    Task<List<AssetResponse>> GetByProductAsync(
        Guid productId,
        CancellationToken cancellationToken = default);

    Task<AssetResponse> GetByIdAsync(
        Guid productId,
        Guid assetId,
        CancellationToken cancellationToken = default);

    Task<AssetResponse> UploadAsync(
        Guid productId,
        UploadAssetRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid productId,
        Guid assetId,
        CancellationToken cancellationToken = default);

    // add to IAssetService
    Task<AssetResponse> ApproveAsync(
        Guid productId,
        Guid assetId,
        CancellationToken cancellationToken = default);

    Task<AssetResponse> RejectAsync(
        Guid productId,
        Guid assetId,
        RejectAssetRequest request,
        CancellationToken cancellationToken = default);
}