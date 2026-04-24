using System.Collections.Generic;
using HaulitCore.Domain.Helpers;

namespace HaulitCore.Domain.Entities;

#region Entity: Driver

public class Driver
{
    #region Identity / Ownership

    public Guid Id { get; private set; } = Guid.NewGuid();

    // Owner/main account that owns this driver record (tenant boundary)
    public Guid OwnerUserId { get; private set; }

    // If this driver is linked to a user account (main profile or logged-in driver)
    public Guid? UserId { get; private set; }

    #endregion

    #region Basic Details

    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? Email { get; private set; }

    public string? PhoneNumber { get; private set; }
    public DateTime? DateOfBirthUtc { get; private set; }

    #endregion

    #region Licence

    public string? LicenceNumber { get; private set; }
    public string? LicenceVersion { get; private set; }                 // NEW
    public string? LicenceClassOrEndorsements { get; private set; }     // NEW
    public DateTime? LicenceIssuedOnUtc { get; private set; }           // NEW
    public DateTime? LicenceExpiresOnUtc { get; private set; }          // existing
    public string? LicenceConditionsNotes { get; private set; }         // NEW

    #endregion

    #region Emergency Contact

    public EmergencyContact EmergencyContact { get; private set; } = new EmergencyContact();

    #endregion

    #region Address

    // Address stored directly on Driver (as requested)
    public string? Line1 { get; private set; }

    public string? Suburb { get; private set; }
    public string? City { get; private set; }
    public string? Region { get; private set; }

    public string? Postcode { get; private set; }
    public string? Country { get; private set; }

    #endregion

    #region Status

    public DriverStatus Status { get; private set; } = DriverStatus.Active;

    #endregion

    #region Constructors

    // Required by EF Core
    private Driver() { }

    public Driver(Guid ownerUserId, Guid? userId, string firstName, string lastName, string email)
    {
        OwnerUserId = ownerUserId;
        UserId = userId;

        // Normalize identity fields for consistent display + searching
        FirstName = NameFormatter.ToTitleCase(firstName);
        LastName = NameFormatter.ToTitleCase(lastName);
        Email = CleanEmail(email);
    }

    #endregion

    #region Mutators: Profile

    public void UpdateIdentity(string firstName, string lastName, string email)
    {
        // Keep formatting consistent with constructor
        FirstName = NameFormatter.ToTitleCase(firstName);
        LastName = NameFormatter.ToTitleCase(lastName);
        Email = CleanEmail(email);
    }

    public void UpdatePhone(string? phone)
    {
        PhoneNumber = Clean(phone);
    }

    public void UpdateDateOfBirthUtc(DateTime? dobUtc)
    {
        // Store as UTC kind to avoid timezone ambiguity across devices/services
        DateOfBirthUtc = dobUtc.HasValue
            ? DateTime.SpecifyKind(dobUtc.Value, DateTimeKind.Utc)
            : null;
    }

    #endregion

    #region Mutators: Licence

    public void UpdateLicenceNumber(string? licenceNumber)
    {
        LicenceNumber = Clean(licenceNumber);
    }

    public void UpdateLicenceVersion(string? version)
    {
        LicenceVersion = Clean(version);
    }

    public void UpdateLicenceClassOrEndorsements(string? classOrEndorsements)
    {
        LicenceClassOrEndorsements = Clean(classOrEndorsements);
    }

    public void UpdateLicenceIssuedOnUtc(DateTime? issuedUtc)
    {
        LicenceIssuedOnUtc = issuedUtc.HasValue
            ? DateTime.SpecifyKind(issuedUtc.Value, DateTimeKind.Utc)
            : null;
    }

    public void UpdateLicenceExpiryUtc(DateTime? expiresUtc)
    {
        LicenceExpiresOnUtc = expiresUtc.HasValue
            ? DateTime.SpecifyKind(expiresUtc.Value, DateTimeKind.Utc)
            : null;
    }

    public void UpdateLicenceConditionsNotes(string? notes)
    {
        LicenceConditionsNotes = Clean(notes);
    }

    #endregion

    #region Mutators: Address

    public void UpdateAddress(
        string? line1,
        string? suburb,
        string? city,
        string? region,
        string? postcode,
        string? country)
    {
        Line1 = Clean(line1);
        Suburb = Clean(suburb);
        City = Clean(city);
        Region = Clean(region);
        Postcode = Clean(postcode);
        Country = Clean(country);
    }

    #endregion

    #region Mutators: Emergency Contact

    public void UpdateEmergencyContact(EmergencyContact contact)
    {
        // Maintain a non-null contact reference for simpler consumers
        EmergencyContact = contact ?? new EmergencyContact();
    }

    #endregion

    #region Derived Properties (UI Helpers)

    // Simple display name for lists/pickers
    public string DisplayName => $"{FirstName} {LastName}".Trim();

    // Compact address display for UI and reporting
    public string AddressSummary
    {
        get
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(Line1)) parts.Add(Line1!.Trim());
            if (!string.IsNullOrWhiteSpace(Suburb)) parts.Add(Suburb!.Trim());
            if (!string.IsNullOrWhiteSpace(City)) parts.Add(City!.Trim());
            if (!string.IsNullOrWhiteSpace(Postcode)) parts.Add(Postcode!.Trim());
            if (!string.IsNullOrWhiteSpace(Country)) parts.Add(Country!.Trim());

            return string.Join(", ", parts);
        }
    }

    // Useful for UI validation/status indicators
    public string EmergencyStatus
    {
        get
        {
            var ec = EmergencyContact;
            if (ec == null) return "Emergency contact not set";

            var missing = new List<string>();

            if (string.IsNullOrWhiteSpace(ec.FirstName)) missing.Add("first name");
            if (string.IsNullOrWhiteSpace(ec.LastName)) missing.Add("last name");
            if (string.IsNullOrWhiteSpace(ec.Relationship)) missing.Add("relationship");
            if (string.IsNullOrWhiteSpace(ec.PhoneNumber)) missing.Add("phone");
            if (string.IsNullOrWhiteSpace(ec.Email)) missing.Add("email");

            return missing.Count == 0
                ? "Emergency contact set"
                : $"Missing: {string.Join(", ", missing)}";
        }
    }

    // True when this driver is linked to a user account
    public bool IsMainProfile => UserId.HasValue;

    #endregion

    #region Ownership Safety

    public void EnsureOwner(Guid ownerUserId)
    {
        // Used for backfilling ownership safely when needed
        if (ownerUserId == Guid.Empty) return;

        if (OwnerUserId == Guid.Empty)
            OwnerUserId = ownerUserId;
    }

    #endregion

    #region Helpers

    private static string? Clean(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static string? CleanEmail(string? email)
    {
        // Email should always be trimmed + lower for consistent login/search behavior
        var cleaned = Clean(email);
        return cleaned?.ToLowerInvariant();
    }

    #endregion
}

#endregion