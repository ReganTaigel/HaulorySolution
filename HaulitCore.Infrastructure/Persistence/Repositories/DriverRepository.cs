using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HaulitCore.Application.Interfaces.Repositories;
using HaulitCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HaulitCore.Infrastructure.Persistence.Repositories;

public class DriverRepository : IDriverRepository
{
    #region Dependencies

    private readonly HaulitCoreDbContext _db;

    #endregion

    #region Constructor

    public DriverRepository(HaulitCoreDbContext db)
    {
        _db = db;
    }

    #endregion

    #region Debug

    public string DebugFilePath => "SQLite: HaulitCore.db";

    #endregion

    #region Queries

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

    // ✅ now nullable, no throw
    public async Task<Driver?> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            return null;

        return await _db.Drivers
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    // ✅ tenant-safe single fetch
    public async Task<Driver?> GetByIdForOwnerAsync(Guid ownerUserId, Guid driverId)
    {
        if (ownerUserId == Guid.Empty || driverId == Guid.Empty)
            return null;

        return await _db.Drivers
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.OwnerUserId == ownerUserId && d.Id == driverId);
    }

    public async Task<Driver?> GetByUserIdAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            return null;

        return await _db.Drivers
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.UserId.HasValue && d.UserId.Value == userId);
    }

    public async Task<int> CountMainDriversAsync(Guid ownerUserId)
    {
        if (ownerUserId == Guid.Empty)
            return 0;

        // ✅ Main driver = linked to owner account
        return await _db.Drivers
            .AsNoTracking()
            .CountAsync(d =>
                d.OwnerUserId == ownerUserId &&
                d.UserId.HasValue &&
                d.UserId.Value == ownerUserId);
    }

    public async Task<int> CountSubDriversAsync(Guid ownerUserId)
    {
        if (ownerUserId == Guid.Empty)
            return 0;

        // ✅ Sub driver accounts = linked to a login user that is NOT the owner
        return await _db.Drivers
            .AsNoTracking()
            .CountAsync(d =>
                d.OwnerUserId == ownerUserId &&
                d.UserId.HasValue &&
                d.UserId.Value != ownerUserId);
    }

    #endregion

    #region Persistence

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
            _db.Drivers.Add(driver);
            await _db.SaveChangesAsync();
            return;
        }

        // ✅ OPTIONAL tenant safety check (recommended)
        if (target.OwnerUserId != driver.OwnerUserId)
            throw new InvalidOperationException("Tenant mismatch: cannot save driver for a different OwnerUserId.");

        // Identity fields
        if (!string.IsNullOrWhiteSpace(driver.FirstName) &&
            !string.IsNullOrWhiteSpace(driver.LastName) &&
            !string.IsNullOrWhiteSpace(driver.Email))
        {
            target.UpdateIdentity(driver.FirstName!, driver.LastName!, driver.Email!);
        }

        // Licence
        target.UpdateLicenceNumber(driver.LicenceNumber);
        target.UpdateLicenceVersion(driver.LicenceVersion);
        target.UpdateLicenceClassOrEndorsements(driver.LicenceClassOrEndorsements);
        target.UpdateLicenceIssuedOnUtc(driver.LicenceIssuedOnUtc);
        target.UpdateLicenceExpiryUtc(driver.LicenceExpiresOnUtc);
        target.UpdateLicenceConditionsNotes(driver.LicenceConditionsNotes);

        // Owned type update
        target.UpdateEmergencyContact(driver.EmergencyContact);

        // Profile
        target.UpdatePhone(driver.PhoneNumber);
        target.UpdateDateOfBirthUtc(driver.DateOfBirthUtc);
        target.UpdateLicenceExpiryUtc(driver.LicenceExpiresOnUtc);

        // Address
        target.UpdateAddress(
            driver.Line1,
            driver.Suburb,
            driver.City,
            driver.Region,
            driver.Postcode,
            driver.Country);

        await _db.SaveChangesAsync();
    }

    #endregion
}