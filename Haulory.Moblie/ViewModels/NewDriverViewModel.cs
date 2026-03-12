using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Haulory.Application.Interfaces.Services;
using Haulory.Mobile.Contracts.Drivers;
using Haulory.Mobile.Features;
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
        ISessionService sessionService,
        IFeatureAccessService featureAccessService)
        : base(featureAccessService)
    {
        _driversApiService = driversApiService;
        _sessionService = sessionService;

        SaveDriverCommand = new Command(async () => await ExecuteSaveAsync(), () => CanSaveDriverAction);

        RefreshSaveState();
    }
    public bool IsEditMode => Guid.TryParse(DriverId, out var id) && id != Guid.Empty;
    public string PageTitle => IsEditMode ? "Edit driver" : "New driver";
    public string SaveButtonText => IsEditMode ? "Update driver" : "Save driver";
    public bool CanCreateLoginAccount => !IsEditMode;
    public string DriverId
    {
        get => _driverId;
        set => SetProperty(ref _driverId, value);
    }

    public bool IsMainUser =>
        _sessionService.CurrentAccountId.HasValue &&
        _sessionService.CurrentOwnerId.HasValue &&
        _sessionService.CurrentAccountId.Value == _sessionService.CurrentOwnerId.Value;

    public bool IsAddDriverVisible => IsFeatureVisible(AppFeature.AddDriver) && IsMainUser;
    public bool IsAddDriverEnabled => IsFeatureEnabled(AppFeature.AddDriver) && IsMainUser;

    public bool CreateLoginAccount
    {
        get => _createLoginAccount;
        set
        {
            if (!SetProperty(ref _createLoginAccount, value))
                return;

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
            if (!SetProperty(ref _password, value))
                return;

            RefreshSaveState();
        }
    }

    public string FirstName
    {
        get => _firstName;
        set
        {
            if (!SetProperty(ref _firstName, value))
                return;

            RefreshSaveState();
        }
    }

    public string LastName
    {
        get => _lastName;
        set
        {
            if (!SetProperty(ref _lastName, value))
                return;

            RefreshSaveState();
        }
    }

    public string Email
    {
        get => _email;
        set
        {
            if (!SetProperty(ref _email, value))
                return;

            RefreshSaveState();
        }
    }

    public string LicenceNumber
    {
        get => _licenceNumber;
        set
        {
            if (!SetProperty(ref _licenceNumber, value))
                return;

            RefreshSaveState();
        }
    }

    public string PhoneNumber
    {
        get => _phoneNumber;
        set
        {
            if (!SetProperty(ref _phoneNumber, value))
                return;

            RefreshSaveState();
        }
    }

    public DateTime DateOfBirthLocal
    {
        get => _dateOfBirthLocal;
        set => SetProperty(ref _dateOfBirthLocal, value);
    }

    public DateTime LicenceExpiryLocal
    {
        get => _licenceExpiryLocal;
        set => SetProperty(ref _licenceExpiryLocal, value);
    }

    public string LicenceVersion
    {
        get => _licenceVersion;
        set
        {
            if (!SetProperty(ref _licenceVersion, value))
                return;

            RefreshSaveState();
        }
    }

    public string LicenceClassOrEndorsements
    {
        get => _licenceClassOrEndorsements;
        set
        {
            if (!SetProperty(ref _licenceClassOrEndorsements, value))
                return;

            RefreshSaveState();
        }
    }

    public string LicenceConditionsNotes
    {
        get => _licenceConditionsNotes;
        set
        {
            if (!SetProperty(ref _licenceConditionsNotes, value))
                return;

            RefreshSaveState();
        }
    }

    public DateTime LicenceIssuedLocal
    {
        get => _licenceIssuedLocal;
        set => SetProperty(ref _licenceIssuedLocal, value);
    }

    public string Line1
    {
        get => _line1;
        set => SetProperty(ref _line1, value);
    }

    public string Line2
    {
        get => _line2;
        set => SetProperty(ref _line2, value);
    }

    public string Suburb
    {
        get => _suburb;
        set => SetProperty(ref _suburb, value);
    }

    public string City
    {
        get => _city;
        set => SetProperty(ref _city, value);
    }

    public string Region
    {
        get => _region;
        set => SetProperty(ref _region, value);
    }

    public string Postcode
    {
        get => _postcode;
        set => SetProperty(ref _postcode, value);
    }

    public string Country
    {
        get => _country;
        set => SetProperty(ref _country, value);
    }

    public string EmergencyFirstName
    {
        get => _ecFirstName;
        set
        {
            if (!SetProperty(ref _ecFirstName, value))
                return;

            RefreshSaveState();
        }
    }

    public string EmergencyLastName
    {
        get => _ecLastName;
        set
        {
            if (!SetProperty(ref _ecLastName, value))
                return;

            RefreshSaveState();
        }
    }

    public string EmergencyRelationship
    {
        get => _ecRelationship;
        set
        {
            if (!SetProperty(ref _ecRelationship, value))
                return;

            RefreshSaveState();
        }
    }

    public string EmergencyEmail
    {
        get => _ecEmail;
        set
        {
            if (!SetProperty(ref _ecEmail, value))
                return;

            RefreshSaveState();
        }
    }

    public string EmergencyPhoneNumber
    {
        get => _ecPhoneNumber;
        set
        {
            if (!SetProperty(ref _ecPhoneNumber, value))
                return;

            RefreshSaveState();
        }
    }

    public string EmergencySecondaryPhoneNumber
    {
        get => _ecSecondaryPhoneNumber;
        set
        {
            if (!SetProperty(ref _ecSecondaryPhoneNumber, value))
                return;

            RefreshSaveState();
        }
    }

    public bool CanSave
    {
        get
        {
            if (_isSaving)
                return false;

            if (!IsFeatureEnabled(AppFeature.AddDriver))
                return false;

            if (!IsMainUser)
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

    public bool CanSaveDriverAction => IsAddDriverEnabled && CanSave;

    private async Task ExecuteSaveAsync()
    {
        if (!await EnsureFeatureEnabledAsync(AppFeature.AddDriver))
            return;

        if (!_sessionService.IsAuthenticated)
            await _sessionService.RestoreAsync();

        if (!CanSave)
        {
            await Shell.Current.DisplayAlertAsync(
                "Cannot save",
                "Please complete all required fields before saving.",
                "OK");
            return;
        }

        try
        {
            _isSaving = true;
            RefreshSaveState();

            if (IsEditMode && Guid.TryParse(DriverId, out var driverId))
            {
                var request = new UpdateDriverRequest
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
                    }
                };

                await _driversApiService.UpdateDriverAsync(driverId, request);

                await Shell.Current.DisplayAlertAsync("Saved", "Driver updated.", "OK");
            }
            else
            {
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
            }

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
        OnPropertyChanged(nameof(IsMainUser));
        OnPropertyChanged(nameof(CanSave));
        OnPropertyChanged(nameof(IsAddDriverVisible));
        OnPropertyChanged(nameof(IsAddDriverEnabled));
        OnPropertyChanged(nameof(CanSaveDriverAction));
        OnPropertyChanged(nameof(IsEditMode));
        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(SaveButtonText));
        OnPropertyChanged(nameof(CanCreateLoginAccount));
        (SaveDriverCommand as Command)?.ChangeCanExecute();
    }

    private static DateTime ToUtcDate(DateTime localDate)
    {
        return DateTime.SpecifyKind(localDate.Date, DateTimeKind.Local).ToUniversalTime();
    }
    private async Task LoadExistingDriverAsync(Guid driverId)
    {
        var driver = await _driversApiService.GetDriverByIdAsync(driverId);
        if (driver == null)
        {
            await Shell.Current.DisplayAlertAsync("Not found", "Driver could not be loaded.", "OK");
            return;
        }

        FirstName = driver.FirstName ?? string.Empty;
        LastName = driver.LastName ?? string.Empty;
        Email = driver.Email ?? string.Empty;
        PhoneNumber = driver.PhoneNumber ?? string.Empty;

        if (driver.DateOfBirthUtc.HasValue)
            DateOfBirthLocal = driver.DateOfBirthUtc.Value.ToLocalTime().Date;

        LicenceNumber = driver.LicenceNumber ?? string.Empty;
        LicenceVersion = driver.LicenceVersion ?? string.Empty;
        LicenceClassOrEndorsements = driver.LicenceClassOrEndorsements ?? string.Empty;
        LicenceConditionsNotes = driver.LicenceConditionsNotes ?? string.Empty;

        if (driver.LicenceIssuedOnUtc.HasValue)
            LicenceIssuedLocal = driver.LicenceIssuedOnUtc.Value.ToLocalTime().Date;

        if (driver.LicenceExpiresOnUtc.HasValue)
            LicenceExpiryLocal = driver.LicenceExpiresOnUtc.Value.ToLocalTime().Date;

        Line1 = driver.Line1 ?? string.Empty;
        Line2 = driver.Line2 ?? string.Empty;
        Suburb = driver.Suburb ?? string.Empty;
        City = driver.City ?? string.Empty;
        Region = driver.Region ?? string.Empty;
        Postcode = driver.Postcode ?? string.Empty;
        Country = driver.Country ?? string.Empty;

        EmergencyFirstName = driver.EmergencyContact?.FirstName ?? string.Empty;
        EmergencyLastName = driver.EmergencyContact?.LastName ?? string.Empty;
        EmergencyRelationship = driver.EmergencyContact?.Relationship ?? string.Empty;
        EmergencyEmail = driver.EmergencyContact?.Email ?? string.Empty;
        EmergencyPhoneNumber = driver.EmergencyContact?.PhoneNumber ?? string.Empty;
        EmergencySecondaryPhoneNumber = driver.EmergencyContact?.SecondaryPhoneNumber ?? string.Empty;

        CreateLoginAccount = false;
        Password = string.Empty;
    }

    public async Task InitializeAsync()
    {
        if (!_sessionService.IsAuthenticated)
            await _sessionService.RestoreAsync();

        if (IsEditMode && Guid.TryParse(DriverId, out var driverId))
            await LoadExistingDriverAsync(driverId);

        OnPropertyChanged(nameof(IsMainUser));
        OnPropertyChanged(nameof(CanSave));
        OnPropertyChanged(nameof(IsAddDriverVisible));
        OnPropertyChanged(nameof(IsAddDriverEnabled));
        OnPropertyChanged(nameof(CanSaveDriverAction));
        OnPropertyChanged(nameof(IsEditMode));
        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(SaveButtonText));
        OnPropertyChanged(nameof(CanCreateLoginAccount));
        (SaveDriverCommand as Command)?.ChangeCanExecute();
    }
}