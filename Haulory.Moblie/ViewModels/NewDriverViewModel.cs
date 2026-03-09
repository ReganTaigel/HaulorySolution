using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Haulory.Application.Interfaces.Services;
using Haulory.Mobile.Contracts.Drivers;
using Haulory.Mobile.Services;
using Microsoft.Maui.Controls;

namespace Haulory.Mobile.ViewModels;

[QueryProperty(nameof(DriverId), "driverId")]
public class NewDriverViewModel : BaseViewModel
{
    private readonly DriversApiService _driversApiService;
    private readonly ISessionService _sessionService;

    private string _driverId = string.Empty;
    private bool _isSaving;

    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _email = string.Empty;
    private string _licenceNumber = string.Empty;

    private string _phoneNumber = string.Empty;

    private DateTime _dateOfBirthLocal = DateTime.Today;
    private DateTime _licenceExpiryLocal = DateTime.Today;

    private string _licenceVersion = string.Empty;
    private string _licenceClassOrEndorsements = string.Empty;
    private string _licenceConditionsNotes = string.Empty;

    private DateTime _licenceIssuedLocal = DateTime.Today;

    private string _line1 = string.Empty;
    private string _line2 = string.Empty;
    private string _suburb = string.Empty;
    private string _city = string.Empty;
    private string _region = string.Empty;
    private string _postcode = string.Empty;
    private string _country = string.Empty;

    private string _ecFirstName = string.Empty;
    private string _ecLastName = string.Empty;
    private string _ecRelationship = string.Empty;
    private string _ecEmail = string.Empty;
    private string _ecPhoneNumber = string.Empty;
    private string _ecSecondaryPhoneNumber = string.Empty;

    private bool _createLoginAccount;
    private string _password = string.Empty;
    public ICommand SaveDriverCommand { get; }

    public NewDriverViewModel(
        DriversApiService driversApiService,
        ISessionService sessionService)
    {
        _driversApiService = driversApiService;
        _sessionService = sessionService;

        SaveDriverCommand = new Command(async () => await ExecuteSaveAsync(), () => CanSave);

        RefreshSaveState();
    }

    public string DriverId
    {
        get => _driverId;
        set => _driverId = value;
    }

    public bool CreateLoginAccount
    {
        get => _createLoginAccount;
        set
        {
            if (_createLoginAccount == value) return;
            _createLoginAccount = value;
            OnPropertyChanged();

            if (!_createLoginAccount && !string.IsNullOrWhiteSpace(Password))
                Password = string.Empty;

            RefreshSaveState();
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged();
            RefreshSaveState();
        }
    }

    public string FirstName
    {
        get => _firstName;
        set { _firstName = value; OnPropertyChanged(); RefreshSaveState(); }
    }

    public string LastName
    {
        get => _lastName;
        set { _lastName = value; OnPropertyChanged(); RefreshSaveState(); }
    }

    public string Email
    {
        get => _email;
        set { _email = value; OnPropertyChanged(); RefreshSaveState(); }
    }

    public string LicenceNumber
    {
        get => _licenceNumber;
        set { _licenceNumber = value; OnPropertyChanged(); RefreshSaveState(); }
    }

    public string PhoneNumber
    {
        get => _phoneNumber;
        set { _phoneNumber = value; OnPropertyChanged(); RefreshSaveState(); }
    }

    public DateTime DateOfBirthLocal
    {
        get => _dateOfBirthLocal;
        set { _dateOfBirthLocal = value; OnPropertyChanged(); }
    }

    public DateTime LicenceExpiryLocal
    {
        get => _licenceExpiryLocal;
        set { _licenceExpiryLocal = value; OnPropertyChanged(); }
    }

    public string LicenceVersion
    {
        get => _licenceVersion;
        set { _licenceVersion = value; OnPropertyChanged(); RefreshSaveState(); }
    }

    public string LicenceClassOrEndorsements
    {
        get => _licenceClassOrEndorsements;
        set { _licenceClassOrEndorsements = value; OnPropertyChanged(); RefreshSaveState(); }
    }

