using Haulory.Application.Interfaces.Services;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Moblie.Views;
using System.Windows.Input;

namespace Haulory.Moblie.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    #region Fields

    private readonly ISessionService _sessionService;
    private readonly IJobRepository _jobRepository;
    private readonly IDeliveryReceiptRepository _deliveryReceiptRepository;
    
    private int _completedTodayCount;
    private decimal _revenueToday;

    private string _currentJobSummary = "No active jobs yet";
    private string _latestCompletedSummary = "No completed jobs yet";
    private string _referenceNumber = "";
    private string _pickupCompany = "";
    private string _deliveryCompany = "";
    private string _deliveryAddress = "";
    private string _loadDescription = "";
    #endregion

    #region Properties

    public string CurrentJobSummary
    {
        get => _currentJobSummary;
        private set
        {
            _currentJobSummary = value;
            OnPropertyChanged();
        }
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
        private set
        {
            _completedTodayCount = value;
            OnPropertyChanged();
        }
    }

    public decimal RevenueToday
    {
        get => _revenueToday;
        private set
        {
            _revenueToday = value;
            OnPropertyChanged();
        }
    }

    public string LatestCompletedSummary
    {
        get => _latestCompletedSummary;
        private set
        {
            _latestCompletedSummary = value;
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
            await Shell.Current.GoToAsync(nameof(DriversPage)));

        GoToReportsCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(ReportsPage)));

        LogoutCommand = new Command(async () => await LogoutAsync());
    }

    #endregion

    #region Public Methods

    public async Task LoadAsync()
    {
        await LoadCurrentJobAsync();
        await LoadCompletedReportSummaryAsync();
    }

    public async Task LoadCurrentJobAsync()
    {
        var jobs = await _jobRepository.GetAllAsync();

        var nextJob = jobs
            .OrderBy(j => j.SortOrder)
            .FirstOrDefault();

        if (nextJob == null)
        {

            CurrentJobSummary = "No active jobs yet";
            OnPropertyChanged(nameof(CurrentJobSummary));
            return;
        }

        CurrentJobSummary =
            $"{nextJob.ReferenceNumber}\n" +
            $"{nextJob.PickupCompany} → {nextJob.DeliveryCompany}\n" +
            $"{nextJob.DeliveryAddress}\n" +
            $"{nextJob.LoadDescription}";

        OnPropertyChanged(nameof(CurrentJobSummary));
    }

    public async Task LoadCompletedReportSummaryAsync()
    {
        var receipts = await _deliveryReceiptRepository.GetAllAsync();

        // Use LOCAL "today" so NZ users don’t miss late-night/early-morning deliveries
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

    #region Private Methods

    private static DateTime ToLocalDate(DateTime deliveredAtUtc)
    {
        // Ensure treated as UTC, then convert to local date for “today” comparisons
        var utc = DateTime.SpecifyKind(deliveredAtUtc, DateTimeKind.Utc);
        return utc.ToLocalTime().Date;
    }

    private async Task LogoutAsync()
    {
        await _sessionService.ClearAsync();
        await Shell.Current.GoToAsync("//LoginPage");
    }

    #endregion
}
