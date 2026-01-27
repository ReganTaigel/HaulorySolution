using Haulory.Application.Interfaces.Services;
using Haulory.Mobile.Views;
using Microsoft.Maui.Controls;

namespace Haulory.Mobile;

public partial class App : Microsoft.Maui.Controls.Application
{
    public App(AppShell shell, ISessionService sessionService)
    {
        InitializeComponent();

        MainPage = shell;

        // Restore session (fire and forget)
        Task.Run(async () =>
        {
            await sessionService.RestoreAsync();

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (sessionService.IsAuthenticated)
                    await Shell.Current.GoToAsync("///DashboardPage");
                else
                    await Shell.Current.GoToAsync("///LoginPage");
            });
        });
    }
}
