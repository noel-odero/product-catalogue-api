using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProductCatalogue.Contracts;
using ProductCatalogue.Data;
using ProductCatalogue.DTOs.Products;
using ProductCatalogue.Exceptions;
using ProductCatalogue.Models;
using ProductCatalogue.Services;
using ProductCatalogue.Services.Storage;
using ProductCatalogue.Infrastructure.Kafka;

namespace ProductCatalogue.Tests.Services;

public class ProductServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ProductService _service;
    private readonly FakeEventPublisher _events = new();

    public ProductServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);

        var kafkaOptions = Options.Create(new KafkaSettings
        {
            BootstrapServers = "localhost:9092",
            AssetEventsTopic = "test.asset-events",
            ProductEventsTopic = "test.product-events",
        });

        _service = new ProductService(
            _context,
            new ReadinessService(_context),
            new FakeStorageService(),
            _events,
            kafkaOptions);
    }

    public void Dispose() => _context.Dispose();

    // fakes

    private class FakeEventPublisher : IEventPublisher
    {
        public List<(string EventType, object Payload)> Enqueued { get; } = new();

        public void Enqueue<TPayload>(string topic, string key, string eventType, TPayload payload)
            => Enqueued.Add((eventType, payload!));
    }

    private class FakeStorageService : IStorageService
    {
        public Task<StoredFile> SaveAsync(IFormFile file, CancellationToken ct = default)
            => Task.FromResult(new StoredFile(
                StoragePath: "fake/path",
                FileName: "fake.jpg",
                OriginalFileName: file.FileName,
                ContentType: file.ContentType,
                FileSize: file.Length,
                ResourceType: "image"));

        public Task DeleteAsync(StoredFile file, CancellationToken ct = default)
            => Task.CompletedTask;

        public string GetFileUrl(string fileName)
            => $"https://fake/{fileName}";
    }

    // seeding helpers

    private async Task<Product> SeedProductAsync(
        ProductStatus status = ProductStatus.Draft,
        string productCode = "MWC-001",
        bool withVariant = false,
        bool withAsset = false,
        string description = "A premium merino wool coat",
        string brand = "Heritage")
    {
        var product = new Product
        {
            Name = "Merino Wool Coat",
            ProductCode = productCode,
            Description = description,
            Brand = brand,
            Category = "Outerwear",
            TargetMarket = "Women",
            Season = "AW24",
            Status = status,
        };

        if (withVariant)
        {
            product.Variants.Add(new Variant
            {
                Name = "Camel S",
                VariantCode = "CAM-S",
                Colour = "Camel",
                Size = "S",
                Material = "Merino Wool",
            });
        }

        if (withAsset)
        {
            product.Assets.Add(new Asset
            {
                OriginalFileName = "hero.jpg",
                FileName = "hero-001.jpg",
                ContentType = "image/jpeg",
                FileSize = 1024,
                StoragePath = "uploads/hero-001.jpg",
                AssetType = AssetType.MainImage,
                Status = AssetStatus.PendingReview,
                Title = "Hero shot",
                Description = "Front facing hero",
                UploadedBy = Guid.NewGuid(),
            });
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    private static CreateProductRequest ValidCreateRequest(string code = "MWC-001") => new()
    {
        Name = "Merino Wool Coat",
        ProductCode = code,
        Description = "A premium merino wool coat",
        Brand = "Heritage",
        Category = "Outerwear",
        TargetMarket = "Women",
        Season = "AW24",
    };

    // CreateAsync

    [Fact]
    public async Task CreateAsync_WithValidData_Succeeds()
    {
        var result = await _service.CreateAsync(ValidCreateRequest());

        Assert.NotNull(result);
        Assert.Equal("Merino Wool Coat", result.Name);
        Assert.Equal("MWC-001", result.ProductCode);
        Assert.Equal(ProductStatus.Draft, result.Status);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateCode_ThrowsConflict()
    {
        await _service.CreateAsync(ValidCreateRequest());

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.CreateAsync(ValidCreateRequest()));
    }

    // GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ReturnsProduct()
    {
        var product = await SeedProductAsync(withVariant: true, withAsset: true);

        var result = await _service.GetByIdAsync(product.Id);

        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
        Assert.Single(result.Variants);
        Assert.Single(result.Assets);
    }

    [Fact]
    public async Task GetByIdAsync_WhenIdNotFound_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.GetByIdAsync(Guid.NewGuid()));
    }

    // SubmitForReviewAsync

    [Fact]
    public async Task SubmitForReviewAsync_WithVariantAndAsset_Succeeds()
    {
        var product = await SeedProductAsync(withVariant: true, withAsset: true);

        var result = await _service.SubmitForReviewAsync(product.Id);

        Assert.Equal(ProductStatus.InReview, result.Status);
    }

    [Theory]
    [InlineData(false, true)]   // no variants
    [InlineData(true, false)]   // no assets
    [InlineData(false, false)]  // neither
    public async Task SubmitForReviewAsync_WhenMissingRequirements_ThrowsBusinessRule(
        bool withVariant,
        bool withAsset)
    {
        var product = await SeedProductAsync(
            withVariant: withVariant, withAsset: withAsset);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => _service.SubmitForReviewAsync(product.Id));
    }

    [Theory]
    [InlineData(ProductStatus.InReview)]
    [InlineData(ProductStatus.ReadyToPublish)]
    [InlineData(ProductStatus.Published)]
    [InlineData(ProductStatus.Archived)]
    public async Task SubmitForReviewAsync_WhenNotDraft_ThrowsConflict(ProductStatus status)
    {
        var product = await SeedProductAsync(
            status: status, withVariant: true, withAsset: true);

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.SubmitForReviewAsync(product.Id));
    }

    // PublishAsync

    [Fact]
    public async Task PublishAsync_WhenInReviewAndReady_Succeeds()
    {
        // InReview product with an approved main image and no active variants → ready
        var product = new Product
        {
            Name = "Merino Wool Coat",
            ProductCode = "PUB-001",
            Description = "A premium merino wool coat",
            Brand = "Heritage",
            Category = "Outerwear",
            TargetMarket = "Women",
            Season = "AW24",
            Status = ProductStatus.InReview,
        };
        product.Assets.Add(new Asset
        {
            AssetType = AssetType.MainImage,
            Status = AssetStatus.Approved,
            Title = "Hero",
            OriginalFileName = "hero.jpg",
            FileName = "hero-001.jpg",
            ContentType = "image/jpeg",
            FileSize = 1024,
            StoragePath = "uploads/hero-001.jpg",
            UploadedBy = Guid.NewGuid(),
        });
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _service.PublishAsync(product.Id);

        Assert.Equal(ProductStatus.Published, result.Status);
    }

    [Fact]
    public async Task PublishAsync_WhenInReviewButNotReady_ThrowsBusinessRule()
    {
        // seeded asset is PendingReview → main-image and review-complete checks fail
        var product = await SeedProductAsync(
            status: ProductStatus.InReview, withVariant: true, withAsset: true);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => _service.PublishAsync(product.Id));
    }

    [Theory]
    [InlineData(ProductStatus.Draft)]
    [InlineData(ProductStatus.ReadyToPublish)]
    [InlineData(ProductStatus.Published)]
    [InlineData(ProductStatus.Archived)]
    public async Task PublishAsync_WhenNotInReview_ThrowsConflict(ProductStatus status)
    {
        var product = await SeedProductAsync(status: status);

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.PublishAsync(product.Id));
    }

    [Fact]
    public async Task PublishAsync_WhenIdNotFound_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.PublishAsync(Guid.NewGuid()));
    }

    // ArchiveAsync

    [Fact]
    public async Task ArchiveAsync_WhenPublished_Succeeds()
    {
        var product = await SeedProductAsync(status: ProductStatus.Published);

        var result = await _service.ArchiveAsync(product.Id);

        Assert.Equal(ProductStatus.Archived, result.Status);
    }

    [Fact]
    public async Task ArchiveAsync_WhenAlreadyArchived_ThrowsConflict()
    {
        var product = await SeedProductAsync(status: ProductStatus.Archived);

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.ArchiveAsync(product.Id));
    }

    // UpdateAsync

    [Fact]
    public async Task UpdateAsync_WithValidData_Succeeds()
    {
        var product = await SeedProductAsync();

        var result = await _service.UpdateAsync(product.Id, new UpdateProductRequest
        {
            Name = "Updated Coat Name",
            Brand = "Essentials",
        });

        Assert.Equal("Updated Coat Name", result.Name);
        Assert.Equal("Essentials", result.Brand);
        Assert.Equal("Outerwear", result.Category);
    }

    [Fact]
    public async Task UpdateAsync_WhenArchived_ThrowsConflict()
    {
        var product = await SeedProductAsync(status: ProductStatus.Archived);

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.UpdateAsync(product.Id, new UpdateProductRequest { Name = "New" }));
    }

    // DeleteAsync

    [Fact]
    public async Task DeleteAsync_WhenDraft_Succeeds()
    {
        var product = await SeedProductAsync();

        await _service.DeleteAsync(product.Id);

        var exists = await _context.Products.AnyAsync(p => p.Id == product.Id);
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteAsync_WhenPublished_ThrowsConflict()
    {
        var product = await SeedProductAsync(status: ProductStatus.Published);

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.DeleteAsync(product.Id));
    }

    [Fact]
    public async Task DeleteAsync_WhenIdNotFound_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.DeleteAsync(Guid.NewGuid()));
    }

    // GetAllAsync

    [Fact]
    public async Task GetAllAsync_WithPaging_ReturnsPaginatedResults()
    {
        for (var i = 1; i <= 15; i++)
            await SeedProductAsync(productCode: $"PROD-{i:000}");

        var result = await _service.GetAllAsync(new ProductQueryParams
        {
            Page = 1,
            PageSize = 10,
        });

        Assert.Equal(10, result.Items.Count);
        Assert.Equal(15, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public async Task GetAllAsync_WhenFilteredByBrand_ReturnsMatchingProducts()
    {
        await SeedProductAsync(productCode: "PROD-001", brand: "Heritage");
        await SeedProductAsync(productCode: "PROD-002", brand: "Essentials");
        await SeedProductAsync(productCode: "PROD-003", brand: "Heritage");

        var result = await _service.GetAllAsync(new ProductQueryParams
        {
            Brand = "Heritage",
        });

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, p => Assert.Equal("Heritage", p.Brand));
    }
}