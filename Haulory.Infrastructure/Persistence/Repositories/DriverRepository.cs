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

    public async Task<Driver> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Driver Id cannot be empty.", nameof(id));

        var driver = await _db.Drivers
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);

        if (driver == null)
            throw new KeyNotFoundException($"Driver not found with Id: {id}");

        return driver;
    }


    public async Task SaveAsync(Driver driver)
    {
        if (driver == null)
            throw new ArgumentNullException(nameof(driver));

        if (driver.OwnerUserId == Guid.Empty)
            throw new InvalidOperationException("Driver.OwnerUserId must be set before saving.");

        // Try tracked first
        var target = _db.Drivers.Local.FirstOrDefault(d => d.Id == driver.Id);

        // If not tracked, load tracked entity from DB (no AsNoTracking here)
        if (target == null)
            target = await _db.Drivers.FirstOrDefaultAsync(d => d.Id == driver.Id);

        if (target == null)
        {
            // New driver
            _db.Drivers.Add(driver);
            await _db.SaveChangesAsync();
            return;
        }

        // Update scalars explicitly (safe + predictable)
        // Identity fields
        if (!string.IsNullOrWhiteSpace(driver.FirstName) &&
            !string.IsNullOrWhiteSpace(driver.LastName) &&
            !string.IsNullOrWhiteSpace(driver.Email))
        {
            target.UpdateIdentity(driver.FirstName!, driver.LastName!, driver.Email!);
        }

        // Optional fields
        target.UpdateLicenceNumber(driver.LicenceNumber);

        // Owned type update (important)
        target.UpdateEmergencyContact(driver.EmergencyContact);

        // Status if you allow changes (optional)
        // target.SetStatus(driver.Status); // if you add a method
        target.UpdatePhone(driver.PhoneNumber);
        target.UpdateDateOfBirthUtc(driver.DateOfBirthUtc);
        target.UpdateLicenceExpiryUtc(driver.LicenceExpiresOnUtc);
        target.UpdateAddress(
                driver.Line1,
                driver.Line2,
                driver.Suburb,
                driver.City,
                driver.Region,
                driver.Postcode,
                driver.Country);


        await _db.SaveChangesAsync();
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


}
