namespace ProductCatalogue.Services.Storage;

public record StoredFile(
    string StoragePath,
    string FileName,
    string OriginalFileName,
    string ContentType,
    long FileSize,
    string ResourceType);

public interface IStorageService
{
    Task<StoredFile> SaveAsync(
        IFormFile file,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        StoredFile file,
        CancellationToken cancellationToken = default);

    string GetFileUrl(string fileName);
}