using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using ProductCatalogue.Settings;

namespace ProductCatalogue.Services.Storage;

public class CloudinaryStorageService : IStorageService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryStorageService(IOptions<CloudinarySettings> settings)
    {
        var config = settings.Value;
        var account = new Account(config.CloudName, config.ApiKey, config.ApiSecret);
        _cloudinary = new Cloudinary(account) {Api = {Secure = true}};
    }

    public async Task<StoredFile> SaveAsync(IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();

        var uploadParams = new AutoUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = "product-catalogue",
            UseFilename = false,
            UniqueFilename = true,
        };

        var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

        if(result.Error is not null)
            throw new InvalidOperationException($"Cloudinary upload failed: {result.Error.Message}");

        return new StoredFile(
            StoragePath: result.PublicId,
            FileName: result.PublicId,
            OriginalFileName: file.FileName,
            ContentType: file.ContentType,
            FileSize: file.Length,
            ResourceType: result.ResourceType ?? "image"
        );
    }

    public async Task DeleteAsync(
    StoredFile file,
    CancellationToken cancellationToken = default)
    {
        var resourceType = file.ResourceType == "raw"
            ? ResourceType.Raw
            : ResourceType.Image;

        var deleteParams = new DeletionParams(file.StoragePath)
        {
            ResourceType = resourceType,
        };

        var result = await _cloudinary.DestroyAsync(deleteParams);
        if (result.Error is not null)
            throw new InvalidOperationException($"Cloudinary delete failed: {result.Error.Message}");
    }

    public string GetFileUrl(string fileName)
    {
        return _cloudinary.Api.UrlImgUp.BuildUrl(fileName);
    }
}