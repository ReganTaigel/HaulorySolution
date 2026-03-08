using Haulory.Application.Features.Jobs;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Domain.Entities;
using Haulory.Domain.Enums;
using Haulory.Mobile.Contracts.Jobs;
using Haulory.Mobile.Services;
using Haulory.Mobile.Views;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Haulory.Mobile.ViewModels;

public class NewJobViewModel : BaseViewModel
{
    private readonly JobsApiService _jobsApiService;
    private readonly IDriverRepository _driverRepo;
    private readonly IVehicleAssetRepository _vehicleRepo;
    private readonly ISessionService _session;

    public IReadOnlyList<RateType> RateTypes { get; } =
        Enum.GetValues(typeof(RateType)).Cast<RateType>().ToList();

    public ObservableCollection<Driver> Drivers { get; } = new();
    public ObservableCollection<VehicleAsset> Vehicles { get; } = new();
    public ObservableCollection<VehicleAsset> Trailers { get; } = new();

    private Driver? _selectedDriver;
    private VehicleAsset? _selectedVehicle;
    private VehicleAsset? _selectedTrailer1;
    private VehicleAsset? _selectedTrailer2;

    private RateType _rateType;
    private decimal _quantity = 1m;
    private decimal _rateValue;

    public string PickupCompany { get; set; } = string.Empty;
    public string PickupAddress { get; set; } = string.Empty;

    public string DeliveryCompany { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;

    public string ReferenceNumber { get; set; } = string.Empty;
    public string LoadDescription { get; set; } = string.Empty;

    public string ClientCompanyName { get; set; } = string.Empty;
    public string? ClientContactName { get; set; }
    public string? ClientEmail { get; set; }
    public string ClientAddressLine1 { get; set; } = string.Empty;
    public string ClientCity { get; set; } = string.Empty;
    public string ClientCountry { get; set; } = "New Zealand";

    public Driver? SelectedDriver
    {
        get => _selectedDriver;
        set
        {
            if (_selectedDriver == value) return;
            _selectedDriver = value;
            OnPropertyChanged();
        }
    }

    public VehicleAsset? SelectedVehicle
    {
        get => _selectedVehicle;
        set
        {
            if (_selectedVehicle == value) return;
            _selectedVehicle = value;
            OnPropertyChanged();

            if (_selectedVehicle == null)
            {
                SelectedTrailer1 = null;
                SelectedTrailer2 = null;
            }
        }
    }

    public VehicleAsset? SelectedTrailer1
    {
        get => _selectedTrailer1;
        set
        {
            if (_selectedTrailer1 == value) return;

            if (value != null && SelectedTrailer2?.Id == value.Id)
                SelectedTrailer2 = null;

            _selectedTrailer1 = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedTrailerSummary));
        }
    }

    public VehicleAsset? SelectedTrailer2
    {
        get => _selectedTrailer2;
        set
        {
            if (_selectedTrailer2 == value) return;

            if (value != null && SelectedTrailer1?.Id == value.Id)
                return;

            _selectedTrailer2 = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedTrailerSummary));
        }
    }

    public string SelectedTrailerSummary
    {
        get
        {
            var t1 = SelectedTrailer1?.Rego;
            var t2 = SelectedTrailer2?.Rego;

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
            if (_rateType == value) return;

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

            if (_quantity == value) return;
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
            if (_rateValue == value) return;
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
        IDriverRepository driverRepo,
        IVehicleAssetRepository vehicleRepo,
        ISessionService session)
    {
        _jobsApiService = jobsApiService;
        _driverRepo = driverRepo;
        _vehicleRepo = vehicleRepo;
        _session = session;

        SaveJobCommand = new Command(async () => await SaveAsync());
        CancelCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(DashboardPage)));

        RateType = RateType.PerLoad;
    }

    public async Task LoadAsync()
    {
        var ownerUserId = _session.CurrentOwnerId ?? Guid.Empty;
        if (ownerUserId == Guid.Empty)
            return;

        Drivers.Clear();
        var drivers = await _driverRepo.GetAllByOwnerUserIdAsync(ownerUserId);
        foreach (var d in drivers.OrderBy(d => d.LastName).ThenBy(d => d.FirstName))
            Drivers.Add(d);

        Vehicles.Clear();
        Trailers.Clear();

        var allAssets = await _vehicleRepo.GetAllAsync();

        foreach (var v in allAssets
                     .Where(a => a.OwnerUserId == ownerUserId)
                     .Where(a => a.Kind == AssetKind.PowerUnit)
                     .OrderBy(a => a.Rego))
            Vehicles.Add(v);

        foreach (var t in allAssets
                     .Where(a => a.OwnerUserId == ownerUserId)
                     .Where(a => a.Kind == AssetKind.Trailer)
                     .OrderBy(a => a.Rego))
            Trailers.Add(t);

        if (Drivers.Count == 1 && SelectedDriver == null)
            SelectedDriver = Drivers[0];

        if (Vehicles.Count == 1 && SelectedVehicle == null)
            SelectedVehicle = Vehicles[0];

        OnPropertyChanged(nameof(SelectedTrailerSummary));
    }

    private async Task SaveAsync()
    {
        var ownerUserId = _session.CurrentOwnerId ?? Guid.Empty;
        if (ownerUserId == Guid.Empty)
            return;

        if (string.IsNullOrWhiteSpace(ClientCompanyName))
        {
            await Shell.Current.DisplayAlertAsync("Missing info", "Client company name is required.", "OK");
            return;
        }

        if ((SelectedTrailer1 != null || SelectedTrailer2 != null) && SelectedVehicle == null)
        {
            await Shell.Current.DisplayAlertAsync("Missing info", "Select a vehicle (power unit) before assigning trailers.", "OK");
            return;
        }

        var trailerIds = new[]
        {
            SelectedTrailer1?.Id,
            SelectedTrailer2?.Id
        }
        .Where(id => id.HasValue && id.Value != Guid.Empty)
        .Select(id => id!.Value)
        .Distinct()
        .ToList();

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

        await Shell.Current.DisplayAlertAsync(
            "Job Saved",
            $"Job created: {result.ReferenceNumber}",
            "OK");

        await Shell.Current.GoToAsync(nameof(JobsCollectionPage));
    }
}