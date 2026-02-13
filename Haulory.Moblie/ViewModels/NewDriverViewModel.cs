using Haulory.Application.Features.Drivers;
using Haulory.Application.Interfaces.Services;
using System.Windows.Input;

namespace Haulory.Mobile.ViewModels;

[QueryProperty(nameof(DriverId), "driverId")]
public class NewDriverViewModel : BaseViewModel
{
    private readonly CreateDriverHandler _createDriverHandler;
    private readonly ISessionService _sessionService;

    private string _driverId = string.Empty;
    private bool _isSaving;

    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _email = string.Empty;

    private string _licenceNumber = string.Empty;

    // Emergency Contact
    private string _ecFirstName = string.Empty;
    private string _ecLastName = string.Empty;
    private string _ecRelationship = string.Empty;
    private string _ecEmail = string.Empty;
    private string _ecPhoneNumber = string.Empty;
    private string _ecSecondaryPhoneNumber = string.Empty;

    public string DriverId
    {
        get => _driverId;
        set
        {
            _driverId = value;
            // New page: not loading an existing driver here
        }
    }

    public string FirstName { get => _firstName; set { _firstName = value; OnPropertyChanged(); RefreshSaveState(); } }
    public string LastName { get => _lastName; set { _lastName = value; OnPropertyChanged(); RefreshSaveState(); } }
    public string Email { get => _email; set { _email = value; OnPropertyChanged(); RefreshSaveState(); } }
    public string LicenceNumber { get => _licenceNumber; set { _licenceNumber = value; OnPropertyChanged(); RefreshSaveState(); } }

    public string EmergencyFirstName { get => _ecFirstName; set { _ecFirstName = value; OnPropertyChanged(); RefreshSaveState(); } }
    public string EmergencyLastName { get => _ecLastName; set { _ecLastName = value; OnPropertyChanged(); RefreshSaveState(); } }
    public string EmergencyRelationship { get => _ecRelationship; set { _ecRelationship = value; OnPropertyChanged(); RefreshSaveState(); } }
    public string EmergencyEmail { get => _ecEmail; set { _ecEmail = value; OnPropertyChanged(); RefreshSaveState(); } }
    public string EmergencyPhoneNumber { get => _ecPhoneNumber; set { _ecPhoneNumber = value; OnPropertyChanged(); RefreshSaveState(); } }
    public string EmergencySecondaryPhoneNumber { get => _ecSecondaryPhoneNumber; set { _ecSecondaryPhoneNumber = value; OnPropertyChanged(); RefreshSaveState(); } }

    public bool CanSave
    {
        get
        {
            if (_isSaving) return false;

            // Must be logged in to create owned sub drivers
            if (!_sessionService.IsAuthenticated) return false;
            if (_sessionService.CurrentUser == null) return false;

            // Driver
            if (string.IsNullOrWhiteSpace(FirstName)) return false;
            if (string.IsNullOrWhiteSpace(LastName)) return false;

            var email = Email?.Trim();
            if (string.IsNullOrWhiteSpace(email)) return false;
            if (!email.Contains('@')) return false;

            // Emergency Contact (required)
            if (string.IsNullOrWhiteSpace(EmergencyFirstName)) return false;
            if (string.IsNullOrWhiteSpace(EmergencyLastName)) return false;
            if (string.IsNullOrWhiteSpace(EmergencyRelationship)) return false;

            var ecEmail = EmergencyEmail?.Trim();
            if (string.IsNullOrWhiteSpace(ecEmail)) return false;
            if (!ecEmail.Contains('@')) return false;

            if (string.IsNullOrWhiteSpace(EmergencyPhoneNumber)) return false;

            return true;
        }
    }

    public ICommand SaveDriverCommand { get; }

    public NewDriverViewModel(CreateDriverHandler createDriverHandler, ISessionService sessionService)
    {
        _createDriverHandler = createDriverHandler;
        _sessionService = sessionService;

        SaveDriverCommand = new Command(async () => await ExecuteSaveAsync(), () => CanSave);
        RefreshSaveState();
    }

    private async Task ExecuteSaveAsync()
    {
        // restore session on restart
        if (!_sessionService.IsAuthenticated)
            await _sessionService.RestoreAsync();

        if (!CanSave) return;

        try
        {
            _isSaving = true;
            RefreshSaveState();

            var ownerUserId = _sessionService.CurrentUser?.Id ?? Guid.Empty;
            if (ownerUserId == Guid.Empty)
                throw new InvalidOperationException("You must be logged in to add a driver.");

            var result = await _createDriverHandler.HandleAsync(
                new CreateDriverCommand(
                    ownerUserId,
                    FirstName,
                    LastName,
                    Email,
                    string.IsNullOrWhiteSpace(LicenceNumber) ? null : LicenceNumber,
                    EmergencyFirstName,
                    EmergencyLastName,
                    EmergencyRelationship,
                    EmergencyEmail,
                    EmergencyPhoneNumber,
                    string.IsNullOrWhiteSpace(EmergencySecondaryPhoneNumber) ? null : EmergencySecondaryPhoneNumber
                ));

            if (result == null)
                throw new InvalidOperationException("Please check the details and try again.");

            await Shell.Current.DisplayAlertAsync("Saved", "Driver saved successfully.", "OK");
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
}
