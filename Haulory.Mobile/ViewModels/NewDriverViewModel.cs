using System.Windows.Input;
using Haulory.Application.Interfaces.Services;
using Haulory.Mobile.Diagnostics;
using Haulory.Mobile.Features;
using Haulory.Mobile.Features.Drivers.NewDriver;
using Haulory.Mobile.Services;

namespace Haulory.Mobile.ViewModels;

[QueryProperty(nameof(DriverId), "driverId")]
public class NewDriverViewModel : BaseViewModel
{
    #region Dependencies

    private readonly ISessionService _sessionService;
    private readonly NewDriverFormState _state = new();
    private readonly NewDriverValidator _validator = new();
    private readonly NewDriverEditorService _editorService;
    private readonly ICrashLogger _crashLogger;

    #endregion

    #region Commands

    public ICommand SaveDriverCommand { get; }

    #endregion

    #region Constructor

    public NewDriverViewModel(
        DriversApiService driversApiService,
        ISessionService sessionService,
        IFeatureAccessService featureAccessService,
        ICrashLogger crashLogger)
        : base(featureAccessService)
    {
        _sessionService = sessionService;
        _crashLogger = crashLogger;

        _editorService = new NewDriverEditorService(
            driversApiService,
            new NewDriverRequestMapper());

        SaveDriverCommand = new Command(async () => await ExecuteSaveAsync(), () => CanSaveDriverAction);

        RefreshSaveState();
    }

    #endregion

    #region State Properties

    public bool IsEditMode => _state.IsEditMode;
    public string PageTitle => IsEditMode ? "Edit driver" : "New driver";
    public string SaveButtonText => IsEditMode ? "Update driver" : "Save driver";
    public bool CanCreateLoginAccount => !IsEditMode;
    public bool IsMainProfileDriver => _state.IsMainProfile;

    public bool ShowCreateLoginSection =>
        !IsEditMode && !IsMainProfileDriver && IsAddDriverVisible;

    public string DriverId
    {
        get => _state.DriverId;
        set
        {
            if (_state.DriverId == value)
                return;

            _state.DriverId = value;
            OnPropertyChanged();
            RefreshSaveState();
        }
    }

    public bool IsMainUser =>
        _sessionService.CurrentAccountId.HasValue &&
        _sessionService.CurrentOwnerId.HasValue &&
        _sessionService.CurrentAccountId.Value == _sessionService.CurrentOwnerId.Value;

    public bool IsAddDriverVisible => IsFeatureVisible(AppFeature.AddDriver) && IsMainUser;
    public bool IsAddDriverEnabled => IsFeatureEnabled(AppFeature.AddDriver) && IsMainUser;

    public bool CreateLoginAccount
    {
        get => _state.CreateLoginAccount;
        set
        {
            if (_state.CreateLoginAccount == value)
                return;

            _state.CreateLoginAccount = value;

            if (!_state.CreateLoginAccount && !string.IsNullOrWhiteSpace(Password))
                Password = string.Empty;

            OnPropertyChanged();
            RefreshSaveState();
        }
    }

    public string Password
    {
        get => _state.Password;
        set
        {
            if (_state.Password == value)
                return;

            _state.Password = value;
            OnPropertyChanged();
            RefreshSaveState();
        }
    }

