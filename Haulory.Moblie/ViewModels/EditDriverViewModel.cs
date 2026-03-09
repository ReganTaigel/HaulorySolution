using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Haulory.Application.Interfaces.Services;
using Haulory.Mobile.Contracts.Drivers;
using Haulory.Mobile.Services;
using Microsoft.Maui.Controls;

namespace Haulory.Mobile.ViewModels;

public class EditDriverViewModel : BaseViewModel
{
    #region Dependencies

    private readonly DriversApiService _driversApiService;
    private readonly ISessionService _session;

    #endregion

    #region State

    private bool _isSaving;
    private string _driverId = string.Empty;
    private bool _isLoaded;

    #endregion

    #region Commands

    public ICommand SaveCommand { get; }

    #endregion

    #region Constructor

    public EditDriverViewModel(
        DriversApiService driversApiService,
        ISessionService sessionService)
    {
        _driversApiService = driversApiService;
        _session = sessionService;

        SaveCommand = new Command(async () => await ExecuteSaveAsync(), () => CanSave);
    }

    #endregion

    #region Initialization

    public async Task InitializeAsync(string driverId)
    {
        _driverId = driverId;
        _isLoaded = false;

        await LoadAsync();
    }

    #endregion

    #region Editable Fields - Identity

    private string _firstName = string.Empty;
    public string FirstName
    {
        get => _firstName;
        set
        {
            _firstName = value;
            OnPropertyChanged();
            Refresh();
        }
    }

    private string _lastName = string.Empty;
    public string LastName
    {
        get => _lastName;
        set
        {
            _lastName = value;
            OnPropertyChanged();
            Refresh();
        }
    }

    private string _email = string.Empty;
    public string Email
    {
        get => _email;
        set
        {
            _email = value;
            OnPropertyChanged();
            Refresh();
        }
    }

    private string _licenceNumber = string.Empty;
    public string LicenceNumber
    {
        get => _licenceNumber;
        set
        {
            _licenceNumber = value;
            OnPropertyChanged();
            Refresh();
        }
    }

    private string _licenceVersion = string.Empty;
    public string LicenceVersion
    {
        get => _licenceVersion;
        set
        {
            _licenceVersion = value;
            OnPropertyChanged();
            Refresh();
        }
    }

    private string _licenceClassOrEndorsements = string.Empty;
    public string LicenceClassOrEndorsements
    {
        get => _licenceClassOrEndorsements;
        set
        {
            _licenceClassOrEndorsements = value;
            OnPropertyChanged();
            Refresh();
        }
    }

    private DateTime _licenceIssuedLocal = DateTime.Today;
    public DateTime LicenceIssuedLocal
    {
        get => _licenceIssuedLocal;
        set
        {
            _licenceIssuedLocal = value;
            OnPropertyChanged();
        }
    }

    private string _licenceConditionsNotes = string.Empty;
    public string LicenceConditionsNotes
    {
        get => _licenceConditionsNotes;
        set
        {
            _licenceConditionsNotes = value;
            OnPropertyChanged();
            Refresh();
        }
    }

    #endregion

    #region Editable Fields - Contact + Licence + Address

    private string _phoneNumber = string.Empty;
    public string PhoneNumber
    {
        get => _phoneNumber;
        set
        {
            _phoneNumber = value;
            OnPropertyChanged();
            Refresh();
        }
    }

    private DateTime _dateOfBirthLocal = DateTime.Today;
    public DateTime DateOfBirthLocal
    {
        get => _dateOfBirthLocal;
        set
        {
            _dateOfBirthLocal = value;
            OnPropertyChanged();
        }
    }

    private DateTime _licenceExpiryLocal = DateTime.Today;
    public DateTime LicenceExpiryLocal
    {
        get => _licenceExpiryLocal;
        set
        {
            _licenceExpiryLocal = value;
            OnPropertyChanged();
        }
    }

    private string _line1 = string.Empty;
    public string Line1
    {
        get => _line1;
        set
        {
            _line1 = value;
            OnPropertyChanged();
        }
    }

