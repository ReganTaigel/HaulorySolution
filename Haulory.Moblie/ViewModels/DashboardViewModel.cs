using Haulory.Application.Interfaces.Services;
using System.Windows.Input;

namespace Haulory.Moblie.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private readonly ISessionService _sessionService;

    public ICommand LogoutCommand { get; }

    public DashboardViewModel(ISessionService sessionService)
    {
        _sessionService = sessionService;

        LogoutCommand = new Command(async () => await LogoutAsync());
    }

    private async Task LogoutAsync()
    {
        // Clear session
        await _sessionService.ClearAsync();

        // Reset navigation stack to Login
        await Shell.Current.GoToAsync("///LoginPage");
    }
}
