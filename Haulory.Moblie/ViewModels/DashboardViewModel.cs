using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Mobile.Views;
using Microsoft.Maui.Controls;

namespace Haulory.Mobile.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    #region Dependencies

    private readonly ISessionService _sessionService;
    private readonly IJobRepository _jobRepository;
    private readonly IDeliveryReceiptRepository _deliveryReceiptRepository;

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

    #endregion

    #region Bindable Properties

    public string CurrentJobSummary
    {
        get => _currentJobSummary;
        private set { _currentJobSummary = value; OnPropertyChanged(); }
    }

    public string ReferenceNumber
    {
        get => _referenceNumber;
        private set { _referenceNumber = value; OnPropertyChanged(); }
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

    #endregion

    #region Commands

    public ICommand GoToJobsCommand { get; }
    public ICommand GoToVehiclesCommand { get; }
    public ICommand GoToDriversCommand { get; }
    public ICommand GoToReportsCommand { get; }
    public ICommand LogoutCommand { get; }

    #endregion

    #region Constructor

    public DashboardViewModel(
        ISessionService sessionService,
        IJobRepository jobRepository,
        IDeliveryReceiptRepository deliveryReceiptRepository)
    {
        _sessionService = sessionService;
        _jobRepository = jobRepository;
        _deliveryReceiptRepository = deliveryReceiptRepository;

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
        }
        finally
        {
            _isLoading = false;
        }
    }

    public async Task LoadCurrentJobAsync()
    {
        var ownerUserId = _sessionService.CurrentAccountId ?? Guid.Empty;
        if (ownerUserId == Guid.Empty)
        {
            ClearCurrentJobUi();
            CurrentJobSummary = "Please log in again";
            return;
        }

        // ✅ NEW: owner-scoped active jobs
        var jobs = await _jobRepository.GetActiveByOwnerAsync(ownerUserId);

        var nextJob = jobs
            .OrderBy(j => j.SortOrder)
            .FirstOrDefault();

        if (nextJob == null)
        {
            CurrentJobSummary = "No active jobs yet";
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

        OnPropertyChanged(nameof(HasActiveJob));
    }

    public async Task LoadCompletedReportSummaryAsync()
    {
        var ownerUserId = _sessionService.CurrentAccountId ?? Guid.Empty;
        if (ownerUserId == Guid.Empty)
        {
            CompletedTodayCount = 0;
            RevenueToday = 0m;
            LatestCompletedSummary = "Please log in again";
            return;
        }

        // ✅ NEW: owner-scoped receipts
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

    #endregion

    #region Private Helpers

    private void ClearCurrentJobUi()
    {
        ReferenceNumber = string.Empty;
        PickupCompany = string.Empty;
        DeliveryCompany = string.Empty;
        DeliveryAddress = string.Empty;
        LoadDescription = string.Empty;
        OnPropertyChanged(nameof(HasActiveJob));
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