namespace ProductCatalogue.Services.Storage;

public record StoredFile(
    string StoragePath,
    string FileName,
    string OriginalFileName,
    string ContentType,
    long FileSize);

public interface IStorageService
{
    Task<StoredFile> SaveAsync(
        IFormFile file,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string storagePath,
        CancellationToken cancellationToken = default);

    string GetFileUrl(string fileName);
}