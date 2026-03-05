using System;
using System.Collections.Generic;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Haulory.Infrastructure.Persistence.Repositories;

public class DriverRepository : IDriverRepository
{
    #region Dependencies

    private readonly HauloryDbContext _db;

    #endregion

    #region Constructor

    public DriverRepository(HauloryDbContext db)
    {
        _db = db;
    }

    #endregion

    #region Debug

    // Optional dev helper (no file path anymore)
    public string DebugFilePath => "SQLite: haulory.db";

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

    // ✅ NEW: main driver count (UserId != null)
    public async Task<int> CountMainDriversAsync(Guid ownerUserId)
    {
        if (ownerUserId == Guid.Empty)
            return 0;

        return await _db.Drivers
            .AsNoTracking()
            .CountAsync(d =>
                d.OwnerUserId == ownerUserId &&
                d.UserId.HasValue);
    }

    // ✅ NEW: sub driver count (UserId == null)
    public async Task<int> CountSubDriversAsync(Guid ownerUserId)
    {
        if (ownerUserId == Guid.Empty)
            return 0;

        return await _db.Drivers
            .AsNoTracking()
            .CountAsync(d =>
                d.OwnerUserId == ownerUserId &&
                !d.UserId.HasValue);
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
            driver.Line2,
            driver.Suburb,
            driver.City,
            driver.Region,
            driver.Postcode,
            driver.Country);

        await _db.SaveChangesAsync();
    }

    #endregion
}