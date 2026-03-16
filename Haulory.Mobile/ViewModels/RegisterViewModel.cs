using Azure;
using Haulory.Application.Interfaces.Services;
using Haulory.Contracts.Auth;
using Haulory.Mobile.Services;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Haulory.Mobile.ViewModels;

public class RegisterViewModel : BaseViewModel
{
    #region Dependencies

    private readonly AuthApiService _authApiService;
    private readonly ISessionService _sessionService;

    #endregion

    #region State Flags

    private bool _isRegistering;

    #endregion

    #region Backing Fields

    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;

    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _email = string.Empty;

    private string _businessName = string.Empty;
    private string _businessEmail = string.Empty;
    private string _businessPhone = string.Empty;

    private string _businessAddress1 = string.Empty;
    private string _businessAddress2 = string.Empty;
    private string _businessSuburb = string.Empty;
    private string _businessCity = string.Empty;
    private string _businessRegion = string.Empty;
    private string _businessPostcode = string.Empty;
    private string _businessCountry = string.Empty;

    private string _supplierGstNumber = string.Empty;
    private string _supplierNzbn = string.Empty;

    #endregion

    #region Bindable Properties - Identity

    public string FirstName
    {
        get => _firstName;
        set { SetProperty(ref _firstName, value); RefreshRegisterState(); }
    }

    public string LastName
    {
        get => _lastName;
        set { SetProperty(ref _lastName, value); RefreshRegisterState(); }
    }

    public string Email
    {
        get => _email;
        set { SetProperty(ref _email, value); RefreshRegisterState(); }
    }

    #endregion

    #region Bindable Properties - Password

    public string Password
    {
        get => _password;
        set
        {
            SetProperty(ref _password, value);
            RaisePasswordMatchState();
            RefreshRegisterState();
        }
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set
        {
            SetProperty(ref _confirmPassword, value);
            RaisePasswordMatchState();
            RefreshRegisterState();
        }
    }

    #endregion

    #region Bindable Properties - Business

    public string BusinessName
    {
        get => _businessName;
        set { SetProperty(ref _businessName, value); RefreshRegisterState(); }
    }

    public string BusinessEmail
    {
        get => _businessEmail;
        set { SetProperty(ref _businessEmail, value); RefreshRegisterState(); }
    }

    public string BusinessPhone
    {
        get => _businessPhone;
        set { SetProperty(ref _businessPhone, value); RefreshRegisterState(); }
    }

    public string BusinessAddress1
    {
        get => _businessAddress1;
        set { SetProperty(ref _businessAddress1, value); RefreshRegisterState(); }
    }

    public string BusinessAddress2
    {
        get => _businessAddress2;
        set { SetProperty(ref _businessAddress2, value); RefreshRegisterState(); }
    }

    public string BusinessSuburb
    {
        get => _businessSuburb;
        set { SetProperty(ref _businessSuburb, value); RefreshRegisterState(); }
    }

    public string BusinessCity
    {
        get => _businessCity;
        set { SetProperty(ref _businessCity, value); RefreshRegisterState(); }
    }

    public string BusinessRegion
    {
        get => _businessRegion;
        set { SetProperty(ref _businessRegion, value); RefreshRegisterState(); }
    }

    public string BusinessPostcode
    {
        get => _businessPostcode;
        set { SetProperty(ref _businessPostcode, value); RefreshRegisterState(); }
    }

    public string BusinessCountry
    {
        get => _businessCountry;
        set { SetProperty(ref _businessCountry, value); RefreshRegisterState(); }
    }

    public string SupplierGstNumber
    {
        get => _supplierGstNumber;
        set { SetProperty(ref _supplierGstNumber, value); RefreshRegisterState(); }
    }

    public string SupplierNzbn
    {
        get => _supplierNzbn;
        set { SetProperty(ref _supplierNzbn, value); RefreshRegisterState(); }
    }

    #endregion

    #region Validation UI

    public bool PasswordsMatch =>
        !string.IsNullOrEmpty(Password) &&
        Password == ConfirmPassword;

    public string PasswordsMatchMessage =>
        string.IsNullOrEmpty(ConfirmPassword) ? string.Empty :
        PasswordsMatch ? "Passwords match" : "Passwords do not match";

    public Color PasswordsMatchColor => PasswordsMatch ? Colors.Green : Colors.Red;

