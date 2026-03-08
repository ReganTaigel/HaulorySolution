using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Application.Services;
using Haulory.Domain.Enums;
using Haulory.Mobile.Views;
using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Haulory.Mobile.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    #region Dependencies

    private readonly ISessionService _sessionService;
    private readonly IJobRepository _jobRepository;
    private readonly IDeliveryReceiptRepository _deliveryReceiptRepository;
    private readonly IOdometerService _odometerService;
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
        private set { _currentJobSummary = value; OnPropertyChanged(); }
    }

    public string ReferenceNumber
    {
        get => _referenceNumber;
        private set
        {
            _referenceNumber = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasActiveJob));
        }
    }

    public string PickupCompany
    {
        get => _pickupCompany;
        private set { _pickupCompany = value; OnPropertyChanged(); }
    }

    public string DeliveryCompany
    {
        get => _deliveryCompany;
        private set { _deliveryCompany = value; OnPropertyChanged(); }
    }

    public string DeliveryAddress
    {
        get => _deliveryAddress;
        private set { _deliveryAddress = value; OnPropertyChanged(); }
    }

    public string LoadDescription
    {
        get => _loadDescription;
        private set { _loadDescription = value; OnPropertyChanged(); }
    }

    public bool HasActiveJob => !string.IsNullOrWhiteSpace(ReferenceNumber);

    public int CompletedTodayCount
    {
        get => _completedTodayCount;
        private set { _completedTodayCount = value; OnPropertyChanged(); }
    }

    public decimal RevenueToday
    {
        get => _revenueToday;
        private set { _revenueToday = value; OnPropertyChanged(); }
    }

    public string LatestCompletedSummary
    {
        get => _latestCompletedSummary;
        private set { _latestCompletedSummary = value; OnPropertyChanged(); }
    }
    public bool HasStartedDay
    {
        get => _hasStartedDay;
        private set
        {
            _hasStartedDay = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanStartDay));
            OnPropertyChanged(nameof(CanEndDay));
        }
    }

    public bool CanStartDay => !HasStartedDay;
    public bool CanEndDay => HasStartedDay;

    public string DayStatusText
    {
        get => _dayStatusText;
        private set
        {
            _dayStatusText = value;
            OnPropertyChanged();
        }
    }

    public string AssignedVehicleDisplay
    {
        get => _assignedVehicleDisplay;
        private set
        {
            _assignedVehicleDisplay = value;
            OnPropertyChanged();
        }
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
        IJobRepository jobRepository,
        IDeliveryReceiptRepository deliveryReceiptRepository,
        IOdometerService odometerService)
    {
        _sessionService = sessionService;
        _jobRepository = jobRepository;
        _deliveryReceiptRepository = deliveryReceiptRepository;
        _odometerService = odometerService;

        StartDayCommand = new Command(async () => await StartDayAsync());
        EndDayCommand = new Command(async () => await EndDayAsync());

        GoToJobsCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(JobsCollectionPage)));

        GoToVehiclesCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(VehicleCollectionPage)));

        GoToDriversCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(DriverCollectionPage)));

        GoToReportsCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(ReportsPage)));

        LogoutCommand = new Command(async () => await LogoutAsync());

        EnsureShellNavigationRefreshHook();
    }

    #endregion

    #region Public Methods

    public async Task LoadAsync()
    {
        if (_isLoading) return;
        _isLoading = true;

        try
        {
            await LoadCurrentJobAsync();
            await LoadCompletedReportSummaryAsync();
            await LoadDayStateAsync();

            OnPropertyChanged(nameof(IsMainUser));
            OnPropertyChanged(nameof(IsSubUser));
        }
        finally
        {
            _isLoading = false;
        }
    }

    public async Task LoadCurrentJobAsync()
    {
        var ownerUserId = _sessionService.CurrentOwnerId ?? Guid.Empty;
        var accountId = _sessionService.CurrentAccountId ?? Guid.Empty;

        if (ownerUserId == Guid.Empty || accountId == Guid.Empty)
        {
            ClearCurrentJobUi();
            CurrentJobSummary = "Please log in again";
            return;
        }

        var jobs = await _jobRepository.GetActiveByOwnerAsync(ownerUserId);

        // IMPORTANT:
        // Main user sees only:
        // - jobs assigned to main account
        // - or unassigned jobs
        //
        // Sub user sees only:
        // - jobs assigned to that sub account
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
        var ownerUserId = _sessionService.CurrentOwnerId ?? Guid.Empty;
        if (ownerUserId == Guid.Empty)
        {
            CompletedTodayCount = 0;
            RevenueToday = 0m;
            LatestCompletedSummary = "Please log in again";
            return;
        }

        var receipts = await _deliveryReceiptRepository.GetByOwnerAsync(ownerUserId);

        var todayLocal = DateTime.Now.Date;

        var todayReceipts = receipts
            .Where(r => ToLocalDate(r.DeliveredAtUtc) == todayLocal)
            .OrderByDescending(r => r.DeliveredAtUtc)
            .ToList();

        CompletedTodayCount = todayReceipts.Count;
        RevenueToday = todayReceipts.Sum(r => r.Total);

        var latest = todayReceipts.FirstOrDefault();
        LatestCompletedSummary = latest == null
            ? "No completed jobs yet"
            : $"{latest.ReferenceNumber} • {latest.ReceiverName} • {latest.Total:C}";
    }

    private async Task LoadDayStateAsync()
    {
        _currentVehicleAssetId = null;
        AssignedVehicleDisplay = "No vehicle assigned";

        var ownerUserId = _sessionService.CurrentOwnerId ?? Guid.Empty;
        var accountId = _sessionService.CurrentAccountId ?? Guid.Empty;

        if (ownerUserId == Guid.Empty || accountId == Guid.Empty)
        {
            HasStartedDay = false;
            DayStatusText = "Please log in again";
            return;
        }

        var jobs = await _jobRepository.GetActiveByOwnerAsync(ownerUserId);

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
        AssignedVehicleDisplay = $"Assigned vehicle ready";

        // Temporary dashboard-only state:
        // later this should come from a persisted DriverDay table
        HasStartedDay = Preferences.Default.Get($"day_started_{accountId}", false);

        DayStatusText = HasStartedDay
            ? "Day in progress"
            : "Day not started";
    }

    private async Task StartDayAsync()
    {
        try
        {
            if (_currentVehicleAssetId == null || _currentVehicleAssetId == Guid.Empty)
            {
                await Shell.Current.DisplayAlertAsync("No vehicle", "There is no assigned vehicle for today.", "OK");
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
                await Shell.Current.DisplayAlertAsync("Invalid value", "Enter a valid odometer reading.", "OK");
                return;
            }

            var currentUserId = _sessionService.CurrentAccountId;
            var driverId = Guid.Empty as Guid?;

            await _odometerService.RecordReadingAsync(
                _currentVehicleAssetId.Value,
                startKm,
                OdometerReadingType.StartOfDay,
                driverId,
                currentUserId,
                "Dashboard start day entry",
                updateCurrentOdometer: true);

            Preferences.Default.Set($"day_started_{currentUserId}", true);

            HasStartedDay = true;
            DayStatusText = $"Day started • {startKm:N0} km";

            await Shell.Current.DisplayAlertAsync("Started", "Day started successfully.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }
    private async Task EndDayAsync()
    {
        try
        {
            if (_currentVehicleAssetId == null || _currentVehicleAssetId == Guid.Empty)
            {
                await Shell.Current.DisplayAlertAsync("No vehicle", "There is no assigned vehicle for today.", "OK");
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
                await Shell.Current.DisplayAlertAsync("Invalid value", "Enter a valid odometer reading.", "OK");
                return;
            }

            var currentUserId = _sessionService.CurrentAccountId;
            var driverId = Guid.Empty as Guid?;

            await _odometerService.RecordReadingAsync(
                _currentVehicleAssetId.Value,
                endKm,
                OdometerReadingType.EndOfDay,
                driverId,
                currentUserId,
                "Dashboard end day entry",
                updateCurrentOdometer: true);

            Preferences.Default.Set($"day_started_{currentUserId}", false);

            HasStartedDay = false;
            DayStatusText = $"Day completed • {endKm:N0} km";

            await Shell.Current.DisplayAlertAsync("Completed", "Day ended successfully.", "OK");
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

    private void EnsureShellNavigationRefreshHook()
    {
        if (_isSubscribedToShell) return;
        if (Shell.Current == null) return;

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

    private static DateTime ToLocalDate(DateTime deliveredAtUtc)
    {
        var utc = deliveredAtUtc.Kind == DateTimeKind.Utc
            ? deliveredAtUtc
            : DateTime.SpecifyKind(deliveredAtUtc, DateTimeKind.Utc);

        return utc.ToLocalTime().Date;
    }

    private async Task LogoutAsync()
    {
        await _sessionService.ClearAsync();
        await Shell.Current.GoToAsync("//LoginPage");
    }

    #endregion
}