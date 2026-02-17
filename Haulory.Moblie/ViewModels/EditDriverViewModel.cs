using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Domain.Entities;
using System.Windows.Input;

namespace Haulory.Mobile.ViewModels;

public class EditDriverViewModel : BaseViewModel
{
    private readonly IDriverRepository _repo;
    private readonly ISessionService _session;
    private readonly IUserAccountRepository _users;

    private Driver? _driver;
    private bool _isSaving;

    private string _driverId = string.Empty;
    private bool _isLoaded;

    public EditDriverViewModel(IDriverRepository repo, ISessionService sessionService, IUserAccountRepository users)
    {
        _repo = repo;
        _session = sessionService;
        _users = users;

        SaveCommand = new Command(async () => await ExecuteSaveAsync(), () => CanSave);
    }

    public ICommand SaveCommand { get; }

    public async Task InitializeAsync(string driverId)
    {
        _driverId = driverId;
        _isLoaded = false;
        await LoadAsync();
    }

    // -------------------------
    // Editable fields (existing)
    // -------------------------
    private string _firstName = string.Empty;
    public string FirstName { get => _firstName; set { _firstName = value; OnPropertyChanged(); Refresh(); } }

    private string _lastName = string.Empty;
    public string LastName { get => _lastName; set { _lastName = value; OnPropertyChanged(); Refresh(); } }

    private string _email = string.Empty;
    public string Email { get => _email; set { _email = value; OnPropertyChanged(); Refresh(); } }

    private string _licenceNumber = string.Empty;
    public string LicenceNumber { get => _licenceNumber; set { _licenceNumber = value; OnPropertyChanged(); Refresh(); } }

    // -------------------------
    // NEW: Contact + licence + address
    // -------------------------
    private string _phoneNumber = string.Empty;
    public string PhoneNumber { get => _phoneNumber; set { _phoneNumber = value; OnPropertyChanged(); Refresh(); } }

    // DatePickers need a value; we’ll default to Today, but on load we set to existing DOB/expiry when available.
    private DateTime _dateOfBirthLocal = DateTime.Today;
    public DateTime DateOfBirthLocal { get => _dateOfBirthLocal; set { _dateOfBirthLocal = value; OnPropertyChanged(); } }

    private DateTime _licenceExpiryLocal = DateTime.Today;
    public DateTime LicenceExpiryLocal { get => _licenceExpiryLocal; set { _licenceExpiryLocal = value; OnPropertyChanged(); } }

    private string _line1 = string.Empty;
    public string Line1 { get => _line1; set { _line1 = value; OnPropertyChanged(); } }

    private string _line2 = string.Empty;
    public string Line2 { get => _line2; set { _line2 = value; OnPropertyChanged(); } }

    private string _suburb = string.Empty;
    public string Suburb { get => _suburb; set { _suburb = value; OnPropertyChanged(); } }

    private string _city = string.Empty;
    public string City { get => _city; set { _city = value; OnPropertyChanged(); } }

    private string _region = string.Empty;
    public string Region { get => _region; set { _region = value; OnPropertyChanged(); } }

    private string _postcode = string.Empty;
    public string Postcode { get => _postcode; set { _postcode = value; OnPropertyChanged(); } }

    private string _country = string.Empty;
    public string Country { get => _country; set { _country = value; OnPropertyChanged(); } }

    // -------------------------
    // Emergency Contact
    // -------------------------
    private string _ecFirstName = string.Empty;
    public string EmergencyFirstName { get => _ecFirstName; set { _ecFirstName = value; OnPropertyChanged(); Refresh(); } }

    private string _ecLastName = string.Empty;
    public string EmergencyLastName { get => _ecLastName; set { _ecLastName = value; OnPropertyChanged(); Refresh(); } }

    private string _ecRelationship = string.Empty;
    public string EmergencyRelationship { get => _ecRelationship; set { _ecRelationship = value; OnPropertyChanged(); Refresh(); } }

    private string _ecEmail = string.Empty;
    public string EmergencyEmail { get => _ecEmail; set { _ecEmail = value; OnPropertyChanged(); Refresh(); } }

    private string _ecPhone = string.Empty;
    public string EmergencyPhoneNumber { get => _ecPhone; set { _ecPhone = value; OnPropertyChanged(); Refresh(); } }

    private string _ecPhone2 = string.Empty;
    public string EmergencySecondaryPhoneNumber { get => _ecPhone2; set { _ecPhone2 = value; OnPropertyChanged(); Refresh(); } }

    // Save should be possible even if Emergency Contact isn't complete
    public bool CanSave
    {
        get
        {
            if (_isSaving) return false;
            if (_driver == null) return false;

            if (string.IsNullOrWhiteSpace(FirstName)) return false;
            if (string.IsNullOrWhiteSpace(LastName)) return false;

            var email = Email?.Trim();
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@')) return false;

            return true;
        }
    }

