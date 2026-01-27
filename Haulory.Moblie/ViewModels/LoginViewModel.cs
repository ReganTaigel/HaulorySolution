using Haulory.Application.Features.Users;
using Haulory.Application.Interfaces.Services;
using System.Windows.Input;

namespace Haulory.Mobile.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly LoginUserHandler _handler;
    private readonly ISessionService _sessionService;

    public string Email { get; set; }
    public string Password { get; set; }

    public ICommand LoginCommand { get; }
    public ICommand GoToRegisterCommand { get; }

    public LoginViewModel(
        LoginUserHandler handler,
        ISessionService sessionService)
    {
        _handler = handler;
        _sessionService = sessionService;

        LoginCommand = new Command(async () => await LoginAsync());

        GoToRegisterCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync("///RegisterPage");
        });
    }

    private async Task LoginAsync()
    {
        var user = await _handler.HandleAsync(
            new LoginUserCommand(Email, Password));

        if (user != null)
        {
            // ✅ Store session
            await _sessionService.SetUserAsync(user);

            // ✅ Navigate to app root
            await Shell.Current.GoToAsync("///DashboardPage");
        }
        else
        {
            // ✅ Correct, non-deprecated alert
            await Shell.Current.DisplayAlert(
                "Login Failed",
                "Invalid credentials",
                "OK");
        }
    }
}
