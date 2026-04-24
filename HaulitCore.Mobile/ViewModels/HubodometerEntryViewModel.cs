
using HaulitCore.Mobile.Services;
using HaulitCore.Domain.Enums;
using HaulitCore.Contracts.Vehicles;
using HaulitCore.Mobile.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using HaulitCore.Application.Interfaces.Services;

namespace HaulitCore.Mobile.ViewModels;

public class HubodometerEntryViewModel : BaseViewModel
{
    private readonly HubodometerApiService _hubodometerApiService;
    private readonly ISessionService _sessionService;

    public ObservableCollection<VehicleAssetPickerItem> Assets { get; } = new();

    public List<HubodometerReadingType> ReadingTypes { get; } = new()
    {
        HubodometerReadingType.StartOfDay,
        HubodometerReadingType.EndOfDay,
        HubodometerReadingType.ServiceEntry
    };

    private VehicleAssetPickerItem? _selectedAsset;
    public VehicleAssetPickerItem? SelectedAsset
    {
        get => _selectedAsset;
        set
        {
            _selectedAsset = value;
            OnPropertyChanged();

            CurrentHubodometerDisplay = value?.CurrentHubodometerKm.HasValue == true
                ? $"{value.CurrentHubodometerKm.Value:N0} km"
                : "—";
        }
    }

    private HubodometerReadingType _selectedReadingType = HubodometerReadingType.StartOfDay;
    public HubodometerReadingType SelectedReadingType
    {
        get => _selectedReadingType;
        set
        {
            _selectedReadingType = value;
            OnPropertyChanged();
        }
    }

    private string _readingKm = string.Empty;
    public string ReadingKm
    {
        get => _readingKm;
        set
        {
            _readingKm = value;
            OnPropertyChanged();
        }
    }

    private string? _notes;
    public string? Notes
    {
        get => _notes;
        set
        {
            _notes = value;
            OnPropertyChanged();
        }
    }

    private string _currentHubodometerDisplay = "—";
    public string CurrentHubodometerDisplay
    {
        get => _currentHubodometerDisplay;
        set
        {
            _currentHubodometerDisplay = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoadCommand { get; }
    public ICommand SaveCommand { get; }

    public HubodometerEntryViewModel(
        HubodometerApiService hubodometerApiService,
        ISessionService sessionService)
    {
        _hubodometerApiService = hubodometerApiService;
        _sessionService = sessionService;

        LoadCommand = new Command(async () => await LoadAsync());
        SaveCommand = new Command(async () => await SaveAsync());
    }

    public async Task LoadAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Assets.Clear();

            var ownerUserId = GetCurrentOwnerUserId();

            if (ownerUserId == Guid.Empty)
            {
                await Shell.Current.DisplayAlertAsync("Session error", "Could not find the current owner account.", "OK");
                return;
            }

            var assets = await _hubodometerApiService.GetAssetsAsync();

            foreach (var asset in assets)
            {
                Assets.Add(new VehicleAssetPickerItem
                {
                    Id = asset.Id,
                    DisplayName = $"{asset.UnitNumber} - {asset.Rego} - {asset.Make} {asset.Model}",
                    CurrentHubodometerKm = asset.HubodometerKm
                });
            }

            if (SelectedAsset != null)
            {
                var refreshedSelected = Assets.FirstOrDefault(x => x.Id == SelectedAsset.Id);
                SelectedAsset = refreshedSelected;
            }
            else if (Assets.Count > 0)
            {
                SelectedAsset = Assets[0];
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task SaveAsync()
    {
        if (IsBusy) return;

        try
        {
            if (SelectedAsset == null)
            {
                await Shell.Current.DisplayAlertAsync("Missing asset", "Please select a vehicle or trailer.", "OK");
                return;
            }

            if (!int.TryParse(ReadingKm, out var km))
            {
                await Shell.Current.DisplayAlertAsync("Invalid value", "Enter a valid Hubodometer reading.", "OK");
                return;
            }

            if (km < 0)
            {
                await Shell.Current.DisplayAlertAsync("Invalid value", "Hubodometer reading cannot be negative.", "OK");
                return;
            }

            IsBusy = true;

            Guid? driverId = null;
            Guid? recordedByUserId = GetCurrentUserId();

            await _hubodometerApiService.RecordReadingAsync(new HubodometerReadingRequest
            {
                VehicleAssetId = SelectedAsset.Id,
                ReadingKm = km,
                ReadingType = SelectedReadingType,
                DriverId = driverId,
                RecordedByUserId = recordedByUserId,
                Notes = Notes,
                UpdateCurrentHubodometer = true
            });

            await Shell.Current.DisplayAlertAsync("Saved", "Hubodometer reading recorded.", "OK");

            ReadingKm = string.Empty;
            Notes = string.Empty;
            OnPropertyChanged(nameof(ReadingKm));
            OnPropertyChanged(nameof(Notes));

            await LoadAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private Guid GetCurrentOwnerUserId()
    {
        return _sessionService.CurrentOwnerId ?? Guid.Empty;
    }

    private Guid? GetCurrentUserId()
    {
        var currentUserId = _sessionService.CurrentAccountId;

        if (!currentUserId.HasValue || currentUserId.Value == Guid.Empty)
            return null;

        return currentUserId.Value;
    }
}