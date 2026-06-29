using ProductCatalogue.Data;
using ProductCatalogue.DTOs.Variants;
using ProductCatalogue.Exceptions;
using ProductCatalogue.Models;
using Microsoft.EntityFrameworkCore;
using ProductCatalogue.Mappings;
using Npgsql;

namespace ProductCatalogue.Services;

public class VariantService : IVariantService
{
    private readonly AppDbContext _context;

    public VariantService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<VariantResponse>> GetByProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        await EnsureProductExists(productId, cancellationToken);

        return await _context.Variants
            .AsNoTracking()
            .Where(v => v.ProductId == productId)
            .OrderBy(v => v.VariantCode)
            .Select(VariantMappings.ToResponseExpression())
            .ToListAsync(cancellationToken);
    }

    public async Task<VariantResponse> AddAsync(Guid productId, CreateVariantRequest request, CancellationToken cancellationToken)
    {
        var product = await GetProductOrThrow(productId, cancellationToken);
        GuardProductIsDraft(product);

        var duplicate = await _context.Variants.AnyAsync(
            v => v.ProductId == productId && v.VariantCode == request.VariantCode, cancellationToken
        );

        if(duplicate)
            throw new ConflictException($"Variant code '{request.VariantCode}' already exists for this product.");

        var variant = new Variant
        {
            ProductId = productId,
            Name = request.Name,
            VariantCode = request.VariantCode,
            Colour = request.Colour,
            Size = request.Size,
            Material = request.Material,
            Barcode = request.Barcode,
            Status = VariantStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        _context.Variants.Add(variant);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch(DbUpdateException ex) when(
            ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation
        )
        {
            throw new ConflictException($"Variant code '{request.VariantCode}' already exists for this product");
        }


        return VariantMappings.ToResponse(variant);
    }


    public async Task<VariantResponse> UpdateAsync(
        Guid productId, Guid variantId, UpdateVariantRequest request, CancellationToken cancellationToken
    )
    {
        var product = await GetProductOrThrow(productId, cancellationToken);

        GuardProductIsDraft(product);

        var variant = await GetVariantOrThrow(productId, variantId, cancellationToken);
        if(!string.IsNullOrWhiteSpace(request.Name))
            variant.Name = request.Name;
        if (!string.IsNullOrWhiteSpace(request.Colour))
            variant.Colour = request.Colour;

        if (!string.IsNullOrWhiteSpace(request.Size))
            variant.Size = request.Size;

        if (!string.IsNullOrWhiteSpace(request.Material))
            variant.Material = request.Material;

        if (request.Barcode is not null)
            variant.Barcode = request.Barcode;

        await _context.SaveChangesAsync(cancellationToken);

        return VariantMappings.ToResponse(variant);
    }

    public async Task DeleteAsync(
        Guid productId,
        Guid variantId,
        CancellationToken cancellationToken = default)
    {
        var product = await GetProductOrThrow(productId, cancellationToken);

        GuardProductIsDraft(product);

        var variant = await GetVariantOrThrow(productId, variantId, cancellationToken);

        _context.Variants.Remove(variant);
        await _context.SaveChangesAsync(cancellationToken);
    }



    // helpers

    // write methods
    private async Task<Product> GetProductOrThrow(
        Guid productId,
        CancellationToken cancellationToken)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken)
            ?? throw new NotFoundException($"Product with id '{productId}' not found");
    }

    // read methods
    private async Task EnsureProductExists(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var exists = await _context.Products
            .AnyAsync(p => p.Id == productId, cancellationToken);

        if (!exists)
            throw new NotFoundException($"Product with id '{productId}' not found");
    }

    private async Task<Variant> GetVariantOrThrow(
        Guid productId,
        Guid variantId,
        CancellationToken cancellationToken)
    {
        return await _context.Variants
            .FirstOrDefaultAsync(
                v => v.Id == variantId && v.ProductId == productId,
                cancellationToken)
            ?? throw new NotFoundException(
                $"Variant with id '{variantId}' not found for this product");
    }


    private static void GuardProductIsDraft(Product product)
    {
        if (product.Status != ProductStatus.Draft)
            throw new ConflictException(
                "Variants can only be modified while the product is in draft");
    }
}