    public string FirstName { get => _state.FirstName; set => SetStateValue(_state.FirstName, value, x => _state.FirstName = x); }
    public string LastName { get => _state.LastName; set => SetStateValue(_state.LastName, value, x => _state.LastName = x); }
    public string Email { get => _state.Email; set => SetStateValue(_state.Email, value, x => _state.Email = x); }
    public string LicenceNumber { get => _state.LicenceNumber; set => SetStateValue(_state.LicenceNumber, value, x => _state.LicenceNumber = x); }
    public string PhoneNumber { get => _state.PhoneNumber; set => SetStateValue(_state.PhoneNumber, value, x => _state.PhoneNumber = x); }
    public DateTime DateOfBirthLocal { get => _state.DateOfBirthLocal; set => SetStateValue(_state.DateOfBirthLocal, value, x => _state.DateOfBirthLocal = x); }
    public DateTime LicenceExpiryLocal { get => _state.LicenceExpiryLocal; set => SetStateValue(_state.LicenceExpiryLocal, value, x => _state.LicenceExpiryLocal = x); }
    public string LicenceVersion { get => _state.LicenceVersion; set => SetStateValue(_state.LicenceVersion, value, x => _state.LicenceVersion = x); }
    public string LicenceClassOrEndorsements { get => _state.LicenceClassOrEndorsements; set => SetStateValue(_state.LicenceClassOrEndorsements, value, x => _state.LicenceClassOrEndorsements = x); }
    public string LicenceConditionsNotes { get => _state.LicenceConditionsNotes; set => SetStateValue(_state.LicenceConditionsNotes, value, x => _state.LicenceConditionsNotes = x); }
    public DateTime LicenceIssuedLocal { get => _state.LicenceIssuedLocal; set => SetStateValue(_state.LicenceIssuedLocal, value, x => _state.LicenceIssuedLocal = x); }
    public string Line1 { get => _state.Line1; set => SetStateValue(_state.Line1, value, x => _state.Line1 = x); }
    public string Line2 { get => _state.Line2; set => SetStateValue(_state.Line2, value, x => _state.Line2 = x); }
    public string Suburb { get => _state.Suburb; set => SetStateValue(_state.Suburb, value, x => _state.Suburb = x); }
    public string City { get => _state.City; set => SetStateValue(_state.City, value, x => _state.City = x); }
    public string Region { get => _state.Region; set => SetStateValue(_state.Region, value, x => _state.Region = x); }
    public string Postcode { get => _state.Postcode; set => SetStateValue(_state.Postcode, value, x => _state.Postcode = x); }
    public string Country { get => _state.Country; set => SetStateValue(_state.Country, value, x => _state.Country = x); }
    public string EmergencyFirstName { get => _state.EmergencyFirstName; set => SetStateValue(_state.EmergencyFirstName, value, x => _state.EmergencyFirstName = x); }
    public string EmergencyLastName { get => _state.EmergencyLastName; set => SetStateValue(_state.EmergencyLastName, value, x => _state.EmergencyLastName = x); }
    public string EmergencyRelationship { get => _state.EmergencyRelationship; set => SetStateValue(_state.EmergencyRelationship, value, x => _state.EmergencyRelationship = x); }
    public string EmergencyEmail { get => _state.EmergencyEmail; set => SetStateValue(_state.EmergencyEmail, value, x => _state.EmergencyEmail = x); }
    public string EmergencyPhoneNumber { get => _state.EmergencyPhoneNumber; set => SetStateValue(_state.EmergencyPhoneNumber, value, x => _state.EmergencyPhoneNumber = x); }
    public string EmergencySecondaryPhoneNumber { get => _state.EmergencySecondaryPhoneNumber; set => SetStateValue(_state.EmergencySecondaryPhoneNumber, value, x => _state.EmergencySecondaryPhoneNumber = x); }

    public bool CanSave =>
        _validator.CanSave(_state, _sessionService, FeatureAccessService, IsMainUser);

    public bool CanSaveDriverAction => IsAddDriverEnabled && CanSave;

    #endregion

    #region Private Methods

