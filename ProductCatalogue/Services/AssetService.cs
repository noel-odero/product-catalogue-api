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

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

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

        var asset = new Asset
        {
            ProductId = productId,
            VariantId = request.VariantId,
            AssetType = request.AssetType,
            Status = AssetStatus.Uploaded,
            Title = request.Title,
            Description = request.Description,
            OriginalFileName = stored.OriginalFileName,
            FileName = stored.FileName,
            ContentType = stored.ContentType,
            FileSize = stored.FileSize,
            StoragePath = stored.StoragePath,
            UploadedBy = Guid.Empty, 
            UploadedAt = now,
            Tags = request.Tags
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => new AssetTag { Tag = t.Trim() })
                .ToList(),
            StatusHistory = new List<AssetStatusHistory>
            {
                new()
                {
                    PreviousStatus = AssetStatus.Uploaded,
                    NewStatus = AssetStatus.Uploaded,
                    Comment = "Asset uploaded",
                    ChangedBy = Guid.Empty,
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

        // remove the stored file first, then the record
        await _storage.DeleteAsync(asset.StoragePath, cancellationToken);

        _context.Assets.Remove(asset);
        await _context.SaveChangesAsync(cancellationToken);
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
        // lenient: assets can be added/removed while Draft or InReview
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
}