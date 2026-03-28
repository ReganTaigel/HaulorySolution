using Haulory.Application.Interfaces.Services;
using Haulory.Mobile.Diagnostics;
using Haulory.Mobile.Features;
using Haulory.Mobile.Features.Jobs.NewJob;
using Haulory.Mobile.Services;
using Haulory.Mobile.Views;
using Haulory.Domain.Enums;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Haulory.Mobile.ViewModels;

public class NewJobViewModel : BaseViewModel, IQueryAttributable
{
    #region Dependencies

    private readonly ISessionService _session;
    private readonly NewJobFormState _state = new();
    private readonly NewJobValidator _validator = new();
    private readonly NewJobEditorService _editorService;
    private readonly JobPickerLoader _pickerLoader;
    private readonly ICrashLogger _crashLogger;
    private readonly CustomersApiService _customersApiService;

    #endregion

    #region Selection State
    private CustomerPickerItem? _selectedCustomer;
    private DriverPickerItem? _selectedDriver;
    private VehiclePickerItem? _selectedVehicle;
    private VehiclePickerItem? _selectedTrailer1;
    private VehiclePickerItem? _selectedTrailer2;

    #endregion

    #region Collections

    public IReadOnlyList<RateType> RateTypes { get; } =
        Enum.GetValues(typeof(RateType)).Cast<RateType>().ToList();

    public ObservableCollection<DriverPickerItem> Drivers { get; } = new();
    public ObservableCollection<VehiclePickerItem> Vehicles { get; } = new();
    public ObservableCollection<VehiclePickerItem> Trailers { get; } = new();
    public ObservableCollection<CustomerPickerItem> Customers { get; } = new();

    #endregion

    #region Constructor

    public NewJobViewModel(
        JobsApiService jobsApiService,
        DriversApiService driversApiService,
        VehiclesApiService vehiclesApiService,
        CustomersApiService customersApiService,
        ISessionService session,
        IFeatureAccessService featureAccessService,
        ICrashLogger crashLogger)
        : base(featureAccessService)
    {
        _session = session;
        _crashLogger = crashLogger;
        _customersApiService = customersApiService;

        _editorService = new NewJobEditorService(
            jobsApiService,
            new NewJobRequestMapper(_validator));

        _pickerLoader = new JobPickerLoader(driversApiService, vehiclesApiService);

        SaveJobCommand = new Command(async () => await SaveAsync());
        CancelCommand = new Command(async () =>
            await Shell.Current.GoToAsync($"//{AppShell.RouteJobs}"));

        _state.RateType = RateType.PerLoad;
    }

    #endregion

    #region Feature Access

    public bool IsAddJobVisible => IsFeatureVisible(AppFeature.AddJob);
    public bool IsAddJobEnabled => IsFeatureEnabled(AppFeature.AddJob);

    public bool IsPickupOnly => _state.IsPickupOnly;
    public bool IsReviewOnly => _state.IsReviewOnly;
    public bool IsEditMode => _state.IsEditMode;

    public bool IsPageVisible => IsPickupOnly || IsReviewOnly || IsAddJobVisible;
    public bool IsSaveVisible => IsPickupOnly || IsReviewOnly || IsAddJobVisible;
    public bool IsSaveEnabled => IsPickupOnly || IsReviewOnly || IsAddJobEnabled;

    public bool CanEditFullJob => !IsPickupOnly && !IsReviewOnly;
    public bool CanEditPickupDetails => IsEditMode && (IsPickupOnly || IsReviewOnly);
    public bool CanEditReviewDetails => IsReviewOnly;

    public string PageTitle => IsReviewOnly
        ? "Review delivery"
        : IsPickupOnly
            ? "Pickup details"
            : IsEditMode ? "Edit job" : "New job";

    public string PageSubtitle => IsReviewOnly
        ? "Review and approve wait time and damage notes"
        : IsPickupOnly
            ? "Update wait time and damage notes"
            : IsEditMode
                ? "Update pickup, delivery, assignment, and billing details"
                : "Enter pickup, delivery, assignment, and billing details";

    public string SaveButtonText => IsReviewOnly
        ? "Approve review"
        : IsPickupOnly
            ? "Save pickup details"
            : IsEditMode ? "Update job" : "Save job";

