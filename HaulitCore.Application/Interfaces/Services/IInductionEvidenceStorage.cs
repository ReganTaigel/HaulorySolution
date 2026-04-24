namespace HaulitCore.Application.Interfaces.Services;

public interface IInductionEvidenceFileStorage
{
    Task<(string storedRelativePath, string contentType)> SaveAsync(
        Stream fileStream,
        string originalFileName,
        string? contentType,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string? storedRelativePath, CancellationToken cancellationToken = default);
}