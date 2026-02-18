using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Haulory.Application.Features.Jobs;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Domain.Entities;
using Haulory.Domain.Enums;
using Haulory.Mobile.Views;
using Microsoft.Maui.Controls;

namespace Haulory.Mobile.ViewModels;

public class NewJobViewModel : BaseViewModel
{
    #region Dependencies

    private readonly CreateJobHandler _handler;
    private readonly IDriverRepository _driverRepo;
    private readonly IVehicleAssetRepository _vehicleRepo;
    private readonly ISessionService _session;

    #endregion

    #region Picker Sources

    public IReadOnlyList<RateType> RateTypes { get; } =
        Enum.GetValues(typeof(RateType)).Cast<RateType>().ToList();

    public ObservableCollection<Driver> Drivers { get; } = new();
    public ObservableCollection<VehicleAsset> Vehicles { get; } = new();

    #endregion

    #region State

    private Driver? _selectedDriver;
    private VehicleAsset? _selectedVehicle;

    private RateType _rateType;
    private decimal _quantity = 1m;
    private decimal _rateValue;

    #endregion

    #region Job Fields

    public string PickupCompany { get; set; } = string.Empty;
    public string PickupAddress { get; set; } = string.Empty;

    public string DeliveryCompany { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;

    public string ReferenceNumber { get; set; } = string.Empty;
    public string LoadDescription { get; set; } = string.Empty;

    #endregion

    #region Selected Items

    public Driver? SelectedDriver
    {
        get => _selectedDriver;
        set
        {
            if (_selectedDriver == value)
                return;

            _selectedDriver = value;
            OnPropertyChanged();
        }
    }

    public VehicleAsset? SelectedVehicle
    {
        get => _selectedVehicle;
        set
        {
            if (_selectedVehicle == value)
                return;

            _selectedVehicle = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Billing

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

            // For fixed fee / percentage, quantity is ignored so force 1
            if (_rateType is RateType.FixedFee or RateType.Percentage)
                Quantity = 1m;
        }
    }

    public decimal Quantity
    {
        get => _quantity;
        set
        {
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
        RateType.Percentage => 0m, // later: apply to base amount
        _ => RateValue * Quantity
    };

    #endregion

    #region Commands

    public ICommand SaveJobCommand { get; }
    public ICommand CancelCommand { get; }

    #endregion

    #region Constructor

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

        // Returns user to dashboard
        CancelCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(DashboardPage)));

        // Default rate type
        RateType = RateType.PerLoad;
    }

    #endregion

    #region Load

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
            .Where(a => a.Kind == AssetKind.PowerUnit)
            .OrderBy(a => a.Rego);

        foreach (var v in vehicles)
            Vehicles.Add(v);

        // Auto-select if there's only one choice
        if (Drivers.Count == 1 && SelectedDriver == null)
            SelectedDriver = Drivers[0];

        if (Vehicles.Count == 1 && SelectedVehicle == null)
            SelectedVehicle = Vehicles[0];
    }

    #endregion

    #region Save

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

    #endregion
}
