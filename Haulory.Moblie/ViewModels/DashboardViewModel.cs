using Haulory.Application.Interfaces.Services;

using Haulory.Domain.Enums;
using Haulory.Mobile.Contracts.Vehicles;
using Haulory.Mobile.Features;
using Haulory.Mobile.Services;
using Haulory.Mobile.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Haulory.Mobile.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    #region Dependencies

    private readonly ISessionService _sessionService;
    private readonly JobsApiService _jobsApiService;
    private readonly OdometerApiService _odometerApiService;
    private readonly ReportsApiService _reportsApiService;

    #endregion

    #region State

    private int _completedTodayCount;
    private decimal _revenueToday;

    private string _currentJobSummary = "No active jobs yet";
    private string _latestCompletedSummary = "No completed jobs yet";

    private string _referenceNumber = string.Empty;
    private string _pickupCompany = string.Empty;
    private string _deliveryCompany = string.Empty;
    private string _deliveryAddress = string.Empty;
    private string _loadDescription = string.Empty;

    private bool _isLoading;
    private bool _isSubscribedToShell;

    private bool _hasStartedDay;
    private string _dayStatusText = "Day not started";
    private string _assignedVehicleDisplay = "No vehicle assigned";
    private Guid? _currentVehicleAssetId;

    #endregion

    #region Feature Access Properties

    public bool IsDriversVisible => IsFeatureVisible(AppFeature.Drivers);
    public bool IsJobsVisible => IsFeatureVisible(AppFeature.Jobs);
    public bool IsVehiclesVisible => IsFeatureVisible(AppFeature.Vehicles);
    public bool IsReportsVisible => IsFeatureVisible(AppFeature.Reports);
    public bool IsInductionsVisible => IsFeatureVisible(AppFeature.Inductions);

    public bool IsDriversEnabled => IsFeatureEnabled(AppFeature.Drivers);
    public bool IsJobsEnabled => IsFeatureEnabled(AppFeature.Jobs);
    public bool IsVehiclesEnabled => IsFeatureEnabled(AppFeature.Vehicles);
    public bool IsReportsEnabled => IsFeatureEnabled(AppFeature.Reports);
    public bool IsInductionsEnabled => IsFeatureEnabled(AppFeature.Inductions);

    public bool IsStartDayVisible => IsFeatureVisible(AppFeature.StartDay);
    public bool IsStartDayEnabled => IsFeatureEnabled(AppFeature.StartDay);

    public bool IsEndDayVisible => IsFeatureVisible(AppFeature.EndDay);
    public bool IsEndDayEnabled => IsFeatureEnabled(AppFeature.EndDay);

    public bool CanStartDayAction => IsFeatureEnabled(AppFeature.StartDay) && CanStartDay;
    public bool CanEndDayAction => IsFeatureEnabled(AppFeature.EndDay) && CanEndDay;

    #endregion

    #region Bindable Properties

    public bool IsSubUser =>
        _sessionService.CurrentAccountId.HasValue &&
        _sessionService.CurrentOwnerId.HasValue &&
        _sessionService.CurrentOwnerId.Value != _sessionService.CurrentAccountId.Value;

    public bool IsMainUser =>
        _sessionService.CurrentAccountId.HasValue &&
        _sessionService.CurrentOwnerId.HasValue &&
        _sessionService.CurrentOwnerId.Value == _sessionService.CurrentAccountId.Value;

    public string CurrentJobSummary
    {
        get => _currentJobSummary;
        private set => SetProperty(ref _currentJobSummary, value);
    }

    public string ReferenceNumber
    {
        get => _referenceNumber;
        private set
        {
            if (SetProperty(ref _referenceNumber, value))
            {
                OnPropertyChanged(nameof(HasActiveJob));
            }
        }
    }

    public string PickupCompany
    {
        get => _pickupCompany;
        private set => SetProperty(ref _pickupCompany, value);
    }

    public string DeliveryCompany
    {
        get => _deliveryCompany;
        private set => SetProperty(ref _deliveryCompany, value);
    }

    public string DeliveryAddress
    {
        get => _deliveryAddress;
        private set => SetProperty(ref _deliveryAddress, value);
    }

    public string LoadDescription
    {
        get => _loadDescription;
        private set => SetProperty(ref _loadDescription, value);
    }

    public bool HasActiveJob => !string.IsNullOrWhiteSpace(ReferenceNumber);

    public int CompletedTodayCount
    {
        get => _completedTodayCount;
        private set => SetProperty(ref _completedTodayCount, value);
    }

    public decimal RevenueToday
    {
        get => _revenueToday;
        private set => SetProperty(ref _revenueToday, value);
    }

    public string LatestCompletedSummary
    {
        get => _latestCompletedSummary;
        private set => SetProperty(ref _latestCompletedSummary, value);
    }

    public bool HasStartedDay
    {
        get => _hasStartedDay;
        private set
        {
            if (SetProperty(ref _hasStartedDay, value))
            {
                OnPropertyChanged(nameof(CanStartDay));
                OnPropertyChanged(nameof(CanEndDay));
                OnPropertyChanged(nameof(CanStartDayAction));
                OnPropertyChanged(nameof(CanEndDayAction));
            }
        }
    }

    public bool CanStartDay => !HasStartedDay;
    public bool CanEndDay => HasStartedDay;

    public string DayStatusText
    {
        get => _dayStatusText;
        private set => SetProperty(ref _dayStatusText, value);
    }

    public string AssignedVehicleDisplay
    {
        get => _assignedVehicleDisplay;
        private set => SetProperty(ref _assignedVehicleDisplay, value);
    }

    #endregion

    #region Commands

    public ICommand GoToJobsCommand { get; }
    public ICommand GoToVehiclesCommand { get; }
    public ICommand GoToDriversCommand { get; }
    public ICommand GoToReportsCommand { get; }
    public ICommand LogoutCommand { get; }
    public ICommand StartDayCommand { get; }
    public ICommand EndDayCommand { get; }

    #endregion

    #region Constructor

    public DashboardViewModel(
        ISessionService sessionService,
        JobsApiService jobsApiService,
        ReportsApiService reportsApiService,
        OdometerApiService odometerApiService,
        IFeatureAccessService featureAccessService)
        : base(featureAccessService)
    {
        _sessionService = sessionService;
        _jobsApiService = jobsApiService;
        _reportsApiService = reportsApiService;
        _odometerApiService = odometerApiService;

        StartDayCommand = new Command(async () => await StartDayAsync());
        EndDayCommand = new Command(async () => await EndDayAsync());

        GoToJobsCommand = new Command(async () =>
            await NavigateToFeatureAsync(AppFeature.Jobs, nameof(JobsCollectionPage)));

        GoToVehiclesCommand = new Command(async () =>
            await NavigateToFeatureAsync(AppFeature.Vehicles, nameof(VehicleCollectionPage)));

        GoToDriversCommand = new Command(async () =>
            await NavigateToFeatureAsync(AppFeature.Drivers, nameof(DriverCollectionPage)));

        GoToReportsCommand = new Command(async () =>
            await NavigateToFeatureAsync(AppFeature.Reports, nameof(ReportsPage)));

        LogoutCommand = new Command(async () => await LogoutAsync());

        EnsureShellNavigationRefreshHook();
    }

    #endregion

    #region Public Methods

    public async Task LoadAsync()
    {
        if (_isLoading)
            return;

        _isLoading = true;

        try
        {
            if (!_sessionService.IsAuthenticated)
                await _sessionService.RestoreAsync();

            OnPropertyChanged(nameof(IsMainUser));
            OnPropertyChanged(nameof(IsSubUser));

            await LoadCurrentJobAsync();
            await LoadCompletedReportSummaryAsync();
            await LoadDayStateAsync();

            RefreshFeatureBindings();
            System.Diagnostics.Debug.WriteLine(
            $"[Dashboard After Refresh] " +
            $"JobsVisible={IsJobsVisible}, " +
            $"VehiclesVisible={IsVehiclesVisible}, " +
            $"DriversVisible={IsDriversVisible}, " +
            $"ReportsVisible={IsReportsVisible}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    public async Task LoadCurrentJobAsync()
    {
        if (!_sessionService.IsAuthenticated)
            await _sessionService.RestoreAsync();

        var ownerUserId = _sessionService.CurrentOwnerId ?? Guid.Empty;
        var accountId = _sessionService.CurrentAccountId ?? Guid.Empty;

        if (ownerUserId == Guid.Empty || accountId == Guid.Empty)
        {
            ClearCurrentJobUi();
            CurrentJobSummary = "Please log in again";
            return;
        }

        var jobs = await _jobsApiService.GetActiveJobsAsync();

        var filteredJobs = IsSubUser
            ? jobs.Where(j => j.AssignedToUserId == accountId)
            : jobs.Where(j => j.AssignedToUserId == null || j.AssignedToUserId == accountId);

        var nextJob = filteredJobs
            .OrderBy(j => j.SortOrder)
            .FirstOrDefault();

        if (nextJob == null)
        {
            CurrentJobSummary = IsSubUser
                ? "No assigned jobs right now"
                : "No main account jobs right now";

            ClearCurrentJobUi();
            return;
        }

        ReferenceNumber = nextJob.ReferenceNumber ?? string.Empty;
        PickupCompany = nextJob.PickupCompany ?? string.Empty;
        DeliveryCompany = nextJob.DeliveryCompany ?? string.Empty;
        DeliveryAddress = nextJob.DeliveryAddress ?? string.Empty;
        LoadDescription = nextJob.LoadDescription ?? string.Empty;

        CurrentJobSummary =
            $"{nextJob.ReferenceNumber}\n" +
            $"{nextJob.PickupCompany} → {nextJob.DeliveryCompany}\n" +
            $"{nextJob.DeliveryAddress}\n" +
            $"{nextJob.LoadDescription}";

        if (!_sessionService.IsAuthenticated)
            await _sessionService.RestoreAsync();

    }

    public async Task LoadCompletedReportSummaryAsync()
    {
        var summary = await _reportsApiService.GetTodayAsync();

        CompletedTodayCount = summary.CompletedTodayCount;
        RevenueToday = summary.RevenueToday;

        LatestCompletedSummary =
            string.IsNullOrWhiteSpace(summary.LatestReferenceNumber)
                ? "No completed jobs yet"
                : $"{summary.LatestReferenceNumber} • {summary.LatestReceiver} • {summary.LatestTotal:C}";
    }

    #endregion

    #region Private Day Workflow

    private async Task LoadDayStateAsync()
    {
        _currentVehicleAssetId = null;
        AssignedVehicleDisplay = "No vehicle assigned";

        if (!_sessionService.IsAuthenticated)
            await _sessionService.RestoreAsync();

        var ownerUserId = _sessionService.CurrentOwnerId ?? Guid.Empty;
        var accountId = _sessionService.CurrentAccountId ?? Guid.Empty;

        if (ownerUserId == Guid.Empty || accountId == Guid.Empty)
        {
            HasStartedDay = false;
            DayStatusText = "Please log in again";
            return;
        }

        var jobs = await _jobsApiService.GetActiveJobsAsync();

        var filteredJobs = IsSubUser
            ? jobs.Where(j => j.AssignedToUserId == accountId)
            : jobs.Where(j => j.AssignedToUserId == null || j.AssignedToUserId == accountId);

        var nextJob = filteredJobs
            .OrderBy(j => j.SortOrder)
            .FirstOrDefault();

        if (nextJob?.VehicleAssetId == null || nextJob.VehicleAssetId == Guid.Empty)
        {
            HasStartedDay = false;
            DayStatusText = "No assigned vehicle";
            return;
        }

        _currentVehicleAssetId = nextJob.VehicleAssetId;
        AssignedVehicleDisplay = "Assigned vehicle ready";

        HasStartedDay = Preferences.Default.Get($"day_started_{accountId}", false);

        DayStatusText = HasStartedDay
            ? "Day in progress"
            : "Day not started";
    }

    private async Task StartDayAsync()
    {
        if (!await EnsureFeatureEnabledAsync(AppFeature.StartDay))
            return;

        try
        {
            if (_currentVehicleAssetId == null || _currentVehicleAssetId == Guid.Empty)
            {
                await Shell.Current.DisplayAlertAsync(
                    "No vehicle",
                    "There is no assigned vehicle for today.",
                    "OK");
                return;
            }

            if (HasStartedDay)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Already started",
                    "The day has already been started.",
                    "OK");
                return;
            }

            var result = await Shell.Current.DisplayPromptAsync(
                "Start Day",
                "Enter start odometer reading",
                accept: "Start Day",
                cancel: "Cancel",
                keyboard: Keyboard.Numeric);

            if (string.IsNullOrWhiteSpace(result))
                return;

            if (!int.TryParse(result, out var startKm))
            {
                await Shell.Current.DisplayAlertAsync(
                    "Invalid value",
                    "Enter a valid odometer reading.",
                    "OK");
                return;
            }

            var currentUserId = _sessionService.CurrentAccountId;
            Guid? driverId = null;

            await _odometerApiService.RecordReadingAsync(new OdometerReadingRequest
            {
                VehicleAssetId = _currentVehicleAssetId.Value,
                ReadingKm = startKm,
                ReadingType = OdometerReadingType.StartOfDay,
                DriverId = driverId,
                RecordedByUserId = currentUserId,
                Notes = "Dashboard start day entry",
                UpdateCurrentOdometer = true
            });

            Preferences.Default.Set($"day_started_{currentUserId}", true);

            HasStartedDay = true;
            DayStatusText = $"Day started • {startKm:N0} km";

            await Shell.Current.DisplayAlertAsync(
                "Started",
                "Day started successfully.",
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }

    private async Task EndDayAsync()
    {
        if (!await EnsureFeatureEnabledAsync(AppFeature.EndDay))
            return;

        try
        {
            if (_currentVehicleAssetId == null || _currentVehicleAssetId == Guid.Empty)
            {
                await Shell.Current.DisplayAlertAsync(
                    "No vehicle",
                    "There is no assigned vehicle for today.",
                    "OK");
                return;
            }

            if (!HasStartedDay)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Cannot end day",
                    "You need to start the day before ending it.",
                    "OK");
                return;
            }

            var result = await Shell.Current.DisplayPromptAsync(
                "End Day",
                "Enter end odometer reading",
                accept: "End Day",
                cancel: "Cancel",
                keyboard: Keyboard.Numeric);

            if (string.IsNullOrWhiteSpace(result))
                return;

            if (!int.TryParse(result, out var endKm))
            {
                await Shell.Current.DisplayAlertAsync(
                    "Invalid value",
                    "Enter a valid odometer reading.",
                    "OK");
                return;
            }

            var currentUserId = _sessionService.CurrentAccountId;
            Guid? driverId = null;

            await _odometerApiService.RecordReadingAsync(new OdometerReadingRequest
            {
                VehicleAssetId = _currentVehicleAssetId.Value,
                ReadingKm = endKm,
                ReadingType = OdometerReadingType.EndOfDay,
                DriverId = driverId,
                RecordedByUserId = currentUserId,
                Notes = "Dashboard end day entry",
                UpdateCurrentOdometer = true
            });

            Preferences.Default.Set($"day_started_{currentUserId}", false);

            HasStartedDay = false;
            DayStatusText = $"Day completed • {endKm:N0} km";

            await Shell.Current.DisplayAlertAsync(
                "Completed",
                "Day ended successfully.",
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }

    #endregion

    #region Private Helpers

    private void ClearCurrentJobUi()
    {
        ReferenceNumber = string.Empty;
        PickupCompany = string.Empty;
        DeliveryCompany = string.Empty;
        DeliveryAddress = string.Empty;
        LoadDescription = string.Empty;
    }

    private void RefreshFeatureBindings()
    {
        OnPropertyChanged(nameof(IsDriversVisible));
        OnPropertyChanged(nameof(IsJobsVisible));
        OnPropertyChanged(nameof(IsVehiclesVisible));
        OnPropertyChanged(nameof(IsReportsVisible));
        OnPropertyChanged(nameof(IsInductionsVisible));

        OnPropertyChanged(nameof(IsDriversEnabled));
        OnPropertyChanged(nameof(IsJobsEnabled));
        OnPropertyChanged(nameof(IsVehiclesEnabled));
        OnPropertyChanged(nameof(IsReportsEnabled));
        OnPropertyChanged(nameof(IsInductionsEnabled));

        OnPropertyChanged(nameof(IsStartDayVisible));
        OnPropertyChanged(nameof(IsStartDayEnabled));
        OnPropertyChanged(nameof(IsEndDayVisible));
        OnPropertyChanged(nameof(IsEndDayEnabled));

        OnPropertyChanged(nameof(CanStartDayAction));
        OnPropertyChanged(nameof(CanEndDayAction));
    }

    private void EnsureShellNavigationRefreshHook()
    {
        if (_isSubscribedToShell)
            return;

        if (Shell.Current == null)
            return;

        Shell.Current.Navigated += OnShellNavigated;
        _isSubscribedToShell = true;
    }

    private async void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
    {
        var target = e.Current?.Location.OriginalString ?? string.Empty;

        if (!target.Contains("DashboardPage", StringComparison.OrdinalIgnoreCase))
            return;

        await LoadAsync();
    }

    private async Task NavigateToFeatureAsync(AppFeature feature, string route)
    {
        if (!await EnsureFeatureEnabledAsync(feature))
            return;

        await Shell.Current.GoToAsync(route);
    }

    private async Task LogoutAsync()
    {
        await _sessionService.ClearAsync();
        await Shell.Current.GoToAsync("//LoginPage");
    }

    #endregion
}