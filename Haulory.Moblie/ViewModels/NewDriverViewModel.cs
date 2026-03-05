using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Haulory.Application.Features.Drivers;
using Haulory.Application.Interfaces.Services;
using Microsoft.Maui.Controls;

namespace Haulory.Mobile.ViewModels;

[QueryProperty(nameof(DriverId), "driverId")]
public class NewDriverViewModel : BaseViewModel
{
    #region Dependencies

    private readonly CreateDriverHandler _createDriverHandler;
    private readonly ISessionService _sessionService;

    #endregion

    #region State

    private string _driverId = string.Empty;
    private bool _isSaving;

    #endregion

    #region Driver Fields

    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _email = string.Empty;
    private string _licenceNumber = string.Empty;

    #endregion

    #region Contact + Licence + Address

    private string _phoneNumber = string.Empty;

    // DatePickers require a value; default to Today.
    // If you want true optional later, add HasDob/HasLicenceExpiry toggles.
    private DateTime _dateOfBirthLocal = DateTime.Today;
    private DateTime _licenceExpiryLocal = DateTime.Today;

    // NEW licence fields
    private string _licenceVersion = string.Empty;
    private string _licenceClassOrEndorsements = string.Empty;
    private string _licenceConditionsNotes = string.Empty;

    // DatePicker pattern (if you want it optional later, use a toggle)
    private DateTime _licenceIssuedLocal = DateTime.Today;

    private string _line1 = string.Empty;
    private string _line2 = string.Empty;
    private string _suburb = string.Empty;
    private string _city = string.Empty;
    private string _region = string.Empty;
    private string _postcode = string.Empty;
    private string _country = string.Empty;

    #endregion

    #region Emergency Contact (Required)

    private string _ecFirstName = string.Empty;
    private string _ecLastName = string.Empty;
    private string _ecRelationship = string.Empty;
    private string _ecEmail = string.Empty;
    private string _ecPhoneNumber = string.Empty;
    private string _ecSecondaryPhoneNumber = string.Empty;

    #endregion

    #region Query Properties

    public string DriverId
    {
        get => _driverId;
        set => _driverId = value;
    }

    #endregion

    #region Bindable Properties (Driver)

    public string FirstName
    {
        get => _firstName;
        set
        {
            _firstName = value;
            OnPropertyChanged();
            RefreshSaveState();
        }
    }

    public string LastName
    {
        get => _lastName;
        set
        {
            _lastName = value;
            OnPropertyChanged();
            RefreshSaveState();
        }
    }

    public string Email
    {
        get => _email;
        set
        {
            _email = value;
            OnPropertyChanged();
            RefreshSaveState();
        }
    }

    public string LicenceNumber
    {
        get => _licenceNumber;
        set
        {
            _licenceNumber = value;
            OnPropertyChanged();
            RefreshSaveState();
        }
    }

    #endregion

    #region Bindable Properties (Contact + Licence + Address)

    public string PhoneNumber
    {
        get => _phoneNumber;
        set
        {
            _phoneNumber = value;
            OnPropertyChanged();
            RefreshSaveState();
        }
    }

    public DateTime DateOfBirthLocal
    {
        get => _dateOfBirthLocal;
        set
        {
            _dateOfBirthLocal = value;
            OnPropertyChanged();
        }
    }

    public DateTime LicenceExpiryLocal
    {
        get => _licenceExpiryLocal;
        set
        {
            _licenceExpiryLocal = value;
            OnPropertyChanged();
        }
    }

    public string LicenceVersion
    {
        get => _licenceVersion;
        set
        {
            _licenceVersion = value;
            OnPropertyChanged();
            RefreshSaveState();
        }
    }

    public string LicenceClassOrEndorsements
    {
        get => _licenceClassOrEndorsements;
        set
        {
            _licenceClassOrEndorsements = value;
            OnPropertyChanged();
            RefreshSaveState();
        }
    }

    public string LicenceConditionsNotes
    {
        get => _licenceConditionsNotes;
        set
        {
            _licenceConditionsNotes = value;
            OnPropertyChanged();
            RefreshSaveState();
        }
    }

    public DateTime LicenceIssuedLocal
    {
        get => _licenceIssuedLocal;
        set
        {
            _licenceIssuedLocal = value;
            OnPropertyChanged();
        }
    }

    public string Line1 { get => _line1; set { _line1 = value; OnPropertyChanged(); } }
    public string Line2 { get => _line2; set { _line2 = value; OnPropertyChanged(); } }
    public string Suburb { get => _suburb; set { _suburb = value; OnPropertyChanged(); } }
    public string City { get => _city; set { _city = value; OnPropertyChanged(); } }
    public string Region { get => _region; set { _region = value; OnPropertyChanged(); } }
    public string Postcode { get => _postcode; set { _postcode = value; OnPropertyChanged(); } }
    public string Country { get => _country; set { _country = value; OnPropertyChanged(); } }

    #endregion

    #region Bindable Properties (Emergency Contact)

    public string EmergencyFirstName
    {
        get => _ecFirstName;
        set
        {
            _ecFirstName = value;
            OnPropertyChanged();
            RefreshSaveState();
        }
    }

    public string EmergencyLastName
    {
        get => _ecLastName;
        set
        {
            _ecLastName = value;
            OnPropertyChanged();
            RefreshSaveState();
        }
    }

    public string EmergencyRelationship
    {
        get => _ecRelationship;
        set
        {
            _ecRelationship = value;
            OnPropertyChanged();
            RefreshSaveState();
        }
    }

