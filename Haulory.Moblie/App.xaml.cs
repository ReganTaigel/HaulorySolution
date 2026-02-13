using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Microsoft.Maui.Controls;

namespace Haulory.Mobile;

public partial class App : Microsoft.Maui.Controls.Application
{
    public App(
        AppShell shell,
        ISessionService sessionService,
        IUserRepository userRepository)
    {
        InitializeComponent();

        MainPage = shell;

        Task.Run(async () =>
        {
            var hasMainUser = await userRepository.AnyAsync();
            await sessionService.RestoreAsync();

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (!hasMainUser)
                    await Shell.Current.GoToAsync("///RegisterPage");
                else if (sessionService.IsAuthenticated)
                    await Shell.Current.GoToAsync("///DashboardPage");
                else
                    await Shell.Current.GoToAsync("///LoginPage");
            });
        });
    }
}
