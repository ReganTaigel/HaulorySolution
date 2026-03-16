using Azure;
using Haulory.Application.Interfaces.Services;
using Haulory.Mobile.Diagnostics;
using Haulory.Mobile.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Haulory.Mobile.ViewModels;

public class LoginViewModel : BaseViewModel
{
    #region Dependencies

    private readonly AuthApiService _authApiService;
    private readonly ISessionService _sessionService;
    private readonly ICrashLogger _crashLogger;

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
        AuthApiService authApiService,
        ISessionService sessionService,
        ICrashLogger crashLogger)
    {
        _authApiService = authApiService;
        _sessionService = sessionService;
        _crashLogger = crashLogger;

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
        await SafeRunner.RunAsync(
            async () =>
            {
                var (result, error) = await _authApiService.LoginAsync(Email, Password);

                if (result is null)
                {
                    await Shell.Current.DisplayAlertAsync(
                        "Login Failed",
                        error ?? "Unable to log in.",
                        "OK");
                    return;
                }

                if (result.AccountId == result.OwnerId)
                {
                    await _sessionService.SetAccountAsync(
                        result.AccountId,
                        result.Token);
                }
                else
                {
                    await _sessionService.SetAccountAsync(
                        result.AccountId,
                        result.OwnerId,
                        result.Token);
                }

                await SecureStorage.Default.SetAsync("haulory_api_token", result.Token);

                await Shell.Current.GoToAsync("///DashboardPage");
            },
            _crashLogger,
            "LoginViewModel.LoginAsync",
            nameof(Views.LoginPage),
            metadataJson: $"{{\"Email\":\"{Email}\"}}",
            onError: async ex =>
            {
                if (ex is HttpRequestException)
                {
                    await Shell.Current.DisplayAlertAsync(
                        "Connection Error",
                        ex.Message,
                        "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync(
                        "Login Error",
                        ex.Message,
                        "OK");
                }
            });
    }

    #endregion
}