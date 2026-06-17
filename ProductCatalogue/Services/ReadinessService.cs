using Microsoft.EntityFrameworkCore;
using ProductCatalogue.Data;
using ProductCatalogue.DTOs.Products;
using ProductCatalogue.Exceptions;
using ProductCatalogue.Models;

namespace ProductCatalogue.Services;

public class ReadinessService : IReadinessService
{
    private readonly AppDbContext _context;

    public ReadinessService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ReadinessResponse> EvaluateAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Variants)
            .Include(p => p.Assets)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken)
            ?? throw new NotFoundException($"Product with id '{productId}' not found");

        return Evaluate(product);
    }

    public ReadinessResponse Evaluate(Product product)
    {
        var assets = product.Assets;
        var activeVariants = product.Variants
            .Where(v => v.Status == VariantStatus.Active)
            .ToList();

        var hasRequiredInfo =
            !string.IsNullOrWhiteSpace(product.Name) &&
            !string.IsNullOrWhiteSpace(product.ProductCode) &&
            !string.IsNullOrWhiteSpace(product.Description);

        var hasMainImage = assets.Any(a =>
            a.AssetType == AssetType.MainImage &&
            a.Status == AssetStatus.Approved);

        var allVariantsHaveImages = activeVariants.All(v =>
            assets.Any(a =>
                a.VariantId == v.Id &&
                a.AssetType == AssetType.VariantImage &&
                a.Status == AssetStatus.Approved));

        var noRejectedAssets = assets.All(a => a.Status != AssetStatus.Rejected);

        var reviewComplete = assets.All(a =>
            a.Status != AssetStatus.Uploaded &&
            a.Status != AssetStatus.PendingReview);

        var checks = new List<ReadinessCheckResponse>
        {
            new() { Id = "required-info",   Label = "Product has name, code, and description", Passed = hasRequiredInfo },
            new() { Id = "main-image",      Label = "Has an approved main image",              Passed = hasMainImage },
            new() { Id = "variant-images",  Label = "Every active variant has an approved image", Passed = allVariantsHaveImages },
            new() { Id = "no-rejected",     Label = "No assets are rejected",                  Passed = noRejectedAssets },
            new() { Id = "review-complete", Label = "No assets are awaiting review",           Passed = reviewComplete },
        };

        var passedCount = checks.Count(c => c.Passed);

        return new ReadinessResponse
        {
            ProductId = product.Id,
            Checks = checks,
            PassedCount = passedCount,
            TotalCount = checks.Count,
            CanPublish = passedCount == checks.Count,
        };
    }
}