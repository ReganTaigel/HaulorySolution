using Haulory.Application.Interfaces.Services;
using Haulory.Mobile.Contracts.Vehicles;
using Haulory.Mobile.Features;
using Haulory.Mobile.Services;
using Haulory.Mobile.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Haulory.Mobile.ViewModels;

public class VehicleCollectionViewModel : BaseViewModel
{
    #region Dependencies

    private readonly VehiclesApiService _vehiclesApiService;
    private readonly ISessionService _session;

    #endregion

    #region State

    public bool IsSubUser =>
        _session.CurrentAccountId.HasValue &&
        _session.CurrentOwnerId.HasValue &&
        _session.CurrentOwnerId.Value != _session.CurrentAccountId.Value;

    public bool IsMainUser =>
        _session.CurrentAccountId.HasValue &&
        _session.CurrentOwnerId.HasValue &&
        _session.CurrentOwnerId.Value == _session.CurrentAccountId.Value;

    public bool IsVehiclesVisible => IsFeatureVisible(AppFeature.Vehicles);
    public bool IsVehiclesEnabled => IsFeatureEnabled(AppFeature.Vehicles);

    public bool IsAddVehicleVisible => IsFeatureVisible(AppFeature.AddVehicle);
    public bool IsAddVehicleEnabled => IsFeatureEnabled(AppFeature.AddVehicle) && IsMainUser;

    #endregion

    #region Collections

    public ObservableCollection<VehicleListItemViewModel> Assets { get; } = new();

    #endregion

    #region Commands

    public ICommand GoToNewVehicleCommand { get; }
    public ICommand RefreshCommand { get; }

    #endregion

    #region Constructor

    public VehicleCollectionViewModel(
        VehiclesApiService vehiclesApiService,
        ISessionService session,
        IFeatureAccessService featureAccessService)
        : base(featureAccessService)
    {
        _vehiclesApiService = vehiclesApiService;
        _session = session;

        GoToNewVehicleCommand = new Command(async () =>
        {
            if (!IsMainUser)
                return;

            await NavigateToFeatureAsync(AppFeature.AddVehicle, nameof(NewVehiclePage));
        });

        RefreshCommand = new Command(async () => await LoadAsync());
    }

    #endregion

    #region Load

    public async Task LoadAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            Assets.Clear();

            if (!IsFeatureEnabled(AppFeature.Vehicles))
            {
                RefreshFeatureBindings();
                return;
            }

            if (!_session.IsAuthenticated)
                await _session.RestoreAsync();

            var ownerUserId = _session.CurrentOwnerId ?? Guid.Empty;
            var accountId = _session.CurrentAccountId ?? Guid.Empty;

            if (ownerUserId == Guid.Empty || accountId == Guid.Empty)
            {
                RefreshFeatureBindings();
                return;
            }

            var assets = await _vehiclesApiService.GetVehiclesAsync();

            IEnumerable<VehicleDto> filteredAssets = assets;

            if (IsSubUser)
            {
                // For now, sub-users still see owner-scoped vehicles from the API.
                // Later, if needed, this can be narrowed to assigned vehicles only.
                filteredAssets = assets;
            }
            else if (IsMainUser)
            {
                filteredAssets = assets.Where(a => a.OwnerUserId == ownerUserId);
            }

            foreach (var asset in filteredAssets
                         .OrderBy(a => a.VehicleSetId)
                         .ThenBy(a => a.UnitNumber)
                         .ThenByDescending(a => a.CreatedUtc))
            {
                Assets.Add(VehicleListItemViewModel.FromDto(asset));
            }

            OnPropertyChanged(nameof(IsMainUser));
            OnPropertyChanged(nameof(IsSubUser));
            RefreshFeatureBindings();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            throw;
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region Private Helpers

    private void RefreshFeatureBindings()
    {
        OnPropertyChanged(nameof(IsVehiclesVisible));
        OnPropertyChanged(nameof(IsVehiclesEnabled));
        OnPropertyChanged(nameof(IsAddVehicleVisible));
        OnPropertyChanged(nameof(IsAddVehicleEnabled));
    }

    private async Task NavigateToFeatureAsync(AppFeature feature, string route)
    {
        if (!await EnsureFeatureEnabledAsync(feature))
            return;

        await Shell.Current.GoToAsync(route);
    }

    #endregion
}