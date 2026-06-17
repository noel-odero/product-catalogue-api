using Microsoft.EntityFrameworkCore;
using ProductCatalogue.Data;
using ProductCatalogue.DTOs.Assets;
using ProductCatalogue.Exceptions;
using ProductCatalogue.Mappings;
using ProductCatalogue.Models;
using ProductCatalogue.Services.Storage;

namespace ProductCatalogue.Services;

public class AssetService : IAssetService
{
    private readonly AppDbContext _context;
    private readonly IStorageService _storage;

    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes = new()
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "application/pdf",
    };

    public AssetService(AppDbContext context, IStorageService storage)
    {
        _context = context;
        _storage = storage;
    }

    public async Task<List<AssetResponse>> GetByProductAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        await EnsureProductExists(productId, cancellationToken);

        var assets = await _context.Assets
            .AsNoTracking()
            .Include(a => a.Tags)
            .Include(a => a.StatusHistory)
            .Where(a => a.ProductId == productId)
            .OrderByDescending(a => a.UploadedAt)
            .ToListAsync(cancellationToken);

        return assets.Select(a => AssetMappings.ToResponse(a, _storage)).ToList();
    }

    public async Task<AssetResponse> GetByIdAsync(
        Guid productId,
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        var asset = await _context.Assets
            .AsNoTracking()
            .Include(a => a.Tags)
            .Include(a => a.StatusHistory)
            .FirstOrDefaultAsync(
                a => a.Id == assetId && a.ProductId == productId,
                cancellationToken)
            ?? throw new NotFoundException(
                $"Asset with id '{assetId}' not found for this product");

        return AssetMappings.ToResponse(asset, _storage);
    }

    

    public async Task<AssetResponse> UploadAsync(
    Guid productId,
    UploadAssetRequest request,
    Guid userId,
    CancellationToken cancellationToken = default)
    {
        var product = await GetProductOrThrow(productId, cancellationToken);

        GuardProductAllowsAssetChanges(product);

        ValidateFile(request.File);

        // if a variant is specified, it must belong to this product
        if (request.VariantId is not null)
        {
            var variantBelongs = await _context.Variants.AnyAsync(
                v => v.Id == request.VariantId && v.ProductId == productId,
                cancellationToken);

            if (!variantBelongs)
                throw new ValidationException(
                    "The specified variant does not belong to this product");
        }

        var stored = await _storage.SaveAsync(request.File, cancellationToken);

        var now = DateTimeOffset.UtcNow;

        // assets uploaded while the product is already under review go straight to PendingReview
        var initialStatus = product.Status == ProductStatus.InReview
            ? AssetStatus.PendingReview
            : AssetStatus.Uploaded;

        var asset = new Asset
        {
            ProductId = productId,
            VariantId = request.VariantId,
            AssetType = request.AssetType,
            Status = initialStatus,
            Title = request.Title,
            Description = request.Description,
            OriginalFileName = stored.OriginalFileName,
            FileName = stored.FileName,
            ContentType = stored.ContentType,
            FileSize = stored.FileSize,
            StoragePath = stored.StoragePath,
            ResourceType = stored.ResourceType,
            UploadedBy = userId,
            UploadedAt = now,
            Tags = request.Tags
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => new AssetTag { Tag = t.Trim() })
                .ToList(),
            StatusHistory = new List<AssetStatusHistory>
            {
                new()
                {
                    PreviousStatus = initialStatus,
                    NewStatus = initialStatus,
                    Comment = initialStatus == AssetStatus.PendingReview
                        ? "Uploaded during review"
                        : "Asset uploaded",
                    ChangedBy = userId,
                    ChangedAt = now,
                }
            },
        };

        _context.Assets.Add(asset);
        await _context.SaveChangesAsync(cancellationToken);

        return AssetMappings.ToResponse(asset, _storage);
    }

    public async Task DeleteAsync(
        Guid productId,
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        var product = await GetProductOrThrow(productId, cancellationToken);

        GuardProductAllowsAssetChanges(product);

        var asset = await _context.Assets
            .FirstOrDefaultAsync(
                a => a.Id == assetId && a.ProductId == productId,
                cancellationToken)
            ?? throw new NotFoundException(
                $"Asset with id '{assetId}' not found for this product");

        var storedFile = new StoredFile(
        StoragePath: asset.StoragePath,
        FileName: asset.FileName,
        OriginalFileName: asset.OriginalFileName,
        ContentType: asset.ContentType,
        FileSize: asset.FileSize,
        ResourceType: asset.ResourceType);

        // remove the stored file first, then the record
        await _storage.DeleteAsync(storedFile, cancellationToken);

        _context.Assets.Remove(asset);
        await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task<AssetResponse> ApproveAsync(
    Guid productId,
    Guid assetId,
    Guid userId,
    CancellationToken cancellationToken = default)
    {
        var asset = await GetReviewableAssetOrThrow(productId, assetId, cancellationToken);

        if (asset.Status != AssetStatus.PendingReview)
            throw new ConflictException(
                "Only assets pending review can be approved");

        TransitionStatus(
            asset,
            AssetStatus.Approved,
            "Asset approved",
            null,
            userId);

        await _context.SaveChangesAsync(cancellationToken);

        return AssetMappings.ToResponse(asset, _storage);
    }

    public async Task<AssetResponse> RejectAsync(
    Guid productId,
    Guid assetId,
    RejectAssetRequest request,
    Guid userId,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new ValidationException("A rejection reason is required");

        var asset = await GetReviewableAssetOrThrow(productId, assetId, cancellationToken);

        if (asset.Status != AssetStatus.PendingReview)
            throw new ConflictException(
                "Only assets pending review can be rejected");

        TransitionStatus(
            asset,
            AssetStatus.Rejected,
            comment: "Asset rejected.",
            rejectionReason: request.Reason,
            userId);

        await _context.SaveChangesAsync(cancellationToken);

        return AssetMappings.ToResponse(asset, _storage);
    }
    // private helpers

    private async Task<Product> GetProductOrThrow(
        Guid productId,
        CancellationToken cancellationToken)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken)
            ?? throw new NotFoundException($"Product with id '{productId}' not found");
    }

    private async Task EnsureProductExists(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var exists = await _context.Products
            .AnyAsync(p => p.Id == productId, cancellationToken);

        if (!exists)
            throw new NotFoundException($"Product with id '{productId}' not found");
    }

    private static void GuardProductAllowsAssetChanges(Product product)
    {
        if (product.Status is not (ProductStatus.Draft or ProductStatus.InReview))
            throw new ConflictException(
                "Assets can only be modified while the product is in draft or under review");
    }

    private static void ValidateFile(IFormFile file)
    {
        if (file is null || file.Length == 0)
            throw new ValidationException("A file is required");

        if (file.Length > MaxFileSizeBytes)
            throw new ValidationException("File exceeds the maximum size of 10 MB");

        if (!AllowedContentTypes.Contains(file.ContentType))
            throw new ValidationException(
                $"File type '{file.ContentType}' is not allowed. " +
                "Allowed types: JPEG, PNG, WebP, PDF");
    }

    private async Task<Asset> GetReviewableAssetOrThrow(
    Guid productId,
    Guid assetId,
    CancellationToken cancellationToken)
    {
        return await _context.Assets
            .Include(a => a.Tags)
            .Include(a => a.StatusHistory)
            .FirstOrDefaultAsync(
                a => a.Id == assetId && a.ProductId == productId,
                cancellationToken)
            ?? throw new NotFoundException(
                $"Asset with id '{assetId}' not found for this product");
    }
    private void TransitionStatus(
    Asset asset,
    AssetStatus newStatus,
    string comment,
    string? rejectionReason,
    Guid userId)
    {
        var now = DateTimeOffset.UtcNow;

        _context.Set<AssetStatusHistory>().Add(new AssetStatusHistory
        {
            AssetId = asset.Id,
            PreviousStatus = asset.Status,
            NewStatus = newStatus,
            Comment = comment,
            ChangedBy = userId,
            ChangedAt = now,
        });

        asset.Status = newStatus;
        asset.RejectionReason = rejectionReason;
    }
}