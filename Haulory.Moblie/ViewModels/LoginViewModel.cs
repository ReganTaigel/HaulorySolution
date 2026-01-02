using Haulory.Application.Features.Users;
using Haulory.Moblie.ViewModels;
using Haulory.Moblie.Views;
using System.Windows.Input;

public class LoginViewModel : BaseViewModel
{
    private readonly LoginUserHandler _handler;

    public string Email { get; set; }
    public string Password { get; set; }

    public ICommand LoginCommand { get; }
    public ICommand GoToRegisterCommand { get; }

    public LoginViewModel(LoginUserHandler handler)
    {
        _handler = handler;

        LoginCommand = new Command(async () =>
        {
            var success = await _handler.HandleAsync(
                new LoginUserCommand(Email, Password));

            if (success)
                await Shell.Current.GoToAsync("///DashboardPage");

            else
                await Application.Current.MainPage.DisplayAlertAsync(
                    "Login Failed", "Invalid credentials", "OK");
        });

        GoToRegisterCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync("///RegisterPage"); ;
        });

    }
}
