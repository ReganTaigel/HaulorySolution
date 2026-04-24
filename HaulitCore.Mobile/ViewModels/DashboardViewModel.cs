using HaulitCore.Application.Interfaces.Services;
using HaulitCore.Contracts.Vehicles;
using HaulitCore.Domain.Enums;
using HaulitCore.Mobile.Diagnostics;
using HaulitCore.Mobile.Features;
using HaulitCore.Mobile.Services;
using HaulitCore.Mobile.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HaulitCore.Mobile.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    #region Dependencies

    private readonly ISessionService _sessionService;
    private readonly JobsApiService _jobsApiService;
    private readonly HubodometerApiService _hubodometerApiService;
    private readonly ReportsApiService _reportsApiService;
    private readonly ICrashLogger _crashLogger;

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

    private int _needsReviewCount;
    private string _needsReviewSummary = "No jobs need review";

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

    public bool IsNeedsReviewVisible => IsMainUser && IsJobsVisible;
    public bool HasNeedsReviewJobs => NeedsReviewCount > 0;
    public bool CanSeeDrivers => IsMainUser && IsDriversVisible;
    public bool CanSeeReports => IsMainUser && IsReportsVisible;
    public bool CanSeeNeedsReview => IsMainUser && IsJobsVisible;

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
                OnPropertyChanged(nameof(HasActiveJob));
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

    public int NeedsReviewCount
    {
        get => _needsReviewCount;
        private set
        {
            if (SetProperty(ref _needsReviewCount, value))
                OnPropertyChanged(nameof(HasNeedsReviewJobs));
        }
    }

    public string NeedsReviewSummary
    {
        get => _needsReviewSummary;
        private set => SetProperty(ref _needsReviewSummary, value);
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
    public ICommand GoToNeedsReviewCommand { get; }
    public ICommand LogoutCommand { get; }
    public ICommand StartDayCommand { get; }
    public ICommand EndDayCommand { get; }

    #endregion

    #region Constructor

    public DashboardViewModel(
        ISessionService sessionService,
        JobsApiService jobsApiService,
        ReportsApiService reportsApiService,
        HubodometerApiService hubodometerApiService,
        IFeatureAccessService featureAccessService,
        ICrashLogger crashLogger)
        : base(featureAccessService)
    {
        _sessionService = sessionService;
        _jobsApiService = jobsApiService;
        _reportsApiService = reportsApiService;
        _hubodometerApiService = hubodometerApiService;
        _crashLogger = crashLogger;

        StartDayCommand = new Command(async () => await StartDayAsync());
        EndDayCommand = new Command(async () => await EndDayAsync());

        GoToJobsCommand = new Command(async () =>
        {
            await SafeRunner.RunAsync(
                async () => await NavigateToFeatureAsync(AppFeature.Jobs, $"//{nameof(JobsCollectionPage)}"),
                _crashLogger,
                "DashboardViewModel.GoToJobsCommand",
                nameof(DashboardPage));
        });

        GoToVehiclesCommand = new Command(async () =>
        {
            await SafeRunner.RunAsync(
                async () => await NavigateToFeatureAsync(AppFeature.Vehicles, $"//{nameof(VehicleCollectionPage)}"),
                _crashLogger,
                "DashboardViewModel.GoToVehiclesCommand",
                nameof(DashboardPage));
        });

        GoToDriversCommand = new Command(async () =>
        {
            if (!IsMainUser)
                return;

            await SafeRunner.RunAsync(
                async () => await NavigateToFeatureAsync(AppFeature.Drivers, $"//{nameof(DriverCollectionPage)}"),
                _crashLogger,
                "DashboardViewModel.GoToDriversCommand",
                nameof(DashboardPage));
        });

        GoToReportsCommand = new Command(async () =>
        {
            if (!IsMainUser)
                return;

            await SafeRunner.RunAsync(
                async () => await NavigateToFeatureAsync(AppFeature.Reports, $"//{nameof(ReportsPage)}"),
                _crashLogger,
                "DashboardViewModel.GoToReportsCommand",
                nameof(DashboardPage));
        });

        GoToNeedsReviewCommand = new Command(async () =>
        {
            if (!IsMainUser)
                return;

            await SafeRunner.RunAsync(
                async () => await NavigateToFeatureAsync(AppFeature.Reports, $"//{nameof(NeedsReviewPage)}"),
                _crashLogger,
                "DashboardViewModel.GoToNeedsReviewCommand",
                nameof(DashboardPage));
        });

        LogoutCommand = new Command(async () =>
        {
            await SafeRunner.RunAsync(
                async () => await LogoutAsync(),
                _crashLogger,
                "DashboardViewModel.LogoutCommand",
                nameof(DashboardPage));
        });

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

            await SafeRunner.RunAsync(
                async () => await LoadCurrentJobAsync(),
                _crashLogger,
                "DashboardViewModel.LoadCurrentJobAsync",
                nameof(DashboardPage),
                metadataJson: CrashMetadataBuilder.Build(
                    route: nameof(DashboardPage),
                    feature: "LoadCurrentJob"),
                onError: async ex =>
                {
                    CurrentJobSummary = "Unable to load current jobs";
                    ClearCurrentJobUi();
                    await Task.CompletedTask;
                });

            await SafeRunner.RunAsync(
                async () => await LoadCompletedReportSummaryAsync(),
                _crashLogger,
                "DashboardViewModel.LoadCompletedReportSummaryAsync",
                nameof(DashboardPage),
                metadataJson: CrashMetadataBuilder.Build(
                    route: nameof(DashboardPage),
                    feature: "LoadCompletedReportSummary"),
                onError: async ex =>
                {
                    CompletedTodayCount = 0;
                    RevenueToday = 0;
                    LatestCompletedSummary = "Unable to load completed summary";
                    await Task.CompletedTask;
                });

            await SafeRunner.RunAsync(
                async () => await LoadNeedsReviewSummaryAsync(),
                _crashLogger,
                "DashboardViewModel.LoadNeedsReviewSummaryAsync",
                nameof(DashboardPage),
                metadataJson: CrashMetadataBuilder.Build(
                    route: nameof(DashboardPage),
                    feature: "LoadNeedsReviewSummary"),
                onError: async ex =>
                {
                    NeedsReviewCount = 0;
                    NeedsReviewSummary = "Unable to load jobs needing review";
                    await Task.CompletedTask;
                });

            await SafeRunner.RunAsync(
                async () => await LoadDayStateAsync(),
                _crashLogger,
                "DashboardViewModel.LoadDayStateAsync",
                nameof(DashboardPage),
                metadataJson: CrashMetadataBuilder.Build(
                    route: nameof(DashboardPage),
                    feature: "LoadDayState"),
                onError: async ex =>
                {
                    HasStartedDay = false;
                    DayStatusText = "Unable to load day state";
                    AssignedVehicleDisplay = "Unable to load assigned vehicle";
                    await Task.CompletedTask;
                });

            RefreshFeatureBindings();
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
    }

    public async Task LoadCompletedReportSummaryAsync()
    {
        if (!IsMainUser)
        {
            CompletedTodayCount = 0;
            RevenueToday = 0;
            LatestCompletedSummary = "No completed jobs yet";
            return;
        }

        var summary = await _reportsApiService.GetTodayAsync();

        CompletedTodayCount = summary.CompletedTodayCount;
        RevenueToday = summary.RevenueToday;

        LatestCompletedSummary =
            string.IsNullOrWhiteSpace(summary.LatestReferenceNumber)
                ? "No completed jobs yet"
                : $"{summary.LatestReferenceNumber} • {summary.LatestReceiver} • {summary.LatestTotal:C}";
    }

    public async Task LoadNeedsReviewSummaryAsync()
    {
        if (!IsMainUser)
        {
            NeedsReviewCount = 0;
            NeedsReviewSummary = "No jobs need review";
            return;
        }

        var jobs = await _jobsApiService.GetNeedsReviewJobsAsync();

        NeedsReviewCount = jobs.Count;

        var latest = jobs
            .OrderByDescending(j => j.DeliveredAtUtc)
            .FirstOrDefault();

        NeedsReviewSummary = latest == null
            ? "No jobs need review"
            : $"{latest.ReferenceNumber} • {latest.DeliveryCompany}";
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
            "Enter start hubodometer reading",
            accept: "Start Day",
            cancel: "Cancel",
            keyboard: Keyboard.Numeric);

        if (string.IsNullOrWhiteSpace(result))
            return;

        if (!int.TryParse(result, out var startKm))
        {
            await Shell.Current.DisplayAlertAsync(
                "Invalid value",
                "Enter a valid hubodometer reading.",
                "OK");
            return;
        }

        await SafeRunner.RunAsync(
            async () =>
            {
                var currentUserId = _sessionService.CurrentAccountId;
                Guid? driverId = null;

                await _hubodometerApiService.RecordReadingAsync(new HubodometerReadingRequest
                {
                    VehicleAssetId = _currentVehicleAssetId.Value,
                    ReadingKm = startKm,
                    ReadingType = HubodometerReadingType.StartOfDay,
                    DriverId = driverId,
                    RecordedByUserId = currentUserId,
                    Notes = "Dashboard start day entry",
                    UpdateCurrentHubodometer = true
                });

                Preferences.Default.Set($"day_started_{currentUserId}", true);

                HasStartedDay = true;
                DayStatusText = $"Day started • {startKm:N0} km";

                await Shell.Current.DisplayAlertAsync(
                    "Started",
                    "Day started successfully.",
                    "OK");
            },
            _crashLogger,
            "DashboardViewModel.StartDayAsync",
            nameof(DashboardPage),
            metadataJson: $"{{\"VehicleAssetId\":\"{_currentVehicleAssetId}\",\"StartKm\":{startKm}}}",
            onError: async ex =>
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            });
    }

    private async Task EndDayAsync()
    {
        if (!await EnsureFeatureEnabledAsync(AppFeature.EndDay))
            return;

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
            "Enter end hubodometer reading",
            accept: "End Day",
            cancel: "Cancel",
            keyboard: Keyboard.Numeric);

        if (string.IsNullOrWhiteSpace(result))
            return;

        if (!int.TryParse(result, out var endKm))
        {
            await Shell.Current.DisplayAlertAsync(
                "Invalid value",
                "Enter a valid hubodometer reading.",
                "OK");
            return;
        }

        await SafeRunner.RunAsync(
            async () =>
            {
                var currentUserId = _sessionService.CurrentAccountId;
                Guid? driverId = null;

                await _hubodometerApiService.RecordReadingAsync(new HubodometerReadingRequest
                {
                    VehicleAssetId = _currentVehicleAssetId.Value,
                    ReadingKm = endKm,
                    ReadingType = HubodometerReadingType.EndOfDay,
                    DriverId = driverId,
                    RecordedByUserId = currentUserId,
                    Notes = "Dashboard end day entry",
                    UpdateCurrentHubodometer = true
                });

                Preferences.Default.Set($"day_started_{currentUserId}", false);

                HasStartedDay = false;
                DayStatusText = $"Day completed • {endKm:N0} km";

                await Shell.Current.DisplayAlertAsync(
                    "Completed",
                    "Day ended successfully.",
                    "OK");
            },
            _crashLogger,
            "DashboardViewModel.EndDayAsync",
            nameof(DashboardPage),
            metadataJson: $"{{\"VehicleAssetId\":\"{_currentVehicleAssetId}\",\"EndKm\":{endKm}}}",
            onError: async ex =>
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            });
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
        OnPropertyChanged(nameof(IsMainUser));
        OnPropertyChanged(nameof(IsSubUser));

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

        OnPropertyChanged(nameof(IsNeedsReviewVisible));
        OnPropertyChanged(nameof(HasNeedsReviewJobs));
        OnPropertyChanged(nameof(CanSeeDrivers));
        OnPropertyChanged(nameof(CanSeeReports));
        OnPropertyChanged(nameof(CanSeeNeedsReview));
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

        if (!target.Contains(nameof(DashboardPage), StringComparison.OrdinalIgnoreCase))
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