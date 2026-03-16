using Haulory.Domain.Entities;
using Haulory.Contracts.Drivers;

namespace Haulory.Mobile.ViewModels;

#region ViewModel Helper: Driver List Item

public class DriverListItem
{
    #region Data

    // Domain driver (used by legacy/local flows)
    public Driver? Driver { get; }

    // API driver (used by cloud flows)
    public DriverDto? DriverDto { get; }

    // Number of inductions expiring soon
    public int ExpiringSoonCount { get; }

    // Number of expired inductions
    public int ExpiredCount { get; }

    #endregion

    #region Constructors

    // Legacy/local constructor
    public DriverListItem(
        Driver driver,
        int expiringSoonCount,
        int expiredCount = 0)
    {
        Driver = driver;
        ExpiringSoonCount = expiringSoonCount;
        ExpiredCount = expiredCount;
    }

    // API constructor
    public DriverListItem(
        DriverDto driver,
        int expiringSoonCount = 0,
        int expiredCount = 0)
    {
        DriverDto = driver;
        ExpiringSoonCount = expiringSoonCount;
        ExpiredCount = expiredCount;
    }

    #endregion

    #region Derived Properties

    public Guid Id =>
        Driver?.Id ??
        DriverDto?.Id ??
        Guid.Empty;

    public string DisplayName =>
        Driver?.DisplayName ??
        DriverDto?.DisplayName ??
        $"{DriverDto?.FirstName} {DriverDto?.LastName}".Trim();

    public string? Email =>
        Driver?.Email ??
        DriverDto?.Email;

    public string? PhoneNumber =>
        Driver?.PhoneNumber ??
        DriverDto?.PhoneNumber;

    public string? LicenceNumber =>
        Driver?.LicenceNumber ??
        DriverDto?.LicenceNumber;

    public string? LicenceVersion =>
        Driver?.LicenceVersion ??
        DriverDto?.LicenceVersion;

    public string? LicenceClassOrEndorsements =>
        Driver?.LicenceClassOrEndorsements ??
        DriverDto?.LicenceClassOrEndorsements;

    public DateTime? LicenceIssuedOnUtc =>
        Driver?.LicenceIssuedOnUtc ??
        DriverDto?.LicenceIssuedOnUtc;

    public DateTime? LicenceExpiresOnUtc =>
        Driver?.LicenceExpiresOnUtc ??
        DriverDto?.LicenceExpiresOnUtc;

    public DateTime? DateOfBirthUtc =>
        Driver?.DateOfBirthUtc ??
        DriverDto?.DateOfBirthUtc;

    public string? AddressSummary =>
        Driver?.AddressSummary ??
        DriverDto?.AddressSummary;

    public string EmergencyStatus =>
        Driver?.EmergencyStatus ??
        DriverDto?.EmergencyStatus ??
        string.Empty;

    public bool IsMainProfile =>
        Driver?.IsMainProfile ??
        DriverDto?.IsMainProfile ??
        false;

    public bool HasEmergencyContact
    {
        get
        {
            if (Driver?.EmergencyContact != null)
            {
                return
                    !string.IsNullOrWhiteSpace(Driver.EmergencyContact.FirstName) ||
                    !string.IsNullOrWhiteSpace(Driver.EmergencyContact.LastName) ||
                    !string.IsNullOrWhiteSpace(Driver.EmergencyContact.PhoneNumber) ||
                    !string.IsNullOrWhiteSpace(Driver.EmergencyContact.Email);
            }

            if (DriverDto?.EmergencyContact != null)
            {
                return
                    !string.IsNullOrWhiteSpace(DriverDto.EmergencyContact.FirstName) ||
                    !string.IsNullOrWhiteSpace(DriverDto.EmergencyContact.LastName) ||
                    !string.IsNullOrWhiteSpace(DriverDto.EmergencyContact.PhoneNumber) ||
                    !string.IsNullOrWhiteSpace(DriverDto.EmergencyContact.Email);
            }

            return false;
        }
    }

    public string EmergencyContactName
    {
        get
        {
            var first =
                Driver?.EmergencyContact?.FirstName ??
                DriverDto?.EmergencyContact?.FirstName;

            var last =
                Driver?.EmergencyContact?.LastName ??
                DriverDto?.EmergencyContact?.LastName;

            return $"{first} {last}".Trim();
        }
    }

    public string? EmergencyContactPhone =>
        Driver?.EmergencyContact?.PhoneNumber ??
        DriverDto?.EmergencyContact?.PhoneNumber;

    public string? EmergencyContactEmail =>
        Driver?.EmergencyContact?.Email ??
        DriverDto?.EmergencyContact?.Email;

    // True if driver has any compliance warnings
    public bool HasWarnings =>
        ExpiringSoonCount > 0 || ExpiredCount > 0;

    // Warning summary text for UI badges
    public string WarningSummary
    {
        get
        {
            if (!HasWarnings)
                return string.Empty;

            if (ExpiredCount > 0 && ExpiringSoonCount > 0)
                return $"{ExpiredCount} expired • {ExpiringSoonCount} due soon";

            if (ExpiredCount > 0)
                return $"{ExpiredCount} expired";

            return $"{ExpiringSoonCount} due soon";
        }
    }

    #endregion
}

#endregion