    public string EmergencyEmail
    {
        get => _ecEmail;
        set
        {
            _ecEmail = value;
            OnPropertyChanged();
            RefreshSaveState();
        }
    }

    public string EmergencyPhoneNumber
    {
        get => _ecPhoneNumber;
        set
        {
            _ecPhoneNumber = value;
            OnPropertyChanged();
            RefreshSaveState();
        }
    }

    public string EmergencySecondaryPhoneNumber
    {
        get => _ecSecondaryPhoneNumber;
        set
        {
            _ecSecondaryPhoneNumber = value;
            OnPropertyChanged();
            RefreshSaveState();
        }
    }

    #endregion

    #region Save Gate

    public bool CanSave
    {
        get
        {
            if (_isSaving)
                return false;

            // Must be logged in
            if (!_sessionService.IsAuthenticated)
                return false;

            var ownerId = _sessionService.CurrentAccountId ?? Guid.Empty;
            if (ownerId == Guid.Empty)
                return false;

            // Driver identity fields
            if (string.IsNullOrWhiteSpace(FirstName))
                return false;

            if (string.IsNullOrWhiteSpace(LastName))
                return false;

            var email = Email?.Trim();
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                return false;

            // Emergency contact (required)
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

            return true;
        }
    }

    #endregion

    #region Commands

    public ICommand SaveDriverCommand { get; }

    #endregion

    #region Constructor

    public NewDriverViewModel(
        CreateDriverHandler createDriverHandler,
        ISessionService sessionService)
    {
        _createDriverHandler = createDriverHandler;
        _sessionService = sessionService;

        SaveDriverCommand = new Command(async () => await ExecuteSaveAsync(), () => CanSave);

        RefreshSaveState();
    }

    #endregion

    #region Save Logic

    private async Task ExecuteSaveAsync()
    {
        if (!CanSave)
            return;

        try
        {
            _isSaving = true;
            RefreshSaveState();

            var ownerId = _sessionService.CurrentAccountId!.Value;

            // Convert date-only Local -> UTC (date-only comparisons work fine)
            var dobUtc = DateTime.SpecifyKind(DateOfBirthLocal.Date, DateTimeKind.Local).ToUniversalTime();
            var licenceExpUtc = DateTime.SpecifyKind(LicenceExpiryLocal.Date, DateTimeKind.Local).ToUniversalTime();

            // NEW: issued date Local -> UTC
            var issuedUtc = DateTime.SpecifyKind(LicenceIssuedLocal.Date, DateTimeKind.Local).ToUniversalTime();

            var cmd = new CreateDriverCommand(
                OwnerUserId: ownerId,
                FirstName: FirstName.Trim(),
                LastName: LastName.Trim(),
                Email: Email.Trim().ToLowerInvariant(),
                LicenceNumber: string.IsNullOrWhiteSpace(LicenceNumber) ? null : LicenceNumber.Trim(),

                // Contact + profile
                PhoneNumber: string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber.Trim(),
                DateOfBirthUtc: dobUtc,

                // Licence
                LicenceExpiresOnUtc: licenceExpUtc,
                LicenceVersion: string.IsNullOrWhiteSpace(LicenceVersion) ? null : LicenceVersion.Trim(),
                LicenceClassOrEndorsements: string.IsNullOrWhiteSpace(LicenceClassOrEndorsements) ? null : LicenceClassOrEndorsements.Trim(),
                LicenceIssuedOnUtc: issuedUtc,
                LicenceConditionsNotes: string.IsNullOrWhiteSpace(LicenceConditionsNotes) ? null : LicenceConditionsNotes.Trim(),

                // Address
                Line1: string.IsNullOrWhiteSpace(Line1) ? null : Line1.Trim(),
                Line2: string.IsNullOrWhiteSpace(Line2) ? null : Line2.Trim(),
                Suburb: string.IsNullOrWhiteSpace(Suburb) ? null : Suburb.Trim(),
                City: string.IsNullOrWhiteSpace(City) ? null : City.Trim(),
                Region: string.IsNullOrWhiteSpace(Region) ? null : Region.Trim(),
                Postcode: string.IsNullOrWhiteSpace(Postcode) ? null : Postcode.Trim(),
                Country: string.IsNullOrWhiteSpace(Country) ? null : Country.Trim(),

                // Emergency contact (required)
                EmergencyFirstName: EmergencyFirstName.Trim(),
                EmergencyLastName: EmergencyLastName.Trim(),
                EmergencyRelationship: EmergencyRelationship.Trim(),
                EmergencyEmail: EmergencyEmail.Trim().ToLowerInvariant(),
                EmergencyPhoneNumber: EmergencyPhoneNumber.Trim(),
                EmergencySecondaryPhoneNumber: string.IsNullOrWhiteSpace(EmergencySecondaryPhoneNumber)
                    ? null
                    : EmergencySecondaryPhoneNumber.Trim()
            );

            var created = await _createDriverHandler.HandleAsync(cmd);

            if (created == null)
            {
                await Shell.Current.DisplayAlertAsync("Save failed", "Unable to create driver.", "OK");
                return;
            }

            await Shell.Current.DisplayAlertAsync("Saved", "Driver created.", "OK");
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

    #endregion

    #region Helpers

    private void RefreshSaveState()
    {
        OnPropertyChanged(nameof(CanSave));
        (SaveDriverCommand as Command)?.ChangeCanExecute();
    }

    #endregion
}