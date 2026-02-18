using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Haulory.Application.Features.Drivers;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Domain.Entities;
using Haulory.Mobile.Views;
using Microsoft.Maui.Controls;

namespace Haulory.Mobile.ViewModels;

public class DriverCollectionViewModel : BaseViewModel
{
    #region Dependencies

    private readonly IDriverRepository _driverRepository;
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly ISessionService _sessionService;
    private readonly CreateDriverFromUserHandler _createDriverFromUserHandler;
    private readonly IDriverInductionRepository _driverInductionRepository;

    #endregion

    #region State

    private bool _isBusy;
    private Driver? _mainDriver;
    private bool _isMainComplete;

    #endregion

    #region Bindable Properties

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
        }
    }

    // Gate: only allow adding sub-drivers once main driver profile is complete
    public bool ShowAddDriver => _isMainComplete;

    public ObservableCollection<DriverListItem> Drivers { get; } = new();

    #endregion

    #region Commands

    public ICommand AddDriverCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ManageInductionsCommand { get; }

    #endregion

    #region Constructor

    public DriverCollectionViewModel(
        IDriverRepository driverRepository,
        IUserAccountRepository userAccountRepository,
        ISessionService sessionService,
        CreateDriverFromUserHandler createDriverFromUserHandler,
        IDriverInductionRepository driverInductionRepository)
    {
        _driverRepository = driverRepository;
        _userAccountRepository = userAccountRepository;
        _sessionService = sessionService;
        _createDriverFromUserHandler = createDriverFromUserHandler;
        _driverInductionRepository = driverInductionRepository;

        AddDriverCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(NewDriverPage)));

        ManageInductionsCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(ManageInductionsPage)));

        RefreshCommand = new Command(async () => await LoadAsync());
    }

    #endregion

    #region Public Methods

    public async Task LoadAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            Drivers.Clear();

            // Ensure session restored
            if (!_sessionService.IsAuthenticated)
                await _sessionService.RestoreAsync();

            var ownerUserId = _sessionService.CurrentAccountId ?? Guid.Empty;

            if (!_sessionService.IsAuthenticated || ownerUserId == Guid.Empty)
            {
                _mainDriver = null;
                _isMainComplete = false;
                RaiseGate();
                return;
            }

            // Load all drivers for this owner
            var drivers = await _driverRepository.GetAllByOwnerUserIdAsync(ownerUserId);

            // Resolve main driver (driver linked to the main user account)
            var existingMain = drivers.FirstOrDefault(d =>
                d.UserId.HasValue && d.UserId.Value == ownerUserId);

            // Create main driver if missing
            if (existingMain == null)
            {
                var account = await _userAccountRepository.GetByIdAsync(ownerUserId);
                if (account == null)
                {
                    _mainDriver = null;
                    _isMainComplete = false;
                    RaiseGate();
                    return;
                }

                existingMain = await _createDriverFromUserHandler.HandleAsync(
                    new CreateDriverFromUserCommand(
                        ownerUserId,
                        account.FirstName ?? string.Empty,
                        account.LastName ?? string.Empty,
                        account.Email ?? string.Empty
                    )
                );

                // Reload after creation
                drivers = await _driverRepository.GetAllByOwnerUserIdAsync(ownerUserId);
            }

            // Ensure main appears (safety)
            if (existingMain != null && drivers.All(d => d.Id != existingMain.Id))
                drivers.Insert(0, existingMain);

            // Get expiring induction counts (within 30 days)
            var expiringSoon =
                await _driverInductionRepository
                    .CountExpiringSoonByDriverAsync(ownerUserId, 30);

            // Populate UI collection
            foreach (var d in drivers
                .OrderByDescending(d => d.UserId.HasValue)
                .ThenBy(d => d.LastName ?? string.Empty)
                .ThenBy(d => d.FirstName ?? string.Empty)
                .ThenBy(d => d.Email ?? string.Empty))
            {
                var expSoonCount =
                    expiringSoon.TryGetValue(d.Id, out var count)
                        ? count
                        : 0;

                Drivers.Add(new DriverListItem(d, expSoonCount));
            }

            _mainDriver = existingMain;

            // Gate based on main driver emergency contact completion
            _isMainComplete =
                _mainDriver != null &&
                _mainDriver.EmergencyContact != null &&
                _mainDriver.EmergencyContact.IsSet;

            RaiseGate();
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region UI Helpers

    private void RaiseGate()
    {
        OnPropertyChanged(nameof(ShowAddDriver));
    }

    #endregion
}
