using Haulory.Application.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;

namespace Haulory.Api.Services;

public class InductionEvidenceFileStorage : IInductionEvidenceFileStorage
{
    private readonly IWebHostEnvironment _environment;

    public InductionEvidenceFileStorage(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<(string storedRelativePath, string contentType)> SaveAsync(
        Stream fileStream,
        string originalFileName,
        string? contentType,
        CancellationToken cancellationToken = default)
    {
        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
            webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");

        var uploadsRoot = Path.Combine(webRoot, "uploads", "inductions");
        Directory.CreateDirectory(uploadsRoot);

        var extension = Path.GetExtension(originalFileName);
        if (string.IsNullOrWhiteSpace(extension))
            extension = ".bin";

        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(uploadsRoot, storedFileName);

        await using var target = File.Create(fullPath);
        await fileStream.CopyToAsync(target, cancellationToken);

        var relativePath = $"/uploads/inductions/{storedFileName}";
        var safeContentType = string.IsNullOrWhiteSpace(contentType)
            ? "application/octet-stream"
            : contentType.Trim();

        return (relativePath, safeContentType);
    }

    public Task DeleteAsync(string? storedRelativePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storedRelativePath))
            return Task.CompletedTask;

        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
            webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");

        var relative = storedRelativePath.TrimStart('/')
            .Replace('/', Path.DirectorySeparatorChar);

        var fullPath = Path.Combine(webRoot, relative);

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }
}