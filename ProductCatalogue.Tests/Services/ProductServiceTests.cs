using Microsoft.EntityFrameworkCore;
using ProductCatalogue.Data;
using ProductCatalogue.DTOs.Products;
using ProductCatalogue.Models;
using ProductCatalogue.Services;

namespace ProductCatalogue.Tests.Services;

public class ProductServiceTests
{
    private readonly AppDbContext _context;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _service = new ProductService(_context);
    }

    //  seeding helpers 

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

    //  Create 

    [Fact]
    public async Task Create_Product_With_Valid_Data_Succeeds()
    {
        var result = await _service.CreateAsync(ValidCreateRequest());

        Assert.NotNull(result);
        Assert.Equal("Merino Wool Coat", result.Name);
        Assert.Equal("MWC-001", result.ProductCode);
        Assert.Equal(ProductStatus.Draft, result.Status);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task Create_Product_With_Duplicate_Code_Fails()
    {
        await _service.CreateAsync(ValidCreateRequest());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateAsync(ValidCreateRequest()));
    }

    //  GetById 

    [Fact]
    public async Task Get_Product_By_Id_Returns_Product()
    {
        var product = await SeedProductAsync(withVariant: true, withAsset: true);

        var result = await _service.GetByIdAsync(product.Id);

        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
        Assert.Single(result.Variants);
        Assert.Single(result.Assets);
    }

    [Fact]
    public async Task Get_Product_By_Non_Existing_Id_Throws_NotFound()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.GetByIdAsync(Guid.NewGuid()));
    }

    //  Submit for review 

    [Fact]
    public async Task Submit_Draft_Product_With_Variant_And_Asset_Succeeds()
    {
        var product = await SeedProductAsync(withVariant: true, withAsset: true);

        var result = await _service.SubmitForReviewAsync(product.Id);

        Assert.Equal(ProductStatus.InReview, result.Status);
    }

    [Theory]
    [InlineData(false, true)]   // no variants
    [InlineData(true, false)]   // no assets
    [InlineData(false, false)]  // neither
    public async Task Submit_Product_Missing_Requirements_Fails(
        bool withVariant,
        bool withAsset)
    {
        var product = await SeedProductAsync(
            withVariant: withVariant, withAsset: withAsset);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.SubmitForReviewAsync(product.Id));
    }


    [Theory]
    [InlineData(ProductStatus.InReview)]
    [InlineData(ProductStatus.ReadyToPublish)]
    [InlineData(ProductStatus.Published)]
    [InlineData(ProductStatus.Archived)]
    public async Task Submit_Non_Draft_Product_Fails(ProductStatus status)
    {
        var product = await SeedProductAsync(
            status: status, withVariant: true, withAsset: true);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.SubmitForReviewAsync(product.Id));
    }

    //  Publish 

    [Fact]
    public async Task Publish_Ready_Product_Succeeds()
    {
        var product = await SeedProductAsync(status: ProductStatus.ReadyToPublish);

        var result = await _service.PublishAsync(product.Id);

        Assert.Equal(ProductStatus.Published, result.Status);
    }

    [Theory]
    [InlineData(ProductStatus.Draft)]
    [InlineData(ProductStatus.InReview)]
    [InlineData(ProductStatus.Published)]
    [InlineData(ProductStatus.Archived)]
    public async Task Publish_Product_That_Is_Not_Ready_Fails(ProductStatus status)
    {
        var product = await SeedProductAsync(status: status);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.PublishAsync(product.Id));
    }

    [Fact]
    public async Task Publish_Non_Existing_Product_Throws_NotFound()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.PublishAsync(Guid.NewGuid()));
    }

    //  Archive 

    [Fact]
    public async Task Archive_Product_Succeeds()
    {
        var product = await SeedProductAsync(status: ProductStatus.Published);

        var result = await _service.ArchiveAsync(product.Id);

        Assert.Equal(ProductStatus.Archived, result.Status);
    }

    [Fact]
    public async Task Archive_Already_Archived_Product_Fails()
    {
        var product = await SeedProductAsync(status: ProductStatus.Archived);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ArchiveAsync(product.Id));
    }

    //  Update 

    [Fact]
    public async Task Update_Product_Succeeds()
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
    public async Task Update_Archived_Product_Fails()
    {
        var product = await SeedProductAsync(status: ProductStatus.Archived);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateAsync(product.Id, new UpdateProductRequest { Name = "New" }));
    }

    //  Delete 

    [Fact]
    public async Task Delete_Draft_Product_Succeeds()
    {
        var product = await SeedProductAsync();

        await _service.DeleteAsync(product.Id);

        var exists = await _context.Products.AnyAsync(p => p.Id == product.Id);
        Assert.False(exists);
    }

    [Fact]
    public async Task Delete_Published_Product_Fails()
    {
        var product = await SeedProductAsync(status: ProductStatus.Published);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.DeleteAsync(product.Id));
    }

    [Fact]
    public async Task Delete_Non_Existing_Product_Throws_NotFound()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.DeleteAsync(Guid.NewGuid()));
    }

    //  GetAll 

    [Fact]
    public async Task Get_All_Returns_Paginated_Results()
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
    public async Task Get_All_Filters_By_Brand()
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