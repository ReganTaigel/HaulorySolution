using Haulory.Application.Features.Users;
using Haulory.Application.Interfaces.Services;
using System.Windows.Input;

namespace Haulory.Mobile.ViewModels;


// Handles first-time user registration and (on success) auto-login + session initialization.
public class RegisterViewModel : BaseViewModel
{
    #region Dependencies

    private readonly RegisterUserHandler _registerHandler;
    private readonly LoginUserHandler _loginHandler;
    private readonly ISessionService _sessionService;

    #endregion

    #region State Flags

    // Prevents double-taps / double submissions while registration is in progress.
    private bool _isRegistering;

    #endregion

    #region Backing Fields

    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;

    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _email = string.Empty;

    #endregion

    #region Bindable Properties

    public string FirstName
    {
        get => _firstName;
        set => SetProperty(ref _firstName, value);
    }

    public string LastName
    {
        get => _lastName;
        set => SetProperty(ref _lastName, value);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Password
    {
        get => _password;
        set
        {
            SetProperty(ref _password, value);
            RaisePasswordMatchState();
        }
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set
        {
            SetProperty(ref _confirmPassword, value);
            RaisePasswordMatchState();
        }
    }

    #endregion

    #region Validation UI (Passwords)

    // True only when both fields are filled and identical.
    public bool PasswordsMatch =>
        !string.IsNullOrEmpty(Password) &&
        Password == ConfirmPassword;

    // Friendly inline message that only appears after the user has started typing confirm password.
    public string PasswordsMatchMessage =>
        string.IsNullOrEmpty(ConfirmPassword) ? string.Empty :
        PasswordsMatch ? "Passwords match" : "Passwords do not match";

    // Inline color indicator for password match feedback.
    public Color PasswordsMatchColor => PasswordsMatch ? Colors.Green : Colors.Red;

    // Centralised "invalidate UI" method for password-match dependent properties.
    // Keeps the setters clean and prevents duplication.
    private void RaisePasswordMatchState()
    {
        OnPropertyChanged(nameof(PasswordsMatch));
        OnPropertyChanged(nameof(PasswordsMatchMessage));
        OnPropertyChanged(nameof(PasswordsMatchColor));
    }

    #endregion

    #region Commands

    public ICommand RegisterCommand { get; }

    #endregion

    #region Constructor

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

    #endregion

    #region Command Handlers

    // Creates the account, then attempts an immediate login and stores the session account id.
    // Navigation rules:
    // - If registration fails -> show error, stay on page
    // - If registration ok but login fails -> route to LoginPage
    // - If login ok -> set session and route to DashboardPage
    private async Task ExecuteRegisterAsync()
    {
        if (_isRegistering) return;
        _isRegistering = true;

        try
        {
            // Basic front-end guard. Server-side should still validate too.
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

            // Persist active account into session for downstream features.
            await _sessionService.SetAccountAsync(user.Id);

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

    #endregion
}
