using System.Threading.Tasks;
using System.Windows.Input;
using Haulory.Application.Features.Users;
using Microsoft.Maui.Controls;

namespace Haulory.Mobile.ViewModels;

public class LoginViewModel : BaseViewModel
{
    #region Dependencies

    private readonly LoginUserHandler _handler;

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

    public LoginViewModel(LoginUserHandler handler)
    {
        _handler = handler;

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
        var user = await _handler.HandleAsync(new LoginUserCommand(Email, Password));

        if (user != null)
        {
            // Session already set by LoginUserHandler
            await Shell.Current.GoToAsync("///DashboardPage");
        }
        else
        {
            await Shell.Current.DisplayAlertAsync(
                "Login Failed",
                "Invalid credentials",
                "OK");
        }
    }

    #endregion
}