    private void RaisePasswordMatchState()
    {
        OnPropertyChanged(nameof(PasswordsMatch));
        OnPropertyChanged(nameof(PasswordsMatchMessage));
        OnPropertyChanged(nameof(PasswordsMatchColor));
    }

    #endregion

    #region Validation

    public bool CanRegister
    {
        get
        {
            if (_isRegistering) return false;
            if (string.IsNullOrWhiteSpace(FirstName)) return false;
            if (string.IsNullOrWhiteSpace(LastName)) return false;

            var email = Email?.Trim();
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@')) return false;

            if (string.IsNullOrWhiteSpace(Password)) return false;
            if (!PasswordsMatch) return false;
            if (string.IsNullOrWhiteSpace(BusinessName)) return false;

            return true;
        }
    }

    private void RefreshRegisterState()
    {
        OnPropertyChanged(nameof(CanRegister));
        (RegisterCommand as Command)?.ChangeCanExecute();
    }

    #endregion

    #region Commands

    public ICommand RegisterCommand { get; }

    #endregion

    #region Constructor

    public RegisterViewModel(
        AuthApiService authApiService,
        ISessionService sessionService)
    {
        _authApiService = authApiService;
        _sessionService = sessionService;

        RegisterCommand = new Command(async () => await ExecuteRegisterAsync(), () => CanRegister);

        RefreshRegisterState();
    }

    #endregion

    #region Command Handlers

    private async Task ExecuteRegisterAsync()
    {
        if (!CanRegister || _isRegistering)
            return;

        _isRegistering = true;
        RefreshRegisterState();

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

            var email = (Email?.Trim() ?? string.Empty).ToLowerInvariant();

            var registerRequest = new RegisterRequest
            {
                FirstName = FirstName?.Trim() ?? string.Empty,
                LastName = LastName?.Trim() ?? string.Empty,
                Email = email,

                BusinessName = BusinessName?.Trim() ?? string.Empty,
                BusinessEmail = string.IsNullOrWhiteSpace(BusinessEmail) ? null : BusinessEmail.Trim().ToLowerInvariant(),
                BusinessPhone = string.IsNullOrWhiteSpace(BusinessPhone) ? null : BusinessPhone.Trim(),

                BusinessAddress1 = string.IsNullOrWhiteSpace(BusinessAddress1) ? null : BusinessAddress1.Trim(),
                BusinessAddress2 = string.IsNullOrWhiteSpace(BusinessAddress2) ? null : BusinessAddress2.Trim(),
                BusinessSuburb = string.IsNullOrWhiteSpace(BusinessSuburb) ? null : BusinessSuburb.Trim(),
                BusinessCity = string.IsNullOrWhiteSpace(BusinessCity) ? null : BusinessCity.Trim(),
                BusinessRegion = string.IsNullOrWhiteSpace(BusinessRegion) ? null : BusinessRegion.Trim(),
                BusinessPostcode = string.IsNullOrWhiteSpace(BusinessPostcode) ? null : BusinessPostcode.Trim(),
                BusinessCountry = string.IsNullOrWhiteSpace(BusinessCountry) ? null : BusinessCountry.Trim(),

                SupplierGstNumber = string.IsNullOrWhiteSpace(SupplierGstNumber) ? null : SupplierGstNumber.Trim(),
                SupplierNzbn = string.IsNullOrWhiteSpace(SupplierNzbn) ? null : SupplierNzbn.Trim(),

                Password = Password
            };
            var registerResult = await _authApiService.RegisterAsync(registerRequest);

            if (!registerResult.Success)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Registration Failed",
                    registerResult.Error ?? "Unable to create account.",
                    "OK");
                return;
            }

            var (response, error) = await _authApiService.LoginAsync(email, Password);

            if (response is null)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Registration Successful",
                    error ?? "Account created. Please log in.",
                    "OK");

                await Shell.Current.GoToAsync("///LoginPage");
                return;
            }

            if (response.AccountId == response.OwnerId)
            {
                await _sessionService.SetAccountAsync(
                    response.AccountId,
                    response.Token);
            }
            else
            {
                await _sessionService.SetAccountAsync(
                    response.AccountId,
                    response.OwnerId,
                    response.Token);
            }

            await SecureStorage.Default.SetAsync("haulory_api_token", response.Token);

            await Shell.Current.DisplayAlertAsync(
                "Welcome",
                "Main account created.",
                "OK");

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
                "Registration Error",
                ex.Message,
                "OK");
        }
        finally
        {
            _isRegistering = false;
            RefreshRegisterState();
        }
    }

    #endregion
}