using Haulory.Domain.Entities;

namespace Haulory.Application.Interfaces.Repositories;

public interface IDocumentSettingsRepository
{
    Task<DocumentSettings> GetOrCreateAsync(Guid ownerUserId);
    Task SaveAsync(DocumentSettings settings);
}