    public string LicenceConditionsNotes
    {
        get => _licenceConditionsNotes;
        set { _licenceConditionsNotes = value; OnPropertyChanged(); RefreshSaveState(); }
    }

    public DateTime LicenceIssuedLocal
    {
        get => _licenceIssuedLocal;
        set { _licenceIssuedLocal = value; OnPropertyChanged(); }
    }

    public string Line1 { get => _line1; set { _line1 = value; OnPropertyChanged(); } }
    public string Line2 { get => _line2; set { _line2 = value; OnPropertyChanged(); } }
    public string Suburb { get => _suburb; set { _suburb = value; OnPropertyChanged(); } }
    public string City { get => _city; set { _city = value; OnPropertyChanged(); } }
    public string Region { get => _region; set { _region = value; OnPropertyChanged(); } }
    public string Postcode { get => _postcode; set { _postcode = value; OnPropertyChanged(); } }
    public string Country { get => _country; set { _country = value; OnPropertyChanged(); } }

    public string EmergencyFirstName
    {
        get => _ecFirstName;
        set { _ecFirstName = value; OnPropertyChanged(); RefreshSaveState(); }
    }

    public string EmergencyLastName
    {
        get => _ecLastName;
        set { _ecLastName = value; OnPropertyChanged(); RefreshSaveState(); }
    }

    public string EmergencyRelationship
    {
        get => _ecRelationship;
        set { _ecRelationship = value; OnPropertyChanged(); RefreshSaveState(); }
    }

    public string EmergencyEmail
    {
        get => _ecEmail;
        set { _ecEmail = value; OnPropertyChanged(); RefreshSaveState(); }
    }

    public string EmergencyPhoneNumber
    {
        get => _ecPhoneNumber;
        set { _ecPhoneNumber = value; OnPropertyChanged(); RefreshSaveState(); }
    }

    public string EmergencySecondaryPhoneNumber
    {
        get => _ecSecondaryPhoneNumber;
        set { _ecSecondaryPhoneNumber = value; OnPropertyChanged(); RefreshSaveState(); }
    }

    public bool CanSave
    {
        get
        {
            if (_isSaving)
                return false;

            if (!_sessionService.IsAuthenticated)
                return false;

            var ownerId = _sessionService.CurrentOwnerId ?? Guid.Empty;
            if (ownerId == Guid.Empty)
                return false;

            if (string.IsNullOrWhiteSpace(FirstName))
                return false;

            if (string.IsNullOrWhiteSpace(LastName))
                return false;

            var email = Email?.Trim();
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                return false;

            if (string.IsNullOrWhiteSpace(EmergencyFirstName))
                return false;

            if (string.IsNullOrWhiteSpace(EmergencyLastName))
                return false;

            if (string.IsNullOrWhiteSpace(EmergencyRelationship))
                return false;

            var ecEmail = EmergencyEmail?.Trim();
            if (string.IsNullOrWhiteSpace(ecEmail) || !ecEmail.Contains('@'))
                return false;

            if (string.IsNullOrWhiteSpace(EmergencyPhoneNumber))
                return false;

            if (CreateLoginAccount && string.IsNullOrWhiteSpace(Password))
                return false;

            return true;
        }
    }



