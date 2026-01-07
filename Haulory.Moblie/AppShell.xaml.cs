using Haulory.Application.Interfaces.Services;

namespace Haulory.Moblie;

public partial class AppShell : Shell
{
    private readonly ISessionService _sessionService;

    public AppShell(ISessionService sessionService)
    {
        InitializeComponent();
        _sessionService = sessionService;

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
