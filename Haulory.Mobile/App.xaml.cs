using Haulory.Application.Interfaces.Services;
using Microsoft.Maui.Controls;

namespace Haulory.Mobile;

public partial class App : Microsoft.Maui.Controls.Application

{
    private readonly ISessionService _sessionService;
    private readonly AppShell _shell;
    private bool _startupNavigationDone;

    public App(
        AppShell shell,
        ISessionService sessionService)
    {
        InitializeComponent();

        _shell = shell;
        _sessionService = sessionService;

        MainPage = _shell;

        _shell.Loaded += OnShellLoaded;
    }

    private async void OnShellLoaded(object? sender, EventArgs e)
    {
        if (_startupNavigationDone)
            return;

        _startupNavigationDone = true;

        try
        {
            await _sessionService.RestoreAsync();

            if (_sessionService.IsAuthenticated)
                await _shell.GoToAsync("///DashboardPage");
            else
                await _shell.GoToAsync("///LoginPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);

            try
            {
                await _shell.GoToAsync("///LoginPage");
            }
            catch (Exception navEx)
            {
                System.Diagnostics.Debug.WriteLine(navEx);
            }
        }
    }
}