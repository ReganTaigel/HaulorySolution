using Haulory.Application.Interfaces.Services;
using Haulory.Moblie.Views;

namespace Haulory.Moblie;

public partial class AppShell : Shell
{
    private readonly ISessionService _sessionService;

    public AppShell(ISessionService sessionService)
    {
        InitializeComponent();
        _sessionService = sessionService;

        Routing.RegisterRoute(nameof(NewJobPage), typeof(NewJobPage));
        Routing.RegisterRoute(nameof(JobsCollectionPage), typeof(JobsCollectionPage));
        Routing.RegisterRoute(nameof(VehiclesPage), typeof(VehiclesPage));
        Routing.RegisterRoute(nameof(DriversPage), typeof(DriversPage));
        Routing.RegisterRoute(nameof(ReportsPage), typeof(ReportsPage));
        Routing.RegisterRoute(nameof(DeliverySignaturePage), typeof(DeliverySignaturePage));


        Navigating += OnNavigating;
    }

    private async void OnNavigating(object? sender, ShellNavigatingEventArgs e)
    {
        var target = e.Target.Location.OriginalString;

        // Block Dashboard if not logged in
        if (!_sessionService.IsAuthenticated &&
            target.Contains("DashboardPage"))
        {
            e.Cancel();
            await GoToAsync("///LoginPage");
        }

        // Block Login/Register if already logged in
        if (_sessionService.IsAuthenticated &&
            (target.Contains("LoginPage") || target.Contains("RegisterPage")))
        {
            e.Cancel();
            await GoToAsync("///DashboardPage");
        }
    }
}
