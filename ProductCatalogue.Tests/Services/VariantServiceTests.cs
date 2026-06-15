using Microsoft.EntityFrameworkCore;
using ProductCatalogue.Data;
using ProductCatalogue.DTOs.Variants;
using ProductCatalogue.Exceptions;
using ProductCatalogue.Models;
using ProductCatalogue.Services;

namespace ProductCatalogue.Tests.Services;

public class VariantServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly VariantService _service;

    public VariantServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _service = new VariantService(_context);
    }

    public void Dispose() => _context.Dispose();

    // helpers

    private async Task<Product> SeedProductAsync(
        ProductStatus status = ProductStatus.Draft,
        string productCode = "MWC-001")
    {
        var product = new Product
        {
            Name = "Merino Wool Coat",
            ProductCode = productCode,
            Description = "A premium merino wool coat",
            Brand = "Heritage",
            Category = "Outerwear",
            TargetMarket = "Women",
            Season = "AW24",
            Status = status,
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    private async Task<Variant> SeedVariantAsync(
        Guid productId,
        string variantCode = "CAM-S")
    {
        var variant = new Variant
        {
            ProductId = productId,
            Name = "Camel S",
            VariantCode = variantCode,
            Colour = "Camel",
            Size = "S",
            Material = "Merino Wool",
            Status = VariantStatus.Active,
        };

        _context.Variants.Add(variant);
        await _context.SaveChangesAsync();
        return variant;
    }

    private static CreateVariantRequest ValidCreateRequest(string code = "CAM-S") => new()
    {
        Name = "Camel S",
        VariantCode = code,
        Colour = "Camel",
        Size = "S",
        Material = "Merino Wool",
        Barcode = "1234567890123",
    };

    // AddAsync

    [Fact]
    public async Task AddAsync_WithValidData_Succeeds()
    {
        var product = await SeedProductAsync();

        var result = await _service.AddAsync(product.Id, ValidCreateRequest(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("CAM-S", result.VariantCode);
        Assert.Equal(product.Id, result.ProductId);
        Assert.Equal(VariantStatus.Active, result.Status);
    }

    [Fact]
    public async Task AddAsync_WhenProductNotFound_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.AddAsync(Guid.NewGuid(), ValidCreateRequest(), CancellationToken.None));
    }

    [Fact]
    public async Task AddAsync_WithDuplicateCode_ThrowsConflict()
    {
        var product = await SeedProductAsync();
        await SeedVariantAsync(product.Id, "CAM-S");

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.AddAsync(product.Id, ValidCreateRequest("CAM-S"), CancellationToken.None));
    }

    [Fact]
    public async Task AddAsync_AllowsSameCodeOnDifferentProducts()
    {
        var productA = await SeedProductAsync(productCode: "PROD-A");
        var productB = await SeedProductAsync(productCode: "PROD-B");

        await _service.AddAsync(productA.Id, ValidCreateRequest("SHARED"), CancellationToken.None);
        var result = await _service.AddAsync(productB.Id, ValidCreateRequest("SHARED"), CancellationToken.None);

        Assert.Equal("SHARED", result.VariantCode);
        Assert.Equal(productB.Id, result.ProductId);
    }

    [Theory]
    [InlineData(ProductStatus.InReview)]
    [InlineData(ProductStatus.ReadyToPublish)]
    [InlineData(ProductStatus.Published)]
    [InlineData(ProductStatus.Archived)]
    public async Task AddAsync_WhenProductNotDraft_ThrowsConflict(ProductStatus status)
    {
        var product = await SeedProductAsync(status: status);

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.AddAsync(product.Id, ValidCreateRequest(), CancellationToken.None));
    }

    // UpdateAsync

    [Fact]
    public async Task UpdateAsync_WithValidData_Succeeds()
    {
        var product = await SeedProductAsync();
        var variant = await SeedVariantAsync(product.Id);

        var result = await _service.UpdateAsync(product.Id, variant.Id,
            new UpdateVariantRequest { Colour = "Charcoal", Size = "M" }, CancellationToken.None);

        Assert.Equal("Charcoal", result.Colour);
        Assert.Equal("M", result.Size);
        Assert.Equal("CAM-S", result.VariantCode); // code unchanged
    }

    [Fact]
    public async Task UpdateAsync_WhenVariantNotFound_ThrowsNotFound()
    {
        var product = await SeedProductAsync();

        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.UpdateAsync(product.Id, Guid.NewGuid(),
                new UpdateVariantRequest { Colour = "Charcoal" }, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_WhenVariantBelongsToDifferentProduct_ThrowsNotFound()
    {
        var productA = await SeedProductAsync(productCode: "PROD-A");
        var productB = await SeedProductAsync(productCode: "PROD-B");
        var variantOnA = await SeedVariantAsync(productA.Id);

        // try to update productA's variant via productB's id
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.UpdateAsync(productB.Id, variantOnA.Id,
                new UpdateVariantRequest { Colour = "Charcoal" }, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_WhenProductNotDraft_ThrowsConflict()
    {
        var product = await SeedProductAsync();
        var variant = await SeedVariantAsync(product.Id);

        // move product out of draft
        product.Status = ProductStatus.InReview;
        await _context.SaveChangesAsync();

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.UpdateAsync(product.Id, variant.Id,
                new UpdateVariantRequest { Colour = "Charcoal" }, CancellationToken.None));
    }

    // DeleteAsync

    [Fact]
    public async Task DeleteAsync_WhenDraft_Succeeds()
    {
        var product = await SeedProductAsync();
        var variant = await SeedVariantAsync(product.Id);

        await _service.DeleteAsync(product.Id, variant.Id, CancellationToken.None);

        var exists = await _context.Variants.AnyAsync(v => v.Id == variant.Id);
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteAsync_WhenVariantNotFound_ThrowsNotFound()
    {
        var product = await SeedProductAsync();

        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.DeleteAsync(product.Id, Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_WhenProductNotDraft_ThrowsConflict()
    {
        var product = await SeedProductAsync();
        var variant = await SeedVariantAsync(product.Id);

        product.Status = ProductStatus.Published;
        await _context.SaveChangesAsync();

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.DeleteAsync(product.Id, variant.Id, CancellationToken.None));
    }

    // GetByProductAsync

    [Fact]
    public async Task GetByProductAsync_ReturnsVariantsForProduct()
    {
        var product = await SeedProductAsync();
        await SeedVariantAsync(product.Id, "CAM-S");
        await SeedVariantAsync(product.Id, "CAM-M");

        var result = await _service.GetByProductAsync(product.Id, CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByProductAsync_WhenProductNotFound_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.GetByProductAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task GetByProductAsync_ReturnsOnlyThatProductsVariants()
    {
        var productA = await SeedProductAsync(productCode: "PROD-A");
        var productB = await SeedProductAsync(productCode: "PROD-B");
        await SeedVariantAsync(productA.Id, "A-1");
        await SeedVariantAsync(productB.Id, "B-1");

        var result = await _service.GetByProductAsync(productA.Id, CancellationToken.None);

        Assert.Single(result);
        Assert.All(result, v => Assert.Equal(productA.Id, v.ProductId));
    }
}