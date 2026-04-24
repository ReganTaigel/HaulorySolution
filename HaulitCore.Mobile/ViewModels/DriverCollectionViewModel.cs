using HaulitCore.Application.Interfaces.Services;
using HaulitCore.Contracts.Drivers;
using HaulitCore.Mobile.Diagnostics;
using HaulitCore.Mobile.Services;
using HaulitCore.Mobile.Views;
using HaulitCore.Mobile.Features;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HaulitCore.Mobile.ViewModels;

public class DriverCollectionViewModel : BaseViewModel
{
    #region Dependencies

    private readonly DriversApiService _driversApiService;
    private readonly ISessionService _sessionService;
    private readonly ICrashLogger _crashLogger;

    #endregion

    #region State

    private DriverDto? _mainDriver;
    private bool _isMainComplete;

    #endregion

    #region Feature Access

    public bool IsMainUser =>
        _sessionService.CurrentAccountId.HasValue &&
        _sessionService.CurrentOwnerId.HasValue &&
        _sessionService.CurrentAccountId.Value == _sessionService.CurrentOwnerId.Value;

    public bool IsDriversVisible => IsFeatureVisible(AppFeature.Drivers);
    public bool IsDriversEnabled => IsFeatureEnabled(AppFeature.Drivers);

    public bool IsAddDriverVisible =>
        _isMainComplete &&
        IsMainUser &&
        IsFeatureVisible(AppFeature.AddDriver);

    public bool IsAddDriverEnabled =>
        _isMainComplete &&
        IsMainUser &&
        IsFeatureEnabled(AppFeature.AddDriver);

    public bool ShowInductionWarnings =>
        IsFeatureVisible(AppFeature.Inductions) &&
        IsFeatureEnabled(AppFeature.Inductions);

    public bool IsInductionsVisible => IsFeatureVisible(AppFeature.Inductions);
    public bool IsInductionsEnabled => IsFeatureEnabled(AppFeature.Inductions);

    #endregion

    #region Collections

    public ObservableCollection<DriverListItem> Drivers { get; } = new();

    #endregion

    #region Commands

    public ICommand AddDriverCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ManageInductionsCommand { get; }
    public ICommand ToggleDriverExpandedCommand { get; }
    public ICommand EditDriverCommand { get; }

    #endregion

    #region Constructor

    public DriverCollectionViewModel(
        DriversApiService driversApiService,
        ISessionService sessionService,
        IFeatureAccessService featureAccessService,
        ICrashLogger crashLogger)
        : base(featureAccessService)
    {
        _driversApiService = driversApiService;
        _sessionService = sessionService;
        _crashLogger = crashLogger;

        AddDriverCommand = new Command(async () =>
        {
            await SafeRunner.RunAsync(
                async () => await NavigateToFeatureAsync(AppFeature.AddDriver, nameof(NewDriverPage)),
                _crashLogger,
                "DriverCollectionViewModel.AddDriverCommand",
                nameof(DriverCollectionPage));
        });

        ManageInductionsCommand = new Command(async () =>
        {
            await SafeRunner.RunAsync(
                async () => await NavigateToFeatureAsync(AppFeature.Inductions, nameof(ManageInductionsPage)),
                _crashLogger,
                "DriverCollectionViewModel.ManageInductionsCommand",
                nameof(DriverCollectionPage));
        });

        RefreshCommand = new Command(async () => await LoadAsync());

        ToggleDriverExpandedCommand = new Command<DriverListItem>(item =>
        {
            if (item == null)
                return;

            foreach (var driver in Drivers)
            {
                if (!ReferenceEquals(driver, item))
                    driver.IsExpanded = false;
            }

            item.IsExpanded = !item.IsExpanded;
        });
        EditDriverCommand = new Command<DriverListItem>(async item =>
        {
            if (item == null)
                return;

            await SafeRunner.RunAsync(
                async () =>
                {
                    if (!await EnsureFeatureEnabledAsync(AppFeature.AddDriver))
                        return;

                    await Shell.Current.GoToAsync($"{nameof(NewDriverPage)}?driverId={item.Id}");
                },
                _crashLogger,
                "DriverCollectionViewModel.EditDriverCommand",
                nameof(DriverCollectionPage));
        });
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

            await SafeRunner.RunAsync(
                async () =>
                {
                    Drivers.Clear();

                    if (!IsFeatureEnabled(AppFeature.Drivers))
                    {
                        RaiseGate();
                        return;
                    }

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

                    _mainDriver = drivers.FirstOrDefault(d =>
                        d.UserId.HasValue && d.UserId.Value == ownerUserId);

                    var orderedDrivers = drivers
                        .OrderByDescending(d =>
                            d.UserId.HasValue && d.UserId.Value == ownerUserId)
                        .ThenBy(d => d.LastName ?? string.Empty)
                        .ThenBy(d => d.FirstName ?? string.Empty)
                        .ThenBy(d => d.Email ?? string.Empty)
                        .ToList();

                    foreach (var driver in orderedDrivers)
                    {
                        var item = new DriverListItem(driver);

                        if (_mainDriver != null && driver.Id == _mainDriver.Id)
                            item.IsExpanded = true;

                        Drivers.Add(item);
                    }

                    _isMainComplete =
                        _mainDriver != null &&
                        _mainDriver.EmergencyContact != null &&
                        !string.IsNullOrWhiteSpace(_mainDriver.EmergencyContact.FirstName) &&
                        !string.IsNullOrWhiteSpace(_mainDriver.EmergencyContact.LastName) &&
                        !string.IsNullOrWhiteSpace(_mainDriver.EmergencyContact.Relationship) &&
                        !string.IsNullOrWhiteSpace(_mainDriver.EmergencyContact.PhoneNumber) &&
                        !string.IsNullOrWhiteSpace(_mainDriver.EmergencyContact.Email);

                    RaiseGate();
                },
                _crashLogger,
                "DriverCollectionViewModel.LoadAsync",
                nameof(DriverCollectionPage),
                onError: async ex =>
                {
                    await Shell.Current.DisplayAlertAsync("Load failed", ex.Message, "OK");
                });
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region Private Methods

    private void RaiseGate()
    {
        OnPropertyChanged(nameof(IsMainUser));
        OnPropertyChanged(nameof(IsDriversVisible));
        OnPropertyChanged(nameof(IsDriversEnabled));
        OnPropertyChanged(nameof(IsAddDriverVisible));
        OnPropertyChanged(nameof(IsAddDriverEnabled));
        OnPropertyChanged(nameof(ShowInductionWarnings));
        OnPropertyChanged(nameof(IsInductionsVisible));
        OnPropertyChanged(nameof(IsInductionsEnabled));
    }

    private async Task NavigateToFeatureAsync(AppFeature feature, string route)
    {
        if (!await EnsureFeatureEnabledAsync(feature))
            return;

        await Shell.Current.GoToAsync(route);
    }

    #endregion
}