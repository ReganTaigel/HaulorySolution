using System.ComponentModel;
using System.Runtime.CompilerServices;
using HaulitCore.Contracts.Drivers;
using HaulitCore.Domain.Entities;

namespace HaulitCore.Mobile.ViewModels;

public class DriverListItem : INotifyPropertyChanged
{
    private bool _isExpanded;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public Driver? Driver { get; }
    public DriverDto? DriverDto { get; }

    public int ExpiringSoonCount { get; }
    public int ExpiredCount { get; }

    public DriverListItem(Driver driver, int expiringSoonCount, int expiredCount = 0)
    {
        Driver = driver;
        ExpiringSoonCount = expiringSoonCount;
        ExpiredCount = expiredCount;
    }

    public DriverListItem(DriverDto driver, int expiringSoonCount = 0, int expiredCount = 0)
    {
        DriverDto = driver;
        ExpiringSoonCount = expiringSoonCount;
        ExpiredCount = expiredCount;
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded == value)
                return;

            _isExpanded = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ExpandIcon));
            OnPropertyChanged(nameof(ExpandText));
        }
    }

    public string ExpandIcon => IsExpanded ? "▲" : "▼";
    public string ExpandText => IsExpanded ? "Hide details" : "Show details";

    public Guid Id => Driver?.Id ?? DriverDto?.Id ?? Guid.Empty;

    public string DisplayName =>
        Driver?.DisplayName ??
        DriverDto?.DisplayName ??
        $"{DriverDto?.FirstName} {DriverDto?.LastName}".Trim();

    public string? Email => Driver?.Email ?? DriverDto?.Email;
    public string? PhoneNumber => Driver?.PhoneNumber ?? DriverDto?.PhoneNumber;
    public string? LicenceNumber => Driver?.LicenceNumber ?? DriverDto?.LicenceNumber;
    public string? LicenceVersion => Driver?.LicenceVersion ?? DriverDto?.LicenceVersion;

    public string? LicenceClassOrEndorsements =>
        Driver?.LicenceClassOrEndorsements ?? DriverDto?.LicenceClassOrEndorsements;

    public DateTime? LicenceIssuedOnUtc =>
        Driver?.LicenceIssuedOnUtc ?? DriverDto?.LicenceIssuedOnUtc;

    public DateTime? LicenceExpiresOnUtc =>
        Driver?.LicenceExpiresOnUtc ?? DriverDto?.LicenceExpiresOnUtc;

    public DateTime? DateOfBirthUtc =>
        Driver?.DateOfBirthUtc ?? DriverDto?.DateOfBirthUtc;

    public string? AddressSummary =>
        Driver?.AddressSummary ?? DriverDto?.AddressSummary;

    public bool IsMainProfile =>
        Driver?.IsMainProfile ?? DriverDto?.IsMainProfile ?? false;

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
            var first = Driver?.EmergencyContact?.FirstName ?? DriverDto?.EmergencyContact?.FirstName;
            var last = Driver?.EmergencyContact?.LastName ?? DriverDto?.EmergencyContact?.LastName;
            return $"{first} {last}".Trim();
        }
    }

    public string? EmergencyContactPhone =>
        Driver?.EmergencyContact?.PhoneNumber ?? DriverDto?.EmergencyContact?.PhoneNumber;

    public string? EmergencyContactEmail =>
        Driver?.EmergencyContact?.Email ?? DriverDto?.EmergencyContact?.Email;
}