using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProductCatalogue.Data;
using ProductCatalogue.DTOs.Products;
using ProductCatalogue.Exceptions;
using ProductCatalogue.Mappings;
using ProductCatalogue.Models;

namespace ProductCatalogue.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;
    private readonly IReadinessService _readinessService;


    public ProductService(AppDbContext context, IReadinessService readinessService)
    {
        _context = context;
        _readinessService = readinessService;
    }

    public async Task<ProductListResponse> GetAllAsync(
        ProductQueryParams query,
        CancellationToken cancellationToken = default)
    {
        var products = ApplyFilters(
            _context.Products.AsNoTracking(),
            query);

        var totalCount = await products.CountAsync(cancellationToken);

        var items = await products
            .OrderByDescending(p => p.UpdatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(ProductMappings.ToResponseExpression())
            .ToListAsync(cancellationToken);

        return new ProductListResponse
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize),
        };
    }

    public async Task<ProductDetailResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(ProductMappings.ToDetailResponseExpression())
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Product with id '{id}' not found");
    }

    public async Task<ProductResponse> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var exists = await _context.Products
            .AnyAsync(p => p.ProductCode == request.ProductCode, cancellationToken);

        if (exists)
            throw new ConflictException(
                $"Product code '{request.ProductCode}' already exists");

        var product = new Product
        {
            Name = request.Name,
            ProductCode = request.ProductCode,
            Description = request.Description,
            Brand = request.Brand,
            Category = request.Category,
            TargetMarket = request.TargetMarket,
            Season = request.Season,
            Status = ProductStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _context.Products.Add(product);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new InvalidOperationException(
                $"Product code '{request.ProductCode}' already exists");
        }

        return ProductMappings.ToResponse(product);
    }

    public async Task<ProductResponse> UpdateAsync(
        Guid id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await GetProductOrThrow(id, cancellationToken);

        if (product.Status == ProductStatus.Archived)
            throw new ConflictException("Archived products cannot be updated");

        if (!string.IsNullOrWhiteSpace(request.Name))
            product.Name = request.Name;

        if (!string.IsNullOrWhiteSpace(request.Description))
            product.Description = request.Description;

        if (!string.IsNullOrWhiteSpace(request.Brand))
            product.Brand = request.Brand;

        if (!string.IsNullOrWhiteSpace(request.Category))
            product.Category = request.Category;

        if (!string.IsNullOrWhiteSpace(request.TargetMarket))
            product.TargetMarket = request.TargetMarket;

        if (!string.IsNullOrWhiteSpace(request.Season))
            product.Season = request.Season;

        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return ProductMappings.ToResponse(product);
    }

    public async Task<ProductResponse> SubmitForReviewAsync(
    Guid id,
    CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .Include(p => p.Variants)
            .Include(p => p.Assets)        // assets only — NOT their status history
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Product with id '{id}' not found");

        ValidateReviewSubmission(product);

        var now = DateTimeOffset.UtcNow;

        foreach (var asset in product.Assets.Where(a => a.Status == AssetStatus.Uploaded))
        {
            // add a NEW history row — EF inserts it because it has no key yet
            _context.Set<AssetStatusHistory>().Add(new AssetStatusHistory
            {
                AssetId = asset.Id,
                PreviousStatus = AssetStatus.Uploaded,
                NewStatus = AssetStatus.PendingReview,
                Comment = "Submitted for review",
                ChangedBy = Guid.Empty,
                ChangedAt = now,
            });

            asset.Status = AssetStatus.PendingReview;
        }

        product.Status = ProductStatus.InReview;
        product.UpdatedAt = now;

        await _context.SaveChangesAsync(cancellationToken);

        return ProductMappings.ToResponse(product);
    }

    public async Task<ProductResponse> PublishAsync(
    Guid id,
    CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .Include(p => p.Variants)
            .Include(p => p.Assets)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Product with id '{id}' not found");

        if (product.Status != ProductStatus.InReview)
            throw new ConflictException(
                "Only products under review can be published");

        var readiness = _readinessService.Evaluate(product);

        if (!readiness.CanPublish)
            throw new BusinessRuleException(
                "Product is not ready to publish. All readiness checks must pass.");

        product.Status = ProductStatus.Published;
        product.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return ProductMappings.ToResponse(product);
}
    

    public async Task<ProductResponse> ArchiveAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var product = await GetProductOrThrow(id, cancellationToken);

        if (product.Status == ProductStatus.Archived)
            throw new ConflictException("Product is already archived");

        product.Status = ProductStatus.Archived;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return ProductMappings.ToResponse(product);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var product = await GetProductOrThrow(id, cancellationToken);

        if (product.Status == ProductStatus.Published)
            throw new ConflictException("Published products cannot be deleted");

        _context.Products.Remove(product);

        await _context.SaveChangesAsync(cancellationToken);
    }

    // private helpers

    private async Task<Product> GetProductOrThrow(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Product with id '{id}' not found");
    }

    private static IQueryable<Product> ApplyFilters(
        IQueryable<Product> queryable,
        ProductQueryParams query)
    {
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            queryable = queryable.Where(p =>
                EF.Functions.ILike(p.Name, $"%{query.Search}%") ||
                EF.Functions.ILike(p.ProductCode, $"%{query.Search}%"));
        }

        if (!string.IsNullOrWhiteSpace(query.Brand))
            queryable = queryable.Where(p => p.Brand == query.Brand);

        if (!string.IsNullOrWhiteSpace(query.Category))
            queryable = queryable.Where(p => p.Category == query.Category);

        if (!string.IsNullOrWhiteSpace(query.Status) &&
            Enum.TryParse<ProductStatus>(query.Status, ignoreCase: true, out var status))
        {
            queryable = queryable.Where(p => p.Status == status);
        }

        return queryable;
    }

    private static void ValidateReviewSubmission(Product product)
    {
        if (product.Status != ProductStatus.Draft)
            throw new ConflictException(
                "Only draft products can be submitted for review");

        if (string.IsNullOrWhiteSpace(product.Name))
            throw new BusinessRuleException("Product must have a name");

        if (string.IsNullOrWhiteSpace(product.ProductCode))
            throw new BusinessRuleException("Product must have a product code");

        if (string.IsNullOrWhiteSpace(product.Description))
            throw new BusinessRuleException("Product must have a description");

        if (!product.Variants.Any())
            throw new BusinessRuleException("Product must have at least one variant");

        if (!product.Assets.Any())
            throw new BusinessRuleException("Product must have at least one uploaded asset");
    }
}