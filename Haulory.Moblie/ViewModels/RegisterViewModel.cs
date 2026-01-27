using Haulory.Application.Features.Users;
using System.Windows.Input;

namespace Haulory.Mobile.ViewModels;

public class RegisterViewModel : BaseViewModel
{
    private readonly RegisterUserHandler _handler;
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

    // LIVE INDICATOR PROPERTIES

    public bool PasswordsMatch =>
        !string.IsNullOrEmpty(Password) &&
        Password == ConfirmPassword;

    public string PasswordsMatchMessage
    {
        get
        {
            if (string.IsNullOrEmpty(ConfirmPassword))
                return string.Empty;

            return PasswordsMatch
                ? "Passwords match"
                : "Passwords do not match";
        }
    }

    public Color PasswordsMatchColor =>
        PasswordsMatch ? Colors.Green : Colors.Red;

    public ICommand RegisterCommand { get; }

    public RegisterViewModel(RegisterUserHandler handler)
    {
        _handler = handler;

        RegisterCommand = new Command(async () =>
        {
            if (_isRegistering) return;
            _isRegistering = true;

            try
            {
                // Prevent submit if passwords don't match
                if (!PasswordsMatch)
                {
                    await Shell.Current.DisplayAlertAsync(
                        "Registration Failed", 
                        "Passwords do not match.",
                        "OK");
                    return;
                }

                var success = await _handler.HandleAsync(
                    new RegisterUserCommand(
                        FirstName,
                        LastName,
                        Email,
                        Password));

                if (success)
                {
                    await Shell.Current.DisplayAlertAsync(
                        "Registration Successful",
                        "Your account has been created. Please log in.",
                        "OK");

                    await Shell.Current.GoToAsync("///LoginPage");
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync(
                        "Registration Failed",
                        "A user with this email already exists or the password does not meet requirements.",
                        "OK");
                }
            }
            finally
            {
                _isRegistering = false;
            }
        });
    }
}
