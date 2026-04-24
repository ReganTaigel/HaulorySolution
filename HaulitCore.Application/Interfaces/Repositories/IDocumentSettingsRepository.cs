using HaulitCore.Domain.Entities;

namespace HaulitCore.Application.Interfaces.Repositories;

public interface IDocumentSettingsRepository
{
    Task<DocumentSettings> GetOrCreateAsync(Guid ownerUserId);
    Task SaveAsync(DocumentSettings settings);
}