using Haulory.Application.Interfaces.Services;
using Haulory.Mobile.Contracts.Drivers;
using Haulory.Mobile.Services;
using Haulory.Mobile.Views;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Haulory.Mobile.ViewModels;

public class DriverCollectionViewModel : BaseViewModel
{
    #region Dependencies

    private readonly DriversApiService _driversApiService;
    private readonly ISessionService _sessionService;

    #endregion

    #region State

    private DriverDto? _mainDriver;
    private bool _isMainComplete;

    #endregion

    #region Bindable Properties

    public bool IsMainUser =>
        _sessionService.CurrentAccountId.HasValue &&
        _sessionService.CurrentOwnerId.HasValue &&
        _sessionService.CurrentAccountId.Value == _sessionService.CurrentOwnerId.Value;

    public bool ShowAddDriver => _isMainComplete && IsMainUser;

    public ObservableCollection<DriverListItem> Drivers { get; } = new();

    #endregion

    #region Commands

    public ICommand AddDriverCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ManageInductionsCommand { get; }

    #endregion

    #region Constructor

    public DriverCollectionViewModel(
        DriversApiService driversApiService,
        ISessionService sessionService)
    {
        _driversApiService = driversApiService;
        _sessionService = sessionService;

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

            if (!_sessionService.IsAuthenticated)
                await _sessionService.RestoreAsync();

            var ownerUserId = _sessionService.CurrentOwnerId ?? Guid.Empty;
            var accountId = _sessionService.CurrentAccountId ?? Guid.Empty;

            if (!_sessionService.IsAuthenticated || ownerUserId == Guid.Empty || accountId == Guid.Empty)
            {
                _mainDriver = null;
                _isMainComplete = false;
                RaiseGate();
                return;
            }

            var drivers = await _driversApiService.GetDriversAsync();

            var orderedDrivers = drivers
                .OrderByDescending(d => d.UserId.HasValue)
                .ThenBy(d => d.LastName ?? string.Empty)
                .ThenBy(d => d.FirstName ?? string.Empty)
                .ThenBy(d => d.Email ?? string.Empty)
                .ToList();

            foreach (var d in orderedDrivers)
            {
                Drivers.Add(new DriverListItem(d));
            }

            _mainDriver = drivers.FirstOrDefault(d =>
                d.UserId.HasValue && d.UserId.Value == ownerUserId);

            _isMainComplete =
                _mainDriver != null &&
                _mainDriver.EmergencyContact != null &&
                !string.IsNullOrWhiteSpace(_mainDriver.EmergencyContact.FirstName) &&
                !string.IsNullOrWhiteSpace(_mainDriver.EmergencyContact.LastName) &&
                !string.IsNullOrWhiteSpace(_mainDriver.EmergencyContact.Relationship) &&
                !string.IsNullOrWhiteSpace(_mainDriver.EmergencyContact.PhoneNumber) &&
                !string.IsNullOrWhiteSpace(_mainDriver.EmergencyContact.Email);

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
        OnPropertyChanged(nameof(IsMainUser));
    }

    #endregion
}