    #endregion

    #region Selected Pickers

    public DriverPickerItem? SelectedDriver
    {
        get => _selectedDriver;
        set
        {
            if (SetProperty(ref _selectedDriver, value))
                SyncSelections();
        }
    }

    public VehiclePickerItem? SelectedVehicle
    {
        get => _selectedVehicle;
        set
        {
            if (SetProperty(ref _selectedVehicle, value))
            {
                if (_selectedVehicle == null)
                {
                    SelectedTrailer1 = null;
                    SelectedTrailer2 = null;
                }

                SyncSelections();
            }
        }
    }

    public VehiclePickerItem? SelectedTrailer1
    {
        get => _selectedTrailer1;
        set
        {
            if (_selectedTrailer1 == value)
                return;

            if (value != null && SelectedTrailer2?.Id == value.Id)
                SelectedTrailer2 = null;

            _selectedTrailer1 = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedTrailerSummary));
            SyncSelections();
        }
    }

    public VehiclePickerItem? SelectedTrailer2
    {
        get => _selectedTrailer2;
        set
        {
            if (_selectedTrailer2 == value)
                return;

            if (value != null && SelectedTrailer1?.Id == value.Id)
                return;

            _selectedTrailer2 = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedTrailerSummary));
            SyncSelections();
        }
    }
    public CustomerPickerItem? SelectedCustomer
    {
        get => _selectedCustomer;
        set
        {
            if (SetProperty(ref _selectedCustomer, value))
                ApplySelectedCustomer();
        }
    }

    #endregion

    #region Form Properties

    public string PickupCompany
    {
        get => _state.PickupCompany;
        set => SetStateValue(_state.PickupCompany, value, x => _state.PickupCompany = x);
    }

    public string PickupAddress
    {
        get => _state.PickupAddress;
        set => SetStateValue(_state.PickupAddress, value, x => _state.PickupAddress = x);
    }

    public string DeliveryCompany
    {
        get => _state.DeliveryCompany;
        set => SetStateValue(_state.DeliveryCompany, value, x => _state.DeliveryCompany = x);
    }

    public string DeliveryAddress
    {
        get => _state.DeliveryAddress;
        set => SetStateValue(_state.DeliveryAddress, value, x => _state.DeliveryAddress = x);
    }

    public string ReferenceNumber
    {
        get => _state.ReferenceNumber;
        set => SetStateValue(_state.ReferenceNumber, value, x => _state.ReferenceNumber = x);
    }

    public string LoadDescription
    {
        get => _state.LoadDescription;
        set => SetStateValue(_state.LoadDescription, value, x => _state.LoadDescription = x);
    }

    public string ClientCompanyName
    {
        get => _state.ClientCompanyName;
        set => SetStateValue(_state.ClientCompanyName, value, x => _state.ClientCompanyName = x);
    }

    public string? ClientContactName
    {
        get => _state.ClientContactName;
        set => SetStateValue(_state.ClientContactName, value, x => _state.ClientContactName = x);
    }

    public string? ClientEmail
    {
        get => _state.ClientEmail;
        set => SetStateValue(_state.ClientEmail, value, x => _state.ClientEmail = x);
    }

    public string ClientAddressLine1
    {
        get => _state.ClientAddressLine1;
        set => SetStateValue(_state.ClientAddressLine1, value, x => _state.ClientAddressLine1 = x);
    }

    public string ClientCity
    {
        get => _state.ClientCity;
        set => SetStateValue(_state.ClientCity, value, x => _state.ClientCity = x);
    }

    public string ClientCountry
    {
        get => _state.ClientCountry;
        set => SetStateValue(_state.ClientCountry, value, x => _state.ClientCountry = x);
    }

    public string InvoiceNumber
    {
        get => _state.InvoiceNumber;
        set => SetStateValue(_state.InvoiceNumber, value, x => _state.InvoiceNumber = x);
    }

    public string WaitTimeMinutesText
    {
        get => _state.WaitTimeMinutesText;
        set => SetStateValue(_state.WaitTimeMinutesText, value, x => _state.WaitTimeMinutesText = x);
    }

    public string? DamageNotes
    {
        get => _state.DamageNotes;
        set => SetStateValue(_state.DamageNotes, value, x => _state.DamageNotes = x);
    }

    public string SelectedTrailerSummary
    {
        get
        {
            var t1 = SelectedTrailer1?.DisplayName;
            var t2 = SelectedTrailer2?.DisplayName;

            if (string.IsNullOrWhiteSpace(t1) && string.IsNullOrWhiteSpace(t2))
                return "No trailers selected";

            if (!string.IsNullOrWhiteSpace(t1) && string.IsNullOrWhiteSpace(t2))
                return $"Trailer 1: {t1}";

            if (string.IsNullOrWhiteSpace(t1) && !string.IsNullOrWhiteSpace(t2))
                return $"Trailer 2: {t2}";

            return $"Trailer 1: {t1}, Trailer 2: {t2}";
        }
    }

    public RateType RateType
    {
        get => _state.RateType;
        set
        {
            if (_state.RateType == value)
                return;

            _state.RateType = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(QuantityLabel));
            OnPropertyChanged(nameof(Total));

            if (_state.RateType is RateType.FixedFee or RateType.Percentage)
                Quantity = 1m;
        }
    }

    public decimal Quantity
    {
        get => _state.Quantity;
        set
        {
            if (_state.RateType is RateType.FixedFee or RateType.Percentage)
                value = 1m;

            if (_state.Quantity == value)
                return;

            _state.Quantity = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Total));
        }
    }

    public decimal RateValue
    {
        get => _state.RateValue;
        set
        {
            if (_state.RateValue == value)
                return;

            _state.RateValue = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Total));
        }
    }

    public string QuantityLabel => RateType switch
    {
        RateType.PerLoad => "Loads",
        RateType.PerPallet => "Pallets",
        RateType.PerTonne => "Tonnes",
        RateType.PerKm => "Km",
        RateType.Hourly => "Hours",
        RateType.FixedFee => "Qty (ignored)",
        RateType.Percentage => "Qty (ignored)",
        _ => "Quantity"
    };

    public decimal Total => RateType switch
    {
        RateType.FixedFee => RateValue,
        RateType.Percentage => 0m,
        _ => RateValue * Quantity
    };

    #endregion

    #region Commands

    public ICommand SaveJobCommand { get; }
    public ICommand CancelCommand { get; }

    #endregion

    #region Public Methods

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("jobId", out var jobIdValue))
        {
            if (jobIdValue is Guid g)
                SetEditingJobId(g);
            else if (jobIdValue is string s && Guid.TryParse(s, out var parsed))
                SetEditingJobId(parsed);
        }

        if (query.TryGetValue("pickupOnly", out var pickupOnlyValue))
        {
            if (pickupOnlyValue is bool b)
                SetPickupOnly(b);
            else if (pickupOnlyValue is string s && bool.TryParse(s, out var parsed))
                SetPickupOnly(parsed);
        }

        if (query.TryGetValue("reviewOnly", out var reviewOnlyValue))
        {
            if (reviewOnlyValue is bool b)
                SetReviewOnly(b);
            else if (reviewOnlyValue is string s && bool.TryParse(s, out var parsed))
                SetReviewOnly(parsed);
        }
    }

    public void SetEditingJobId(Guid? jobId)
    {
        _state.EditingJobId = jobId;
        RaiseModeState();
    }

    public void SetPickupOnly(bool pickupOnly)
    {
        _state.IsPickupOnly = pickupOnly;

        if (pickupOnly)
            _state.IsReviewOnly = false;

        RaiseModeState();
    }

    public void SetReviewOnly(bool reviewOnly)
    {
        _state.IsReviewOnly = reviewOnly;

        if (reviewOnly)
            _state.IsPickupOnly = false;

        RaiseModeState();
    }

    public async Task LoadAsync()
    {
        await SafeRunner.RunAsync(
            async () =>
            {
                if (!IsPageVisible)
                {
                    RefreshFeatureBindings();
                    return;
                }

                if (!_session.IsAuthenticated)
                    await _session.RestoreAsync();

                var ownerUserId = _session.CurrentOwnerId ?? Guid.Empty;
                if (ownerUserId == Guid.Empty)
                {
                    RefreshFeatureBindings();
                    return;
                }

                Drivers.Clear();
                Vehicles.Clear();
                Trailers.Clear();
                Customers.Clear();

                var customers = await _customersApiService.GetCustomersAsync(cancellationToken: default);
                foreach (var customer in customers
                    .OrderBy(c => c.CompanyName)
                    .ThenBy(c => c.ContactName)
                    .Select(c => new CustomerPickerItem
                    {
                        Id = c.Id,
                        CompanyName = c.CompanyName,
                        ContactName = c.ContactName,
                        Email = c.Email,
                        AddressLine1 = c.AddressLine1,
                        City = c.City,
                        Country = c.Country
                    }))
                {
                    Customers.Add(customer);
                }

                if (!IsPickupOnly && !IsReviewOnly)
                {
                    var pickers = await _pickerLoader.LoadAsync();

                    foreach (var driver in pickers.Drivers)
                        Drivers.Add(driver);

                    foreach (var vehicle in pickers.Vehicles)
                        Vehicles.Add(vehicle);

                    foreach (var trailer in pickers.Trailers)
                        Trailers.Add(trailer);
                }

                if (IsEditMode && _state.EditingJobId.HasValue && !_state.IsLoadingExistingJob)
                {
                    _state.IsLoadingExistingJob = true;

                    try
                    {
                        await _editorService.LoadIntoStateAsync(_state, _state.EditingJobId.Value);
                        ApplyStateToSelections();
                        ApplyStateToCustomerSelection();
                        RaiseAllStateBindings();
                    }
                    finally
                    {
                        _state.IsLoadingExistingJob = false;
                    }
                }
                else
                {
                    if (Drivers.Count == 1 && SelectedDriver == null)
                        SelectedDriver = Drivers[0];

                    if (Vehicles.Count == 1 && SelectedVehicle == null)
                        SelectedVehicle = Vehicles[0];
                }

                OnPropertyChanged(nameof(SelectedTrailerSummary));
                OnPropertyChanged(nameof(Total));
                RefreshFeatureBindings();
            },
            _crashLogger,
            "NewJobViewModel.LoadAsync",
            nameof(NewJobPage),
            metadataJson: $"{{\"JobId\":\"{_state.EditingJobId}\",\"IsEditMode\":{IsEditMode.ToString().ToLowerInvariant()},\"IsPickupOnly\":{IsPickupOnly.ToString().ToLowerInvariant()},\"IsReviewOnly\":{IsReviewOnly.ToString().ToLowerInvariant()}}}",
            onError: async ex =>
            {
                await Shell.Current.DisplayAlertAsync("Load failed", ex.Message, "OK");
            });
    }

    #endregion

    #region Private Methods

    private async Task SaveAsync()
    {
        var ownerUserId = _session.CurrentOwnerId ?? Guid.Empty;
        if (ownerUserId == Guid.Empty)
            return;

        if (!IsPickupOnly && !IsReviewOnly)
        {
            if (!await EnsureFeatureEnabledAsync(AppFeature.AddJob))
                return;
        }

        SyncSelections();

        var errors = _validator.ValidateForSave(_state);
        if (errors.Count > 0)
        {
            await Shell.Current.DisplayAlertAsync(
                errors.Any(x => x.Contains("Invalid value", StringComparison.OrdinalIgnoreCase)) ? "Invalid value" : "Missing info",
                errors[0],
                "OK");
            return;
        }

        await SafeRunner.RunAsync(
            async () =>
            {
                await _editorService.SaveAsync(_state);
                await Shell.Current.GoToAsync($"//{AppShell.RouteJobs}");
            },
            _crashLogger,
            "NewJobViewModel.SaveAsync",
            nameof(NewJobPage),
            metadataJson: $"{{\"JobId\":\"{_state.EditingJobId}\",\"IsEditMode\":{IsEditMode.ToString().ToLowerInvariant()},\"IsPickupOnly\":{IsPickupOnly.ToString().ToLowerInvariant()},\"IsReviewOnly\":{IsReviewOnly.ToString().ToLowerInvariant()},\"RateType\":\"{RateType}\",\"RateValue\":{RateValue},\"Quantity\":{Quantity}}}",
            onError: async ex =>
            {
                await Shell.Current.DisplayAlertAsync("Save failed", ex.Message, "OK");
            });
    }

    private void ApplyStateToSelections()
    {
        SelectedDriver = _state.SelectedDriverId.HasValue
            ? Drivers.FirstOrDefault(x => x.Id == _state.SelectedDriverId.Value)
            : null;

        SelectedVehicle = _state.SelectedVehicleId.HasValue
            ? Vehicles.FirstOrDefault(x => x.Id == _state.SelectedVehicleId.Value)
            : null;

        SelectedTrailer1 = _state.SelectedTrailer1Id.HasValue
            ? Trailers.FirstOrDefault(x => x.Id == _state.SelectedTrailer1Id.Value)
            : null;

        SelectedTrailer2 = _state.SelectedTrailer2Id.HasValue
            ? Trailers.FirstOrDefault(x => x.Id == _state.SelectedTrailer2Id.Value)
            : null;
    }

    private void SyncSelections()
    {
        _editorService.SyncSelections(
            _state,
            SelectedDriver,
            SelectedVehicle,
            SelectedTrailer1,
            SelectedTrailer2);
    }

    private void RaiseModeState()
    {
        OnPropertyChanged(nameof(IsEditMode));
        RefreshFeatureBindings();
    }

    private void RaiseAllStateBindings()
    {
        OnPropertyChanged(nameof(ClientCompanyName));
        OnPropertyChanged(nameof(ClientContactName));
        OnPropertyChanged(nameof(ClientEmail));
        OnPropertyChanged(nameof(ClientAddressLine1));
        OnPropertyChanged(nameof(ClientCity));
        OnPropertyChanged(nameof(ClientCountry));
        OnPropertyChanged(nameof(PickupCompany));
        OnPropertyChanged(nameof(PickupAddress));
        OnPropertyChanged(nameof(DeliveryCompany));
        OnPropertyChanged(nameof(DeliveryAddress));
        OnPropertyChanged(nameof(ReferenceNumber));
        OnPropertyChanged(nameof(LoadDescription));
        OnPropertyChanged(nameof(InvoiceNumber));
        OnPropertyChanged(nameof(WaitTimeMinutesText));
        OnPropertyChanged(nameof(DamageNotes));
        OnPropertyChanged(nameof(RateType));
        OnPropertyChanged(nameof(RateValue));
        OnPropertyChanged(nameof(Quantity));
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(SelectedTrailerSummary));
        OnPropertyChanged(nameof(SelectedCustomer));
    }

    private void RefreshFeatureBindings()
    {
        OnPropertyChanged(nameof(IsAddJobVisible));
        OnPropertyChanged(nameof(IsAddJobEnabled));
        OnPropertyChanged(nameof(IsPageVisible));
        OnPropertyChanged(nameof(IsSaveVisible));
        OnPropertyChanged(nameof(IsSaveEnabled));
        OnPropertyChanged(nameof(IsPickupOnly));
        OnPropertyChanged(nameof(IsReviewOnly));
        OnPropertyChanged(nameof(CanEditFullJob));
        OnPropertyChanged(nameof(CanEditPickupDetails));
        OnPropertyChanged(nameof(CanEditReviewDetails));
        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(PageSubtitle));
        OnPropertyChanged(nameof(SaveButtonText));
    }

    private void SetStateValue<T>(
        T current,
        T value,
        Action<T> assign,
        [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(current, value))
            return;

        assign(value);
        OnPropertyChanged(propertyName);

        if (propertyName == nameof(RateValue) || propertyName == nameof(Quantity))
            OnPropertyChanged(nameof(Total));
    }

    #endregion

    #region Helpers

    private void ApplySelectedCustomer()
    {
        _state.SelectedCustomerId = SelectedCustomer?.Id;

        if (SelectedCustomer == null)
            return;

        ClientCompanyName = SelectedCustomer.CompanyName;
        ClientContactName = SelectedCustomer.ContactName;
        ClientEmail = SelectedCustomer.Email;
        ClientAddressLine1 = SelectedCustomer.AddressLine1;
        ClientCity = SelectedCustomer.City;
        ClientCountry = SelectedCustomer.Country;
    }
    private void ApplyStateToCustomerSelection()
    {
        SelectedCustomer = _state.SelectedCustomerId.HasValue
            ? Customers.FirstOrDefault(x => x.Id == _state.SelectedCustomerId.Value)
            : null;
    }
    #endregion
}