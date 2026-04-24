using HaulitCore.Application.Interfaces.Repositories;
using HaulitCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HaulitCore.Infrastructure.Persistence.Repositories;

public class DocumentSettingsRepository : IDocumentSettingsRepository
{
    private readonly HaulitCoreDbContext _db;

    public DocumentSettingsRepository(HaulitCoreDbContext db)
    {
        _db = db;
    }

    public async Task<DocumentSettings> GetOrCreateAsync(Guid ownerUserId)
    {
        var settings = await _db.DocumentSettings
            .FirstOrDefaultAsync(x => x.OwnerUserId == ownerUserId);

        if (settings != null)
            return settings;

        settings = new DocumentSettings(ownerUserId);
        _db.DocumentSettings.Add(settings);
        await _db.SaveChangesAsync();

        return settings;
    }

    public async Task SaveAsync(DocumentSettings settings)
    {
        _db.DocumentSettings.Update(settings);
        await _db.SaveChangesAsync();
    }
}