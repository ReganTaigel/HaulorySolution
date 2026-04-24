using Azure;
using HaulitCore.Application.Interfaces.Services;
using HaulitCore.Mobile.Diagnostics;
using HaulitCore.Mobile.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HaulitCore.Mobile.ViewModels;

public class LoginViewModel : BaseViewModel
{
    #region Dependencies

    private readonly AuthApiService _authApiService;
    private readonly ISessionService _sessionService;
    private readonly ICrashLogger _crashLogger;

    #endregion

    #region Private Properties

    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _loginStatusMessage = string.Empty;
    private bool _isBusy;

    #endregion

    #region Bindable Properties

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string LoginStatusMessage
    {
        get => _loginStatusMessage;
        set
        {
            if (SetProperty(ref _loginStatusMessage, value))
            {
                OnPropertyChanged(nameof(IsStatusMessageVisible));
            }
        }
    }

    public bool IsStatusMessageVisible => !string.IsNullOrWhiteSpace(LoginStatusMessage);

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(LoginButtonText));
            }
        }
    }

    public string LoginButtonText => IsBusy ? "Signing in..." : "Sign In";

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

        LoginCommand = new Command(async () => await LoginAsync(), () => !IsBusy);

        GoToRegisterCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync("///RegisterPage");
        });
    }

    #endregion

    #region Login Logic

    private async Task LoginAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        LoginStatusMessage = string.Empty;
        RefreshLoginState();

        CancellationTokenSource? slowLoginCts = null;

        try
        {
            slowLoginCts = new CancellationTokenSource();

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(3000, slowLoginCts.Token);

                    if (!slowLoginCts.Token.IsCancellationRequested)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            LoginStatusMessage = "Signing you in... this is taking a little longer than usual.";
                            RefreshLoginState();
                        });
                    }
                }
                catch (TaskCanceledException)
                {
                }
            });

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

                    await SecureStorage.Default.SetAsync("HaulitCore_api_token", result.Token);

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
        finally
        {
            slowLoginCts?.Cancel();
            slowLoginCts?.Dispose();

            IsBusy = false;
            LoginStatusMessage = string.Empty;
            RefreshLoginState();
        }
    }
    private void RefreshLoginState()
    {
        OnPropertyChanged(nameof(LoginButtonText));
        OnPropertyChanged(nameof(IsStatusMessageVisible));
        (LoginCommand as Command)?.ChangeCanExecute();
    }
    #endregion
}