using Haulory.Application.Interfaces.Services;
using Haulory.Domain.Enums;
using Haulory.Mobile.Contracts.Jobs;
using Haulory.Mobile.Features;
using Haulory.Mobile.Services;
using Haulory.Mobile.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Haulory.Mobile.ViewModels;

public class NewJobViewModel : BaseViewModel
{
    private readonly JobsApiService _jobsApiService;
    private readonly DriversApiService _driversApiService;
    private readonly VehiclesApiService _vehiclesApiService;
    private readonly ISessionService _session;

    private Guid? _editingJobId;
    private bool _isLoadingExistingJob;

    public IReadOnlyList<RateType> RateTypes { get; } =
        Enum.GetValues(typeof(RateType)).Cast<RateType>().ToList();

    public ObservableCollection<DriverPickerItem> Drivers { get; } = new();
    public ObservableCollection<VehiclePickerItem> Vehicles { get; } = new();
    public ObservableCollection<VehiclePickerItem> Trailers { get; } = new();

    private DriverPickerItem? _selectedDriver;
    private VehiclePickerItem? _selectedVehicle;
    private VehiclePickerItem? _selectedTrailer1;
    private VehiclePickerItem? _selectedTrailer2;

    private RateType _rateType;
    private decimal _quantity = 1m;
    private decimal _rateValue;

    private string _pickupCompany = string.Empty;
    private string _pickupAddress = string.Empty;
    private string _deliveryCompany = string.Empty;
    private string _deliveryAddress = string.Empty;
    private string _referenceNumber = string.Empty;
    private string _loadDescription = string.Empty;

    private string _clientCompanyName = string.Empty;
    private string? _clientContactName;
    private string? _clientEmail;
    private string _clientAddressLine1 = string.Empty;
    private string _clientCity = string.Empty;
    private string _clientCountry = "New Zealand";

    private string _invoiceNumber = string.Empty;

    public bool IsAddJobVisible => IsFeatureVisible(AppFeature.AddJob);
    public bool IsAddJobEnabled => IsFeatureEnabled(AppFeature.AddJob);

    public bool IsEditMode => _editingJobId.HasValue;

    public string PageTitle => IsEditMode ? "Edit job" : "New job";

    public string PageSubtitle => IsEditMode
        ? "Update pickup, delivery, assignment, and billing details"
        : "Enter pickup, delivery, assignment, and billing details";

    public string SaveButtonText => IsEditMode ? "Update job" : "Save job";

    public DriverPickerItem? SelectedDriver
    {
        get => _selectedDriver;
        set => SetProperty(ref _selectedDriver, value);
    }

    public VehiclePickerItem? SelectedVehicle
    {
        get => _selectedVehicle;
        set
        {
            if (SetProperty(ref _selectedVehicle, value) && _selectedVehicle == null)
            {
                SelectedTrailer1 = null;
                SelectedTrailer2 = null;
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
        }
    }

    public string PickupCompany
    {
        get => _pickupCompany;
        set => SetProperty(ref _pickupCompany, value);
    }

    public string PickupAddress
    {
        get => _pickupAddress;
        set => SetProperty(ref _pickupAddress, value);
    }

    public string DeliveryCompany
    {
        get => _deliveryCompany;
        set => SetProperty(ref _deliveryCompany, value);
    }

    public string DeliveryAddress
    {
        get => _deliveryAddress;
        set => SetProperty(ref _deliveryAddress, value);
    }

    public string ReferenceNumber
    {
        get => _referenceNumber;
        set => SetProperty(ref _referenceNumber, value);
    }

    public string LoadDescription
    {
        get => _loadDescription;
        set => SetProperty(ref _loadDescription, value);
    }

    public string ClientCompanyName
    {
        get => _clientCompanyName;
        set => SetProperty(ref _clientCompanyName, value);
    }

    public string? ClientContactName
    {
        get => _clientContactName;
        set => SetProperty(ref _clientContactName, value);
    }

    public string? ClientEmail
    {
        get => _clientEmail;
        set => SetProperty(ref _clientEmail, value);
    }

    public string ClientAddressLine1
    {
        get => _clientAddressLine1;
        set => SetProperty(ref _clientAddressLine1, value);
    }

    public string ClientCity
    {
        get => _clientCity;
        set => SetProperty(ref _clientCity, value);
    }

    public string ClientCountry
    {
        get => _clientCountry;
        set => SetProperty(ref _clientCountry, value);
    }

    public string InvoiceNumber
    {
        get => _invoiceNumber;
        set => SetProperty(ref _invoiceNumber, value);
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
        get => _rateType;
        set
        {
            if (_rateType == value)
                return;

            _rateType = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(QuantityLabel));
            OnPropertyChanged(nameof(Total));

            if (_rateType is RateType.FixedFee or RateType.Percentage)
                Quantity = 1m;
        }
    }

