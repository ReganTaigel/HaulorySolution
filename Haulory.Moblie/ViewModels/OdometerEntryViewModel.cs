using System.Collections.ObjectModel;
using System.Windows.Input;
using Haulory.Application.Interfaces.Services;
using Haulory.Application.Services;
using Haulory.Domain.Enums;
using Haulory.Mobile.Models;

namespace Haulory.Mobile.ViewModels;

public class OdometerEntryViewModel : BaseViewModel
{
    private readonly IOdometerService _odometerService;
    private readonly ISessionService _sessionService;

    public ObservableCollection<VehicleAssetPickerItem> Assets { get; } = new();

    public List<OdometerReadingType> ReadingTypes { get; } = new()
    {
        OdometerReadingType.StartOfDay,
        OdometerReadingType.EndOfDay,
        OdometerReadingType.ServiceEntry
    };

    private VehicleAssetPickerItem? _selectedAsset;
    public VehicleAssetPickerItem? SelectedAsset
    {
        get => _selectedAsset;
        set
        {
            _selectedAsset = value;
            OnPropertyChanged();

            CurrentOdometerDisplay = value?.CurrentOdometerKm.HasValue == true
                ? $"{value.CurrentOdometerKm.Value:N0} km"
                : "—";
        }
    }

    private OdometerReadingType _selectedReadingType = OdometerReadingType.StartOfDay;
    public OdometerReadingType SelectedReadingType
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

    private string _currentOdometerDisplay = "—";
    public string CurrentOdometerDisplay
    {
        get => _currentOdometerDisplay;
        set
        {
            _currentOdometerDisplay = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoadCommand { get; }
    public ICommand SaveCommand { get; }

    public OdometerEntryViewModel(
        IOdometerService odometerService,
        ISessionService sessionService)
    {
        _odometerService = odometerService;
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

            var assets = await _odometerService.GetAssetsForOwnerAsync(ownerUserId);

            foreach (var asset in assets)
                Assets.Add(VehicleAssetPickerItem.FromEntity(asset));

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
                await Shell.Current.DisplayAlertAsync("Invalid value", "Enter a valid odometer reading.", "OK");
                return;
            }

            if (km < 0)
            {
                await Shell.Current.DisplayAlertAsync("Invalid value", "Odometer reading cannot be negative.", "OK");
                return;
            }

            IsBusy = true;

            Guid? driverId = null;
            Guid? recordedByUserId = GetCurrentUserId();

            await _odometerService.RecordReadingAsync(
                SelectedAsset.Id,
                km,
                SelectedReadingType,
                driverId,
                recordedByUserId,
                Notes,
                updateCurrentOdometer: true);

            await Shell.Current.DisplayAlertAsync("Saved", "Odometer reading recorded.", "OK");

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