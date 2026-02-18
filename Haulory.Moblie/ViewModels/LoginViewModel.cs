using System.Threading.Tasks;
using System.Windows.Input;
using Haulory.Application.Features.Users;
using Haulory.Application.Interfaces.Services;
using Microsoft.Maui.Controls;

namespace Haulory.Mobile.ViewModels;

public class LoginViewModel : BaseViewModel
{
    #region Dependencies

    private readonly LoginUserHandler _handler;
    private readonly ISessionService _sessionService;

    #endregion

    #region Bindable Properties

    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    #endregion

    #region Commands

    public ICommand LoginCommand { get; }
    public ICommand GoToRegisterCommand { get; }

    #endregion

    #region Constructor

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

    #endregion

    #region Login Logic

    private async Task LoginAsync()
    {
        var user = await _handler.HandleAsync(
            new LoginUserCommand(Email, Password));

        if (user != null)
        {
            // Store authenticated session
            await _sessionService.SetAccountAsync(user.Id);

            // Navigate to app root
            await Shell.Current.GoToAsync("///DashboardPage");
        }
        else
        {
            // Display login failure
            await Shell.Current.DisplayAlertAsync(
                "Login Failed",
                "Invalid credentials",
                "OK");
        }
    }

    #endregion
}
