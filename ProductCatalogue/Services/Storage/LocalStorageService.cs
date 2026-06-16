namespace ProductCatalogue.Services.Storage;

public class LocalStorageService : IStorageService
{
    private readonly string _uploadsRoot;
    private readonly string _publicBaseUrl;

    public LocalStorageService(IWebHostEnvironment env, IConfiguration configuration)
    {
        // files live under wwwroot/uploads
        _uploadsRoot = Path.Combine(env.WebRootPath ?? "wwwroot", "uploads");
        Directory.CreateDirectory(_uploadsRoot);

        // e.g. "http://localhost:5213" from config, used to build absolute URLs
        _publicBaseUrl = configuration["Storage:PublicBaseUrl"] ?? string.Empty;
    }

    public async Task<StoredFile> SaveAsync(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        // unique stored name, preserve the original extension
        var extension = Path.GetExtension(file.FileName);
        var storedName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(_uploadsRoot, storedName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream, cancellationToken);

        return new StoredFile(
            StoragePath: fullPath,
            FileName: storedName,
            OriginalFileName: file.FileName,
            ContentType: file.ContentType,
            FileSize: file.Length);
    }

    public Task DeleteAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        if (File.Exists(storagePath))
            File.Delete(storagePath);

        return Task.CompletedTask;
    }

    public string GetFileUrl(string fileName)
    {
        return $"{_publicBaseUrl}/uploads/{fileName}";
    }
}