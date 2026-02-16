using System.Collections.ObjectModel;
using System.Windows.Input;
using Haulory.Application.Features.Jobs;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Domain.Entities;
using Haulory.Domain.Enums;
using Haulory.Mobile.Views;

namespace Haulory.Mobile.ViewModels;

public class NewJobViewModel : BaseViewModel
{
    private readonly CreateJobHandler _handler;
    private readonly IDriverRepository _driverRepo;
    private readonly IVehicleAssetRepository _vehicleRepo;
    private readonly ISessionService _session;

    public IReadOnlyList<RateType> RateTypes { get; } =
        Enum.GetValues(typeof(RateType)).Cast<RateType>().ToList();

    // Job fields
    public string PickupCompany { get; set; } = string.Empty;
    public string PickupAddress { get; set; } = string.Empty;
    public string DeliveryCompany { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string LoadDescription { get; set; } = string.Empty;

    // Picker sources
    public ObservableCollection<Driver> Drivers { get; } = new();
    private Driver? _selectedDriver;
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

    public ObservableCollection<VehicleAsset> Vehicles { get; } = new();
    private VehicleAsset? _selectedVehicle;
    public VehicleAsset? SelectedVehicle
    {
        get => _selectedVehicle;
        set
        {
            if (_selectedVehicle == value) return;
            _selectedVehicle = value;
            OnPropertyChanged();
        }
    }

    // Billing
    private RateType _rateType;
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

    private decimal _quantity = 1m;
    public decimal Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity == value) return;
            _quantity = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Total));
        }
    }

    private decimal _rateValue;
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
        RateType.Percentage => 0m, // later: apply to base
        _ => RateValue * Quantity
    };

    // Commands
    public ICommand SaveJobCommand { get; }
    public ICommand CancelCommand { get; }

    public NewJobViewModel(
        CreateJobHandler handler,
        IDriverRepository driverRepo,
        IVehicleAssetRepository vehicleRepo,
        ISessionService session)
    {
        _handler = handler;
        _driverRepo = driverRepo;
        _vehicleRepo = vehicleRepo;
        _session = session;

        SaveJobCommand = new Command(async () => await SaveAsync());
        CancelCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(DashboardPage)));

        RateType = RateType.PerLoad;
    }

    // Call this from Page.OnAppearing
    public async Task LoadAsync()
    {
        var ownerUserId = _session.CurrentAccountId ?? Guid.Empty;
        if (ownerUserId == Guid.Empty)
            return;

        // Drivers (owned by account)
        Drivers.Clear();
        var drivers = await _driverRepo.GetAllByOwnerUserIdAsync(ownerUserId);
        foreach (var d in drivers.OrderBy(d => d.LastName).ThenBy(d => d.FirstName))
            Drivers.Add(d);

        // Vehicles (owned by account) - show power units only
        Vehicles.Clear();
        var allAssets = await _vehicleRepo.GetAllAsync();

        var vehicles = allAssets
            .Where(a => a.OwnerUserId == ownerUserId)
            .Where(a => a.Kind == AssetKind.PowerUnit) // better than UnitNumber == 1
            .OrderBy(a => a.Rego);

        foreach (var v in vehicles)
            Vehicles.Add(v);

        // Optional: auto select if there is only one option
        if (Drivers.Count == 1 && SelectedDriver == null)
            SelectedDriver = Drivers[0];

        if (Vehicles.Count == 1 && SelectedVehicle == null)
            SelectedVehicle = Vehicles[0];
    }

    private async Task SaveAsync()
    {
        var ownerUserId = _session.CurrentAccountId ?? Guid.Empty;
        if (ownerUserId == Guid.Empty)
            return;

        await _handler.HandleAsync(new CreateJobCommand(
            ownerUserId,
            PickupCompany,
            PickupAddress,
            DeliveryCompany,
            DeliveryAddress,
            ReferenceNumber,
            LoadDescription,
            RateType,
            RateValue,
            Quantity,
            DriverId: SelectedDriver?.Id,
            VehicleAssetId: SelectedVehicle?.Id
        ));

        await Shell.Current.GoToAsync(nameof(JobsCollectionPage));
    }
}
