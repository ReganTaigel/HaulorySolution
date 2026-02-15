using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Haulory.Infrastructure.Persistence.Repositories;

public class DriverRepository : IDriverRepository
{
    private readonly HauloryDbContext _db;

    public DriverRepository(HauloryDbContext db)
    {
        _db = db;
    }

    // Optional dev helper (no file path anymore)
    public string DebugFilePath => "SQLite: haulory.db";

    public async Task<List<Driver>> GetAllAsync()
    {
        return await _db.Drivers
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Driver>> GetAllByOwnerUserIdAsync(Guid ownerUserId)
    {
        if (ownerUserId == Guid.Empty)
            return new List<Driver>();

        return await _db.Drivers
            .AsNoTracking()
            .Where(d => d.OwnerUserId == ownerUserId)
            .ToListAsync();
    }

    public async Task<Driver?> GetByUserIdAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            return null;

        return await _db.Drivers
            .AsNoTracking()
            .FirstOrDefaultAsync(d =>
                d.UserId.HasValue &&
                d.UserId.Value == userId);
    }

    public async Task SaveAsync(Driver driver)
    {
        if (driver == null)
            throw new ArgumentNullException(nameof(driver));

        if (driver.OwnerUserId == Guid.Empty)
            throw new InvalidOperationException("Driver.OwnerUserId must be set before saving.");

        var exists = await _db.Drivers
            .AnyAsync(d => d.Id == driver.Id);

        if (exists)
            _db.Drivers.Update(driver);
        else
            _db.Drivers.Add(driver);

        await _db.SaveChangesAsync();
    }
}
