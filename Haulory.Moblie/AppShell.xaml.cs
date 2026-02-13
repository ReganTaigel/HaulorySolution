using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Mobile.Views;

namespace Haulory.Mobile;

public partial class AppShell : Shell
{
    private readonly ISessionService _sessionService;
    private readonly IUserRepository _userRepository;

    private bool? _hasMainUser;

    public AppShell(ISessionService sessionService, IUserRepository userRepository)
    {
        InitializeComponent();
        _sessionService = sessionService;
        _userRepository = userRepository;

        // Jobs
        Routing.RegisterRoute(nameof(NewJobPage), typeof(NewJobPage));
        Routing.RegisterRoute(nameof(JobsCollectionPage), typeof(JobsCollectionPage));
        Routing.RegisterRoute(nameof(DeliverySignaturePage), typeof(DeliverySignaturePage));

        // Vehicles
        Routing.RegisterRoute(nameof(NewVehiclePage), typeof(NewVehiclePage));
        Routing.RegisterRoute(nameof(VehicleCollectionPage), typeof(VehicleCollectionPage));

        // Reports
        Routing.RegisterRoute(nameof(ReportsPage), typeof(ReportsPage));

        // Drivers
        Routing.RegisterRoute(nameof(DriverCollectionPage), typeof(DriverCollectionPage));
        Routing.RegisterRoute(nameof(NewDriverPage), typeof(NewDriverPage));
        Routing.RegisterRoute(nameof(EditDriverPage), typeof(EditDriverPage));

        Navigating += OnNavigating;
    }

    private async Task<bool> HasMainUserAsync()
    {
        // Once true, cache it forever (fast path)
        if (_hasMainUser == true)
            return true;

        // Otherwise keep checking until it becomes true
        _hasMainUser = await _userRepository.AnyAsync();
        return _hasMainUser.Value;
    }


    private async void OnNavigating(object? sender, ShellNavigatingEventArgs e)
    {
        // ✅ Always restore session on cold start before any routing rules
        if (!_sessionService.IsAuthenticated)
            await _sessionService.RestoreAsync();

        var target = e.Target.Location.OriginalString;

        // BOOTSTRAP: if no Main user exists yet, force RegisterPage
        var hasMainUser = await HasMainUserAsync();
        if (!hasMainUser)
        {
            if (!target.Contains("RegisterPage"))
            {
                e.Cancel();
                await GoToAsync("///RegisterPage");
            }
            return;
        }

        // If user exists but not authenticated, block app pages
        if (!_sessionService.IsAuthenticated &&
            (target.Contains("DashboardPage") ||
             target.Contains("DriverCollectionPage") ||
             target.Contains("VehicleCollectionPage") ||
             target.Contains("JobsCollectionPage") ||
             target.Contains("ReportsPage")))
        {
            e.Cancel();
            await GoToAsync("///LoginPage");
            return;
        }

        // If authenticated, block Login/Register pages
        if (_sessionService.IsAuthenticated &&
            (target.Contains("LoginPage") || target.Contains("RegisterPage")))
        {
            e.Cancel();
            await GoToAsync("///DashboardPage");
            return;
        }
    }

}