    private async Task ExecuteSaveAsync()
    {

        if (!_sessionService.IsAuthenticated)
            await _sessionService.RestoreAsync();

        if (!CanSave)
        {
            await Shell.Current.DisplayAlertAsync(
                "Cannot save",
                $"Authenticated: {_sessionService.IsAuthenticated}\n" +
                $"OwnerId: {_sessionService.CurrentOwnerId}\n" +
                $"FirstName ok: {!string.IsNullOrWhiteSpace(FirstName)}\n" +
                $"LastName ok: {!string.IsNullOrWhiteSpace(LastName)}\n" +
                $"Email ok: {!string.IsNullOrWhiteSpace(Email) && Email.Contains('@')}\n" +
                $"EmergencyFirstName ok: {!string.IsNullOrWhiteSpace(EmergencyFirstName)}\n" +
                $"EmergencyLastName ok: {!string.IsNullOrWhiteSpace(EmergencyLastName)}\n" +
                $"EmergencyRelationship ok: {!string.IsNullOrWhiteSpace(EmergencyRelationship)}\n" +
                $"EmergencyEmail ok: {!string.IsNullOrWhiteSpace(EmergencyEmail) && EmergencyEmail.Contains('@')}\n" +
                $"EmergencyPhone ok: {!string.IsNullOrWhiteSpace(EmergencyPhoneNumber)}\n" +
                $"Password ok: {!CreateLoginAccount || !string.IsNullOrWhiteSpace(Password)}",
                "OK");
            return;
        }

        try
        {
            _isSaving = true;
            RefreshSaveState();

            var request = new CreateDriverRequest
            {
                FirstName = FirstName.Trim(),
                LastName = LastName.Trim(),
                Email = Email.Trim().ToLowerInvariant(),

                PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber.Trim(),
                DateOfBirthUtc = ToUtcDate(DateOfBirthLocal),

                LicenceNumber = string.IsNullOrWhiteSpace(LicenceNumber) ? null : LicenceNumber.Trim(),
                LicenceVersion = string.IsNullOrWhiteSpace(LicenceVersion) ? null : LicenceVersion.Trim(),
                LicenceClassOrEndorsements = string.IsNullOrWhiteSpace(LicenceClassOrEndorsements) ? null : LicenceClassOrEndorsements.Trim(),
                LicenceIssuedOnUtc = ToUtcDate(LicenceIssuedLocal),
                LicenceExpiresOnUtc = ToUtcDate(LicenceExpiryLocal),
                LicenceConditionsNotes = string.IsNullOrWhiteSpace(LicenceConditionsNotes) ? null : LicenceConditionsNotes.Trim(),

                Line1 = string.IsNullOrWhiteSpace(Line1) ? null : Line1.Trim(),
                Line2 = string.IsNullOrWhiteSpace(Line2) ? null : Line2.Trim(),
                Suburb = string.IsNullOrWhiteSpace(Suburb) ? null : Suburb.Trim(),
                City = string.IsNullOrWhiteSpace(City) ? null : City.Trim(),
                Region = string.IsNullOrWhiteSpace(Region) ? null : Region.Trim(),
                Postcode = string.IsNullOrWhiteSpace(Postcode) ? null : Postcode.Trim(),
                Country = string.IsNullOrWhiteSpace(Country) ? null : Country.Trim(),

                EmergencyContact = new EmergencyContactRequest
                {
                    FirstName = EmergencyFirstName.Trim(),
                    LastName = EmergencyLastName.Trim(),
                    Relationship = EmergencyRelationship.Trim(),
                    Email = EmergencyEmail.Trim().ToLowerInvariant(),
                    PhoneNumber = EmergencyPhoneNumber.Trim(),
                    SecondaryPhoneNumber = string.IsNullOrWhiteSpace(EmergencySecondaryPhoneNumber)
                        ? null
                        : EmergencySecondaryPhoneNumber.Trim()
                },

                CreateLoginAccount = CreateLoginAccount,
                Password = string.IsNullOrWhiteSpace(Password) ? null : Password
            };

            await _driversApiService.CreateDriverAsync(request);

            await Shell.Current.DisplayAlertAsync(
                "Saved",
                CreateLoginAccount ? "Driver + login account created." : "Driver created.",
                "OK");

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Save failed", ex.Message, "OK");
        }
        finally
        {
            _isSaving = false;
            RefreshSaveState();
        }
    }

    private void RefreshSaveState()
    {
        OnPropertyChanged(nameof(CanSave));
        (SaveDriverCommand as Command)?.ChangeCanExecute();
    }

    private static DateTime ToUtcDate(DateTime localDate)
    {
        return DateTime.SpecifyKind(localDate.Date, DateTimeKind.Local).ToUniversalTime();
    }
    public async Task InitializeAsync()
    {
        if (!_sessionService.IsAuthenticated)
            await _sessionService.RestoreAsync();

        OnPropertyChanged(nameof(CanSave));
        (SaveDriverCommand as Command)?.ChangeCanExecute();
    }
}