    private async Task LoadAsync()
    {
        if (_isLoaded) return;

        if (!Guid.TryParse(_driverId, out var id))
            return;

        // Must be logged in
        var ownerUserId = _session.CurrentAccountId ?? Guid.Empty;
        if (!_session.IsAuthenticated || ownerUserId == Guid.Empty)
            return;

        // Only load drivers owned by the current main user
        var ownedDrivers = await _repo.GetAllByOwnerUserIdAsync(ownerUserId);
        _driver = ownedDrivers.FirstOrDefault(d => d.Id == id);

        if (_driver == null)
            return;

        // Populate UI fields (existing)
        FirstName = _driver.FirstName ?? string.Empty;
        LastName = _driver.LastName ?? string.Empty;
        Email = _driver.Email ?? string.Empty;
        LicenceNumber = _driver.LicenceNumber ?? string.Empty;

        // Populate NEW fields
        PhoneNumber = _driver.PhoneNumber ?? string.Empty;

        if (_driver.DateOfBirthUtc.HasValue)
            DateOfBirthLocal = _driver.DateOfBirthUtc.Value.ToLocalTime().Date;

        if (_driver.LicenceExpiresOnUtc.HasValue)
            LicenceExpiryLocal = _driver.LicenceExpiresOnUtc.Value.ToLocalTime().Date;

        Line1 = _driver.Line1 ?? string.Empty;
        Line2 = _driver.Line2 ?? string.Empty;
        Suburb = _driver.Suburb ?? string.Empty;
        City = _driver.City ?? string.Empty;
        Region = _driver.Region ?? string.Empty;
        Postcode = _driver.Postcode ?? string.Empty;
        Country = _driver.Country ?? string.Empty;

        // Emergency contact
        var ec = _driver.EmergencyContact ?? new EmergencyContact();
        EmergencyFirstName = ec.FirstName ?? string.Empty;
        EmergencyLastName = ec.LastName ?? string.Empty;
        EmergencyRelationship = ec.Relationship ?? string.Empty;
        EmergencyEmail = ec.Email ?? string.Empty;
        EmergencyPhoneNumber = ec.PhoneNumber ?? string.Empty;
        EmergencySecondaryPhoneNumber = ec.SecondaryPhoneNumber ?? string.Empty;

        _isLoaded = true;
        Refresh();
    }

    private async Task ExecuteSaveAsync()
    {
        if (_driver == null) return;
        if (!CanSave) return;

        try
        {
            _isSaving = true;
            Refresh();

            // update driver (existing)
            _driver.UpdateIdentity(FirstName, LastName, Email);
            _driver.UpdateLicenceNumber(string.IsNullOrWhiteSpace(LicenceNumber) ? null : LicenceNumber);

            // update NEW fields
            _driver.UpdatePhone(string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber);

            var dobUtc = DateTime.SpecifyKind(DateOfBirthLocal.Date, DateTimeKind.Local).ToUniversalTime();
            _driver.UpdateDateOfBirthUtc(dobUtc);

            var licExpUtc = DateTime.SpecifyKind(LicenceExpiryLocal.Date, DateTimeKind.Local).ToUniversalTime();
            _driver.UpdateLicenceExpiryUtc(licExpUtc);

            _driver.UpdateAddress(
                string.IsNullOrWhiteSpace(Line1) ? null : Line1,
                string.IsNullOrWhiteSpace(Line2) ? null : Line2,
                string.IsNullOrWhiteSpace(Suburb) ? null : Suburb,
                string.IsNullOrWhiteSpace(City) ? null : City,
                string.IsNullOrWhiteSpace(Region) ? null : Region,
                string.IsNullOrWhiteSpace(Postcode) ? null : Postcode,
                string.IsNullOrWhiteSpace(Country) ? null : Country
            );

            // emergency contact (unchanged)
            var ec = new EmergencyContact(
                EmergencyFirstName,
                EmergencyLastName,
                EmergencyRelationship,
                EmergencyEmail,
                EmergencyPhoneNumber,
                string.IsNullOrWhiteSpace(EmergencySecondaryPhoneNumber) ? null : EmergencySecondaryPhoneNumber
            );
            _driver.UpdateEmergencyContact(ec);

            await _repo.SaveAsync(_driver);

            // If this driver is the MAIN user's profile, sync UserAccount identity too
            if (_driver.UserId.HasValue && (_session.CurrentAccountId == _driver.UserId.Value))
            {
                var account = await _users.GetByIdAsync(_driver.UserId.Value);
                if (account != null)
                {
                    account.UpdateIdentity(FirstName, LastName, Email);
                    await _users.UpdateAsync(account);
                }
            }

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

    private void Refresh()
    {
        OnPropertyChanged(nameof(CanSave));
        (SaveCommand as Command)?.ChangeCanExecute();
    }
}
