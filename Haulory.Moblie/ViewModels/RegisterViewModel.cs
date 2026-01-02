using Haulory.Application.Features.Users;
using Haulory.Moblie.Views;
using System.Windows.Input;
namespace Haulory.Moblie.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly RegisterUserHandler _Handler;

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
                var success = await _Handler.HandleAsync(
                    new RegisterUserCommand(FirstName,
                    LastName,
                    Email,
                    Password));

                if (success)
                    await Shell.Current.GoToAsync("///LoginPage");

                ;
                await Microsoft.Maui.Controls.Application
                    .Current!
                    .Dispatcher.DispatchAsync(async () =>
                    {
                        await Shell.Current.DisplayAlertAsync(
                        "Error",
                        "User already exists",
                        "OK");

                    });

            });
        }
    }
}
