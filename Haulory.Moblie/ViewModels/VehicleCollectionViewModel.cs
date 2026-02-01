using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Haulory.Mobile.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Haulory.Mobile.ViewModels;

public class VehicleCollectionViewModel : BaseViewModel
{
    private readonly IVehicleAssetRepository _assetRepository;

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    public ObservableCollection<VehicleAsset> Assets { get; } = new();

    public ICommand GoToNewVehicleCommand { get; }
    public ICommand RefreshCommand { get; }

    public VehicleCollectionViewModel(IVehicleAssetRepository assetRepository)
    {
        _assetRepository = assetRepository;

        GoToNewVehicleCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(NewVehiclePage)));

        RefreshCommand = new Command(async () => await LoadAsync());
    }
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
            // optionally expose an ErrorMessage property or alert in page
            System.Diagnostics.Debug.WriteLine(ex);
            throw;
        }
        finally
        {
            IsBusy = false;
        }
    }

}