    public decimal Quantity
    {
        get => _quantity;
        set
        {
            if (_rateType is RateType.FixedFee or RateType.Percentage)
                value = 1m;

            if (_quantity == value)
                return;

            _quantity = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Total));
        }
    }

    public decimal RateValue
    {
        get => _rateValue;
        set
        {
            if (_rateValue == value)
                return;

            _rateValue = value;
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

    public ICommand SaveJobCommand { get; }
    public ICommand CancelCommand { get; }

    public NewJobViewModel(
        JobsApiService jobsApiService,
        DriversApiService driversApiService,
        VehiclesApiService vehiclesApiService,
        ISessionService session,
        IFeatureAccessService featureAccessService)
        : base(featureAccessService)
    {
        _jobsApiService = jobsApiService;
        _driversApiService = driversApiService;
        _vehiclesApiService = vehiclesApiService;
        _session = session;

        SaveJobCommand = new Command(async () => await SaveAsync());
        CancelCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(JobsCollectionPage)));
        RateType = RateType.PerLoad;
    }

    public void SetEditingJobId(Guid? jobId)
    {
        _editingJobId = jobId;

        OnPropertyChanged(nameof(IsEditMode));
        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(PageSubtitle));
        OnPropertyChanged(nameof(SaveButtonText));
    }

    public async Task LoadAsync()
    {
        if (!IsFeatureEnabled(AppFeature.AddJob))
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

        var drivers = await _driversApiService.GetDriversAsync();
        var vehicles = await _vehiclesApiService.GetVehiclesAsync();
        var trailers = await _vehiclesApiService.GetTrailersAsync();

        foreach (var d in drivers.OrderBy(d => d.LastName).ThenBy(d => d.FirstName))
        {
            Drivers.Add(new DriverPickerItem
            {
                Id = d.Id,
                UserId = d.UserId,
                DisplayName = $"{d.FirstName} {d.LastName}"
            });
        }

        foreach (var v in vehicles.OrderBy(v => v.Rego))
        {
            Vehicles.Add(new VehiclePickerItem
            {
                Id = v.Id,
                DisplayName = $"{v.UnitNumber} - {v.Rego} - {v.Make} {v.Model}"
            });
        }

        foreach (var t in trailers.OrderBy(t => t.Rego))
        {
            Trailers.Add(new VehiclePickerItem
            {
                Id = t.Id,
                DisplayName = $"{t.UnitNumber} - {t.Rego} - {t.Make} {t.Model}"
            });
        }

        if (IsEditMode && _editingJobId.HasValue && !_isLoadingExistingJob)
        {
            _isLoadingExistingJob = true;

            try
            {
                await LoadExistingJobAsync(_editingJobId.Value);
            }
            finally
            {
                _isLoadingExistingJob = false;
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
    }

    private async Task LoadExistingJobAsync(Guid jobId)
    {
        var job = await _jobsApiService.GetJobByIdAsync(jobId);
        if (job == null)
        {
            await Shell.Current.DisplayAlertAsync("Not found", "The selected job could not be loaded.", "OK");
            return;
        }

        ClientCompanyName = job.ClientCompanyName ?? string.Empty;
        ClientContactName = job.ClientContactName;
        ClientEmail = job.ClientEmail;
        ClientAddressLine1 = job.ClientAddressLine1 ?? string.Empty;
        ClientCity = job.ClientCity ?? string.Empty;
        ClientCountry = string.IsNullOrWhiteSpace(job.ClientCountry)
            ? "New Zealand"
            : job.ClientCountry;

        PickupCompany = job.PickupCompany ?? string.Empty;
        PickupAddress = job.PickupAddress ?? string.Empty;

        DeliveryCompany = job.DeliveryCompany ?? string.Empty;
        DeliveryAddress = job.DeliveryAddress ?? string.Empty;

        ReferenceNumber = job.ReferenceNumber ?? string.Empty;
        LoadDescription = job.LoadDescription ?? string.Empty;

        InvoiceNumber = job.InvoiceNumber ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(job.RateType) &&
            Enum.TryParse<RateType>(job.RateType, true, out var parsedRateType))
        {
            RateType = parsedRateType;
        }
        else
        {
            RateType = RateType.PerLoad;
        }

        RateValue = job.RateValue;
        Quantity = job.Quantity;

        SelectedDriver = null;
        SelectedVehicle = null;
        SelectedTrailer1 = null;
        SelectedTrailer2 = null;

        if (job.DriverId.HasValue)
            SelectedDriver = Drivers.FirstOrDefault(x => x.Id == job.DriverId.Value);

        if (job.VehicleAssetId.HasValue)
            SelectedVehicle = Vehicles.FirstOrDefault(x => x.Id == job.VehicleAssetId.Value);

        if (job.TrailerAssetIds != null && job.TrailerAssetIds.Count > 0)
            SelectedTrailer1 = Trailers.FirstOrDefault(x => x.Id == job.TrailerAssetIds[0]);

        if (job.TrailerAssetIds != null && job.TrailerAssetIds.Count > 1)
            SelectedTrailer2 = Trailers.FirstOrDefault(x => x.Id == job.TrailerAssetIds[1]);

        OnPropertyChanged(nameof(SelectedTrailerSummary));
        OnPropertyChanged(nameof(Total));
    }

    private async Task SaveAsync()
    {
        if (!await EnsureFeatureEnabledAsync(AppFeature.AddJob))
            return;

        var ownerUserId = _session.CurrentOwnerId ?? Guid.Empty;
        if (ownerUserId == Guid.Empty)
            return;

        if (string.IsNullOrWhiteSpace(ClientCompanyName))
        {
            await Shell.Current.DisplayAlertAsync("Missing info", "Client company name is required.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(ClientAddressLine1))
        {
            await Shell.Current.DisplayAlertAsync("Missing info", "Client address is required.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(ClientCity))
        {
            await Shell.Current.DisplayAlertAsync("Missing info", "Client city is required.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(ClientCountry))
        {
            await Shell.Current.DisplayAlertAsync("Missing info", "Client country is required.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(PickupCompany))
        {
            await Shell.Current.DisplayAlertAsync("Missing info", "Pickup company is required.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(PickupAddress))
        {
            await Shell.Current.DisplayAlertAsync("Missing info", "Pickup address is required.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(DeliveryCompany))
        {
            await Shell.Current.DisplayAlertAsync("Missing info", "Delivery company is required.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(DeliveryAddress))
        {
            await Shell.Current.DisplayAlertAsync("Missing info", "Delivery address is required.", "OK");
            return;
        }

        if ((SelectedTrailer1 != null || SelectedTrailer2 != null) && SelectedVehicle == null)
        {
            await Shell.Current.DisplayAlertAsync(
                "Missing info",
                "Select a vehicle (power unit) before assigning trailers.",
                "OK");
            return;
        }

        var trailerIds = new[] { SelectedTrailer1?.Id, SelectedTrailer2?.Id }
            .Where(id => id.HasValue && id.Value != Guid.Empty)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        if (trailerIds.Count > 2)
        {
            await Shell.Current.DisplayAlertAsync(
                "Too many trailers",
                "A maximum of 2 trailers can be assigned to a job.",
                "OK");
            return;
        }

        if (IsEditMode && _editingJobId.HasValue)
        {
            var request = new UpdateJobRequest
            {
                ClientCompanyName = ClientCompanyName,
                ClientContactName = ClientContactName,
                ClientEmail = ClientEmail,
                ClientAddressLine1 = ClientAddressLine1,
                ClientCity = ClientCity,
                ClientCountry = ClientCountry,

                PickupCompany = PickupCompany,
                PickupAddress = PickupAddress,
                DeliveryCompany = DeliveryCompany,
                DeliveryAddress = DeliveryAddress,

                ReferenceNumber = ReferenceNumber,
                LoadDescription = LoadDescription,

                RateType = RateType,
                RateValue = RateValue,
                Quantity = Quantity,

                DriverId = SelectedDriver?.Id,
                VehicleAssetId = SelectedVehicle?.Id,
                AssignedToUserId = SelectedDriver?.UserId,
                TrailerAssetIds = trailerIds
            };

            var result = await _jobsApiService.UpdateJobAsync(_editingJobId.Value, request);
        }
        else
        {
            var request = new CreateJobRequest
            {
                ClientCompanyName = ClientCompanyName,
                ClientContactName = ClientContactName,
                ClientEmail = ClientEmail,
                ClientAddressLine1 = ClientAddressLine1,
                ClientCity = ClientCity,
                ClientCountry = ClientCountry,

                PickupCompany = PickupCompany,
                PickupAddress = PickupAddress,
                DeliveryCompany = DeliveryCompany,
                DeliveryAddress = DeliveryAddress,

                ReferenceNumber = ReferenceNumber,
                LoadDescription = LoadDescription,

                RateType = RateType,
                RateValue = RateValue,
                Quantity = Quantity,

                DriverId = SelectedDriver?.Id,
                VehicleAssetId = SelectedVehicle?.Id,
                AssignedToUserId = SelectedDriver?.UserId,
                TrailerAssetIds = trailerIds
            };

            var result = await _jobsApiService.CreateJobAsync(request);
        }
        await Shell.Current.GoToAsync(nameof(JobsCollectionPage));
    }

    private void RefreshFeatureBindings()
    {
        OnPropertyChanged(nameof(IsAddJobVisible));
        OnPropertyChanged(nameof(IsAddJobEnabled));
    }
}