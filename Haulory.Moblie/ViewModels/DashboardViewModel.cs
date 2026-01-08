using Haulory.Application.Interfaces.Services;
using Haulory.Moblie.Views;
using System.Windows.Input;

namespace Haulory.Moblie.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private readonly ISessionService _sessionService;

    public ICommand GoToJobsCommand { get; }
    public ICommand GoToVehiclesCommand { get; }
    public ICommand GoToDriversCommand { get; }
    public ICommand GoToReportsCommand { get; }
    public ICommand LogoutCommand { get; }

    public string TodaySummary => "No active jobs yet";

    public DashboardViewModel(ISessionService sessionService)
    {
        _sessionService = sessionService;

        try
        {
            GoToJobsCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(JobsCollectionPage)));
        }
        catch(Exception ex)
        {
            throw new InvalidOperationException(
                "Failed to navi", ex);
        }

        GoToVehiclesCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(VehiclesPage)));

        GoToDriversCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(DriversPage)));

        GoToReportsCommand = new Command(async () =>
            await Shell.Current.GoToAsync(nameof(ReportsPage)));

        LogoutCommand = new Command(async () => await LogoutAsync());
    }

    private async Task LogoutAsync()
    {
        await _sessionService.ClearAsync();
        await Shell.Current.GoToAsync($"//LoginPage");
    }
}
