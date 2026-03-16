using Azure;
using Haulory.Application.Interfaces.Services;
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
        ISessionService sessionService)
    {
        _authApiService = authApiService;
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
        try
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
        }
        catch (HttpRequestException ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Connection Error",
                ex.Message,
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Login Error",
                ex.Message,
                "OK");
        }
    }

    #endregion
}