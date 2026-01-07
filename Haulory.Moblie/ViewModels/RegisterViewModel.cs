using Haulory.Application.Features.Users;
using Haulory.Moblie.ViewModels;
using System.Windows.Input;

public class RegisterViewModel : BaseViewModel
{
    private readonly RegisterUserHandler _Handler;
    private bool _isRegistering;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public ICommand RegisterCommand { get; }

    public RegisterViewModel(RegisterUserHandler handler)
    {
        _Handler = handler;

        RegisterCommand = new Command(async () =>
        {
            if (_isRegistering) return;
            _isRegistering = true;

            try
            {
                var success = await _Handler.HandleAsync(
                    new RegisterUserCommand(
                        FirstName,
                        LastName,
                        Email,
                        Password));

                if (success)
                {
                    // Show success feedback
                    await Shell.Current.DisplayAlertAsync(
                        "Registration Successful",
                        "Your account has been created. Please log in.",
                        "OK");

                    // Navigate AFTER alert
                    await Shell.Current.GoToAsync("///LoginPage");
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync(
                    "Registration Failed",
                    "Password must be at least 8 characters and include 2 numbers and 2 special characters.",
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