    private async Task ExecuteSaveAsync()
    {
        if (!await EnsureFeatureEnabledAsync(AppFeature.AddDriver))
            return;

        if (!_sessionService.IsAuthenticated)
            await _sessionService.RestoreAsync();

        var errors = _validator.Validate(_state);
        if (errors.Count > 0 || !CanSave)
        {
            await Shell.Current.DisplayAlertAsync(
                "Cannot save",
                errors.FirstOrDefault() ?? "Please complete all required fields before saving.",
                "OK");
            return;
        }

        try
        {
            _state.IsSaving = true;
            RefreshSaveState();

            await SafeRunner.RunAsync(
                async () =>
                {
                    await _editorService.SaveAsync(_state);

                    await Shell.Current.DisplayAlertAsync(
                        "Saved",
                        IsEditMode
                            ? "Driver updated."
                            : CreateLoginAccount ? "Driver + login account created." : "Driver created.",
                        "OK");

                    await Shell.Current.GoToAsync("..");
                },
                _crashLogger,
                "NewDriverViewModel.ExecuteSaveAsync",
                nameof(Views.NewDriverPage),
                metadataJson: $"{{\"DriverId\":\"{DriverId}\",\"IsEditMode\":{IsEditMode.ToString().ToLowerInvariant()},\"CreateLoginAccount\":{CreateLoginAccount.ToString().ToLowerInvariant()},\"IsMainProfileDriver\":{IsMainProfileDriver.ToString().ToLowerInvariant()}}}",
                onError: async ex =>
                {
                    await Shell.Current.DisplayAlertAsync("Save failed", ex.Message, "OK");
                });
        }
        finally
        {
            _state.IsSaving = false;
            RefreshSaveState();
        }
    }

    #endregion

    #region Public Methods

    public async Task InitializeAsync()
    {
        await SafeRunner.RunAsync(
            async () =>
            {
                if (!_sessionService.IsAuthenticated)
                    await _sessionService.RestoreAsync();

                if (IsEditMode && Guid.TryParse(DriverId, out var driverId))
                {
                    await _editorService.LoadIntoStateAsync(_state, driverId);
                    RaiseAllBindings();
                }

                RefreshSaveState();
            },
            _crashLogger,
            "NewDriverViewModel.InitializeAsync",
            nameof(Views.NewDriverPage),
            metadataJson: $"{{\"DriverId\":\"{DriverId}\",\"IsEditMode\":{IsEditMode.ToString().ToLowerInvariant()}}}",
            onError: async ex =>
            {
                await Shell.Current.DisplayAlertAsync("Not found", ex.Message, "OK");
            });
    }

    #endregion

    #region Helper Methods

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
        OnPropertyChanged(nameof(IsMainProfileDriver));
        OnPropertyChanged(nameof(ShowCreateLoginSection));
        (SaveDriverCommand as Command)?.ChangeCanExecute();
    }

    private void RaiseAllBindings()
    {
        OnPropertyChanged(nameof(FirstName));
        OnPropertyChanged(nameof(LastName));
        OnPropertyChanged(nameof(Email));
        OnPropertyChanged(nameof(PhoneNumber));
        OnPropertyChanged(nameof(DateOfBirthLocal));
        OnPropertyChanged(nameof(LicenceNumber));
        OnPropertyChanged(nameof(LicenceVersion));
        OnPropertyChanged(nameof(LicenceClassOrEndorsements));
        OnPropertyChanged(nameof(LicenceConditionsNotes));
        OnPropertyChanged(nameof(LicenceIssuedLocal));
        OnPropertyChanged(nameof(LicenceExpiryLocal));
        OnPropertyChanged(nameof(Line1));
        OnPropertyChanged(nameof(Line2));
        OnPropertyChanged(nameof(Suburb));
        OnPropertyChanged(nameof(City));
        OnPropertyChanged(nameof(Region));
        OnPropertyChanged(nameof(Postcode));
        OnPropertyChanged(nameof(Country));
        OnPropertyChanged(nameof(EmergencyFirstName));
        OnPropertyChanged(nameof(EmergencyLastName));
        OnPropertyChanged(nameof(EmergencyRelationship));
        OnPropertyChanged(nameof(EmergencyEmail));
        OnPropertyChanged(nameof(EmergencyPhoneNumber));
        OnPropertyChanged(nameof(EmergencySecondaryPhoneNumber));
        OnPropertyChanged(nameof(CreateLoginAccount));
        OnPropertyChanged(nameof(Password));
        OnPropertyChanged(nameof(IsMainProfileDriver));
        OnPropertyChanged(nameof(ShowCreateLoginSection));
    }

    private void SetStateValue<T>(T current, T value, Action<T> assign, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(current, value))
            return;

        assign(value);
        OnPropertyChanged(propertyName);
        RefreshSaveState();
    }

    #endregion
}