    private string _line2 = string.Empty;
    public string Line2
    {
        get => _line2;
        set
        {
            _line2 = value;
            OnPropertyChanged();
        }
    }

    private string _suburb = string.Empty;
    public string Suburb
    {
        get => _suburb;
        set
        {
            _suburb = value;
            OnPropertyChanged();
        }
    }

    private string _city = string.Empty;
    public string City
    {
        get => _city;
        set
        {
            _city = value;
            OnPropertyChanged();
        }
    }

    private string _region = string.Empty;
    public string Region
    {
        get => _region;
        set
        {
            _region = value;
            OnPropertyChanged();
        }
    }

    private string _postcode = string.Empty;
    public string Postcode
    {
        get => _postcode;
        set
        {
            _postcode = value;
            OnPropertyChanged();
        }
    }

    private string _country = string.Empty;
    public string Country
    {
        get => _country;
        set
        {
            _country = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Editable Fields - Emergency Contact

    private string _ecFirstName = string.Empty;
    public string EmergencyFirstName
    {
        get => _ecFirstName;
        set
        {
            _ecFirstName = value;
            OnPropertyChanged();
            Refresh();
        }
    }

    private string _ecLastName = string.Empty;
    public string EmergencyLastName
    {
        get => _ecLastName;
        set
        {
            _ecLastName = value;
            OnPropertyChanged();
            Refresh();
        }
    }

    private string _ecRelationship = string.Empty;
    public string EmergencyRelationship
    {
        get => _ecRelationship;
        set
        {
            _ecRelationship = value;
            OnPropertyChanged();
            Refresh();
        }
    }

    private string _ecEmail = string.Empty;
    public string EmergencyEmail
    {
        get => _ecEmail;
        set
        {
            _ecEmail = value;
            OnPropertyChanged();
            Refresh();
        }
    }

    private string _ecPhone = string.Empty;
    public string EmergencyPhoneNumber
    {
        get => _ecPhone;
        set
        {
            _ecPhone = value;
            OnPropertyChanged();
            Refresh();
        }
    }

    private string _ecPhone2 = string.Empty;
    public string EmergencySecondaryPhoneNumber
    {
        get => _ecPhone2;
        set
        {
            _ecPhone2 = value;
            OnPropertyChanged();
            Refresh();
        }
    }

    #endregion

    #region Validation

    public bool CanSave
    {
        get
        {
            if (_isSaving) return false;

            if (string.IsNullOrWhiteSpace(FirstName)) return false;
            if (string.IsNullOrWhiteSpace(LastName)) return false;

            var email = Email?.Trim();
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@')) return false;

            return true;
        }
    }

    #endregion

    #region Load

    private async Task LoadAsync()
    {
        if (_isLoaded)
            return;

        if (!Guid.TryParse(_driverId, out var id))
            return;

        if (!_session.IsAuthenticated)
            await _session.RestoreAsync();

        if (!_session.IsAuthenticated)
            return;

        var dto = await _driversApiService.GetDriverByIdAsync(id);
        if (dto == null)
            return;

        FirstName = dto.FirstName ?? string.Empty;
        LastName = dto.LastName ?? string.Empty;
        Email = dto.Email ?? string.Empty;

        LicenceNumber = dto.LicenceNumber ?? string.Empty;
        LicenceVersion = dto.LicenceVersion ?? string.Empty;
        LicenceClassOrEndorsements = dto.LicenceClassOrEndorsements ?? string.Empty;
        LicenceConditionsNotes = dto.LicenceConditionsNotes ?? string.Empty;

        if (dto.LicenceIssuedOnUtc.HasValue)
            LicenceIssuedLocal = dto.LicenceIssuedOnUtc.Value.ToLocalTime().Date;

        if (dto.DateOfBirthUtc.HasValue)
            DateOfBirthLocal = dto.DateOfBirthUtc.Value.ToLocalTime().Date;

        if (dto.LicenceExpiresOnUtc.HasValue)
            LicenceExpiryLocal = dto.LicenceExpiresOnUtc.Value.ToLocalTime().Date;

        PhoneNumber = dto.PhoneNumber ?? string.Empty;

        Line1 = dto.Line1 ?? string.Empty;
        Line2 = dto.Line2 ?? string.Empty;
        Suburb = dto.Suburb ?? string.Empty;
        City = dto.City ?? string.Empty;
        Region = dto.Region ?? string.Empty;
        Postcode = dto.Postcode ?? string.Empty;
        Country = dto.Country ?? string.Empty;

        EmergencyFirstName = dto.EmergencyContact?.FirstName ?? string.Empty;
        EmergencyLastName = dto.EmergencyContact?.LastName ?? string.Empty;
        EmergencyRelationship = dto.EmergencyContact?.Relationship ?? string.Empty;
        EmergencyEmail = dto.EmergencyContact?.Email ?? string.Empty;
        EmergencyPhoneNumber = dto.EmergencyContact?.PhoneNumber ?? string.Empty;
        EmergencySecondaryPhoneNumber = dto.EmergencyContact?.SecondaryPhoneNumber ?? string.Empty;

        _isLoaded = true;
        Refresh();
    }

    #endregion

    #region Save

    private async Task ExecuteSaveAsync()
    {
        if (!Guid.TryParse(_driverId, out var id))
            return;

        if (!CanSave)
            return;

        try
        {
            _isSaving = true;
            Refresh();

            var request = new UpdateDriverRequest
            {
                FirstName = FirstName.Trim(),
                LastName = LastName.Trim(),
                Email = Email.Trim(),

                PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber.Trim(),
                DateOfBirthUtc = DateTime.SpecifyKind(DateOfBirthLocal.Date, DateTimeKind.Local).ToUniversalTime(),

                LicenceNumber = string.IsNullOrWhiteSpace(LicenceNumber) ? null : LicenceNumber.Trim(),
                LicenceVersion = string.IsNullOrWhiteSpace(LicenceVersion) ? null : LicenceVersion.Trim(),
                LicenceClassOrEndorsements = string.IsNullOrWhiteSpace(LicenceClassOrEndorsements) ? null : LicenceClassOrEndorsements.Trim(),
                LicenceIssuedOnUtc = DateTime.SpecifyKind(LicenceIssuedLocal.Date, DateTimeKind.Local).ToUniversalTime(),
                LicenceExpiresOnUtc = DateTime.SpecifyKind(LicenceExpiryLocal.Date, DateTimeKind.Local).ToUniversalTime(),
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
                    FirstName = string.IsNullOrWhiteSpace(EmergencyFirstName) ? null : EmergencyFirstName.Trim(),
                    LastName = string.IsNullOrWhiteSpace(EmergencyLastName) ? null : EmergencyLastName.Trim(),
                    Relationship = string.IsNullOrWhiteSpace(EmergencyRelationship) ? null : EmergencyRelationship.Trim(),
                    Email = string.IsNullOrWhiteSpace(EmergencyEmail) ? null : EmergencyEmail.Trim(),
                    PhoneNumber = string.IsNullOrWhiteSpace(EmergencyPhoneNumber) ? null : EmergencyPhoneNumber.Trim(),
                    SecondaryPhoneNumber = string.IsNullOrWhiteSpace(EmergencySecondaryPhoneNumber)
                        ? null
                        : EmergencySecondaryPhoneNumber.Trim()
                }
            };

            await _driversApiService.UpdateDriverAsync(id, request);

            await Shell.Current.DisplayAlertAsync("Saved", "Driver updated.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Save failed", ex.Message, "OK");
        }
        finally
        {
            _isSaving = false;
            Refresh();
        }
    }

    #endregion

    #region UI Helpers

    private void Refresh()
    {
        OnPropertyChanged(nameof(CanSave));
        (SaveCommand as Command)?.ChangeCanExecute();
    }

    #endregion
}