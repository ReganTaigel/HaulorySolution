using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Domain.Entities;
using Haulory.Mobile.Views;
using System.Windows.Input;

namespace Haulory.Mobile.ViewModels;

public class EditDriverViewModel : BaseViewModel
{
    private readonly IDriverRepository _repo;
    private readonly ISessionService _session;
    private readonly IUserRepository _users;

    private Driver? _driver;
    private bool _isSaving;

    private string _driverId = string.Empty;
    private bool _isLoaded;

    public EditDriverViewModel(IDriverRepository repo, ISessionService sessionService, IUserRepository users)
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

    // Editable fields
    private string _firstName = string.Empty;
    public string FirstName { get => _firstName; set { _firstName = value; OnPropertyChanged(); Refresh(); } }

    private string _lastName = string.Empty;
    public string LastName { get => _lastName; set { _lastName = value; OnPropertyChanged(); Refresh(); } }

    private string _email = string.Empty;
    public string Email { get => _email; set { _email = value; OnPropertyChanged(); Refresh(); } }

    private string _licenceNumber = string.Empty;
    public string LicenceNumber { get => _licenceNumber; set { _licenceNumber = value; OnPropertyChanged(); Refresh(); } }

    // Emergency Contact
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
        var ownerUserId = _session.CurrentUser?.Id ?? Guid.Empty;
        if (!_session.IsAuthenticated || ownerUserId == Guid.Empty)
            return;

        // Only load drivers owned by the current main user
        var ownedDrivers = await _repo.GetAllByOwnerUserIdAsync(ownerUserId);
        _driver = ownedDrivers.FirstOrDefault(d => d.Id == id);

        if (_driver == null)
            return;

        // Populate UI fields
        FirstName = _driver.FirstName ?? string.Empty;
        LastName = _driver.LastName ?? string.Empty;
        Email = _driver.Email ?? string.Empty;
        LicenceNumber = _driver.LicenceNumber ?? string.Empty;

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

    private bool HasAnyEmergencyInput()
    {
        return !string.IsNullOrWhiteSpace(EmergencyFirstName) ||
               !string.IsNullOrWhiteSpace(EmergencyLastName) ||
               !string.IsNullOrWhiteSpace(EmergencyRelationship) ||
               !string.IsNullOrWhiteSpace(EmergencyEmail) ||
               !string.IsNullOrWhiteSpace(EmergencyPhoneNumber) ||
               !string.IsNullOrWhiteSpace(EmergencySecondaryPhoneNumber);
    }

    private void EnsureEmergencyIsValidIfProvided()
    {
        if (!HasAnyEmergencyInput())
            return;

        if (string.IsNullOrWhiteSpace(EmergencyFirstName) ||
            string.IsNullOrWhiteSpace(EmergencyLastName) ||
            string.IsNullOrWhiteSpace(EmergencyRelationship) ||
            string.IsNullOrWhiteSpace(EmergencyPhoneNumber))
            throw new InvalidOperationException("Please complete the emergency contact details before saving them.");

        var ecEmail = EmergencyEmail?.Trim();
        if (string.IsNullOrWhiteSpace(ecEmail) || !ecEmail.Contains('@'))
            throw new InvalidOperationException("Please enter a valid emergency email.");
    }

    private async Task ExecuteSaveAsync()
    {
        if (_driver == null) return;
        if (!CanSave) return;

        try
        {
            _isSaving = true;
            Refresh();

            // update driver
            _driver.UpdateIdentity(FirstName, LastName, Email);
            _driver.UpdateLicenceNumber(string.IsNullOrWhiteSpace(LicenceNumber) ? null : LicenceNumber);

            // emergency (same as your current logic)
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

            // ✅ CRITICAL: if this is MAIN profile, update the existing User record (NO new User!)
            if (_driver.UserId.HasValue && _session.CurrentUser?.Id == _driver.UserId.Value)
            {
                var u = _session.CurrentUser!;
                u.UpdateIdentity(FirstName, LastName, Email);

                await _users.UpdateAsync(u);
                await _session.SetUserAsync(u); // keep session consistent
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
