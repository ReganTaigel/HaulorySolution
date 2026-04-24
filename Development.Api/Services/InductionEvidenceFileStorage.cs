using HaulitCore.Application.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;

namespace HaulitCore.Api.Services;

// Handles storage of induction evidence files on the local filesystem.
// Implements IInductionEvidenceFileStorage for abstraction and future extensibility (e.g., cloud storage).
public class InductionEvidenceFileStorage : IInductionEvidenceFileStorage
{
    // Provides access to environment paths such as wwwroot.
    private readonly IWebHostEnvironment _environment;

    // Constructor injection of hosting environment.
    public InductionEvidenceFileStorage(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    // Saves an uploaded file stream to disk and returns its relative path and content type.
    public async Task<(string storedRelativePath, string contentType)> SaveAsync(
        Stream fileStream,
        string originalFileName,
        string? contentType,
        CancellationToken cancellationToken = default)
    {
        // Determine the root directory for web-accessible files.
        var webRoot = _environment.WebRootPath;

        // Fallback to a local wwwroot folder if not configured.
        if (string.IsNullOrWhiteSpace(webRoot))
            webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");

        // Build the directory path for induction uploads.
        var uploadsRoot = Path.Combine(webRoot, "uploads", "inductions");

        // Ensure the directory exists.
        Directory.CreateDirectory(uploadsRoot);

        // Extract file extension from original file name.
        var extension = Path.GetExtension(originalFileName);

        // Default to .bin if no extension is provided.
        if (string.IsNullOrWhiteSpace(extension))
            extension = ".bin";

        // Generate a unique file name to avoid collisions.
        var storedFileName = $"{Guid.NewGuid():N}{extension}";

        // Build the full file path.
        var fullPath = Path.Combine(uploadsRoot, storedFileName);

        // Create and write the file to disk.
        await using var target = File.Create(fullPath);
        await fileStream.CopyToAsync(target, cancellationToken);

        // Build a relative path for later retrieval via HTTP.
        var relativePath = $"/uploads/inductions/{storedFileName}";

        // Ensure a safe content type is returned.
        var safeContentType = string.IsNullOrWhiteSpace(contentType)
            ? "application/octet-stream"
            : contentType.Trim();

        // Return stored file path and content type.
        return (relativePath, safeContentType);
    }

    // Deletes a previously stored file based on its relative path.
    public Task DeleteAsync(string? storedRelativePath, CancellationToken cancellationToken = default)
    {
        // Ignore if no path is provided.
        if (string.IsNullOrWhiteSpace(storedRelativePath))
            return Task.CompletedTask;

        // Determine web root path.
        var webRoot = _environment.WebRootPath;

        // Fallback if web root is not configured.
        if (string.IsNullOrWhiteSpace(webRoot))
            webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");

        // Convert relative path to a filesystem-safe path.
        var relative = storedRelativePath.TrimStart('/')
            .Replace('/', Path.DirectorySeparatorChar);

        // Build the full file path.
        var fullPath = Path.Combine(webRoot, relative);

        // Delete file if it exists.
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }
}