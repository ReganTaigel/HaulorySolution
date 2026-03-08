using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Domain.Entities;
using Haulory.Mobile.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Haulory.Mobile.ViewModels;

public class VehicleCollectionViewModel : BaseViewModel
{
    #region Dependencies

    private readonly IVehicleAssetRepository _assetRepository;
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

    #endregion

    #region Collections

    public ObservableCollection<VehicleAsset> Assets { get; } = new();

    #endregion

    #region Commands

    public ICommand GoToNewVehicleCommand { get; }
    public ICommand RefreshCommand { get; }

    #endregion

    #region Constructor

    public VehicleCollectionViewModel(
        IVehicleAssetRepository assetRepository,
        ISessionService session)
    {
        _assetRepository = assetRepository;
        _session = session;

        GoToNewVehicleCommand = new Command(async () =>
        {
            if (!IsMainUser) return;
            await Shell.Current.GoToAsync(nameof(NewVehiclePage));
        });

        RefreshCommand = new Command(async () => await LoadAsync());
    }

    #endregion

    #region Load

    public async Task LoadAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            Assets.Clear();

            var ownerUserId = _session.CurrentOwnerId ?? Guid.Empty;
            var accountId = _session.CurrentAccountId ?? Guid.Empty;

            if (ownerUserId == Guid.Empty || accountId == Guid.Empty)
                return;

            var assets = IsSubUser
                ? await _assetRepository.GetActiveAssetsAssignedToUserAsync(ownerUserId, accountId)
                : await _assetRepository.GetAllAsync();

            // Main should only see their own assets
            if (IsMainUser)
                assets = assets.Where(a => a.OwnerUserId == ownerUserId).ToList();

            foreach (var a in assets.OrderByDescending(a => a.CreatedUtc))
                Assets.Add(a);

            OnPropertyChanged(nameof(IsMainUser));
            OnPropertyChanged(nameof(IsSubUser));
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
}