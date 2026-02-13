using Haulory.Application.Features.Users;
using Haulory.Application.Interfaces.Services;
using System.Windows.Input;

namespace Haulory.Mobile.ViewModels;

public class RegisterViewModel : BaseViewModel
{
    private readonly RegisterUserHandler _registerHandler;
    private readonly LoginUserHandler _loginHandler;
    private readonly ISessionService _sessionService;

    private bool _isRegistering;

    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public string Password
    {
        get => _password;
        set
        {
            SetProperty(ref _password, value);
            OnPropertyChanged(nameof(PasswordsMatch));
            OnPropertyChanged(nameof(PasswordsMatchMessage));
            OnPropertyChanged(nameof(PasswordsMatchColor));
        }
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set
        {
            SetProperty(ref _confirmPassword, value);
            OnPropertyChanged(nameof(PasswordsMatch));
            OnPropertyChanged(nameof(PasswordsMatchMessage));
            OnPropertyChanged(nameof(PasswordsMatchColor));
        }
    }

    public bool PasswordsMatch =>
        !string.IsNullOrEmpty(Password) &&
        Password == ConfirmPassword;

    public string PasswordsMatchMessage =>
        string.IsNullOrEmpty(ConfirmPassword) ? string.Empty :
        PasswordsMatch ? "Passwords match" : "Passwords do not match";

    public Color PasswordsMatchColor => PasswordsMatch ? Colors.Green : Colors.Red;

    public ICommand RegisterCommand { get; }

    public RegisterViewModel(
        RegisterUserHandler registerHandler,
        LoginUserHandler loginHandler,
        ISessionService sessionService)
    {
        _registerHandler = registerHandler;
        _loginHandler = loginHandler;
        _sessionService = sessionService;

        RegisterCommand = new Command(async () => await ExecuteRegisterAsync());
    }

    private async Task ExecuteRegisterAsync()
    {
        if (_isRegistering) return;
        _isRegistering = true;

        try
        {
            if (!PasswordsMatch)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Registration Failed",
                    "Passwords do not match.",
                    "OK");
                return;
            }

            var result = await _registerHandler.HandleAsync(
                new RegisterUserCommand(FirstName, LastName, Email, Password));

            if (!result.Success)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Registration Failed",
                    result.Error ?? "Registration failed.",
                    "OK");
                return;
            }


            // Auto-login immediately (uses same validation as normal login)
            var user = await _loginHandler.HandleAsync(new LoginUserCommand(Email, Password));
            if (user == null)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Registration Successful",
                    "Account created. Please log in.",
                    "OK");
                await Shell.Current.GoToAsync("///LoginPage");
                return;
            }

            await _sessionService.SetUserAsync(user);

            await Shell.Current.DisplayAlertAsync(
                "Welcome",
                "Main account created.",
                "OK");

            await Shell.Current.GoToAsync("///DashboardPage");
        }
        finally
        {
            _isRegistering = false;
        }
    }
}
