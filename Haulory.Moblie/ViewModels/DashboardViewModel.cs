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

    #endregion

    #region Properties

    public string CurrentJobSummary { get; private set; } = "No active jobs yet";

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
        IJobRepository jobRepository)
    {
        _sessionService = sessionService;
        _jobRepository = jobRepository;

        GoToJobsCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(JobsCollectionPage)));

        GoToVehiclesCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(VehiclesPage)));

        GoToDriversCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(DriversPage)));

        GoToReportsCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(ReportsPage)));

        LogoutCommand = new Command(async () => await LogoutAsync());

        _ = LoadCurrentJobAsync();
    }

    #endregion

    #region Public Methods

    public async Task LoadCurrentJobAsync()
    {
        var jobs = await _jobRepository.GetAllAsync();
        var nextJob = jobs
            .OrderBy(j => j.SortOrder)
            .FirstOrDefault();

        CurrentJobSummary = nextJob == null
            ? "No active jobs yet"
            : $"{nextJob.PickupCompany} → {nextJob.DeliveryCompany}";

        OnPropertyChanged(nameof(CurrentJobSummary));
    }

    #endregion

    #region Private Methods
    private async Task LogoutAsync()
    {
        await _sessionService.ClearAsync();
        await Shell.Current.GoToAsync("//LoginPage");
    }

    #endregion
}
