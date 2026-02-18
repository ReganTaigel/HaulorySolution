using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Haulory.Mobile.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Haulory.Mobile.ViewModels;

// Displays the user's saved vehicle assets and allows navigation to create new assets.
public class VehicleCollectionViewModel : BaseViewModel
{
    #region Dependencies

    private readonly IVehicleAssetRepository _assetRepository;

    #endregion

    #region State

    private bool _isBusy;

    // True while the list is loading. Use this to disable UI interactions and show a spinner.
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (_isBusy == value) return;
            _isBusy = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Collections

    // Vehicle assets displayed in the collection list.
    public ObservableCollection<VehicleAsset> Assets { get; } = new();

    #endregion

    #region Commands

    // Navigates to the New Vehicle wizard page.
    public ICommand GoToNewVehicleCommand { get; }

    // Reloads the assets list.
    public ICommand RefreshCommand { get; }

    #endregion

    #region Constructor

    public VehicleCollectionViewModel(IVehicleAssetRepository assetRepository)
    {
        _assetRepository = assetRepository;

        GoToNewVehicleCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(NewVehiclePage)));

        RefreshCommand = new Command(async () => await LoadAsync());
    }

    #endregion

    #region Load

    // Loads all vehicle assets and sorts newest first.
    public async Task LoadAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            Assets.Clear();

            var assets = await _assetRepository.GetAllAsync();

            foreach (var a in assets.OrderByDescending(a => a.CreatedUtc))
                Assets.Add(a);
        }
        catch (Exception ex)
        {
            // Option: expose an ErrorMessage property for the UI, or show an alert from the page.
            System.Diagnostics.Debug.WriteLine(ex);

            // Re-throwing will crash the app if the caller doesn't catch it.
            // If you prefer "no crash", remove this throw and surface a friendly message instead.
            throw;
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion
}
