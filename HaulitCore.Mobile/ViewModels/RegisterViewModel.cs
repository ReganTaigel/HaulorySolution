using HaulitCore.Application.Interfaces.Services;
using HaulitCore.Application.Security;
using HaulitCore.Contracts.Auth;
using HaulitCore.Mobile.Diagnostics;
using HaulitCore.Mobile.Services;
using HaulitCore.Mobile.Views;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HaulitCore.Mobile.ViewModels;

public class RegisterViewModel : BaseViewModel
{
    #region Dependencies

    private readonly AuthApiService _authApiService;
    private readonly ISessionService _sessionService;
    private readonly ICrashLogger _crashLogger;

    #endregion

    #region State Flags

    private bool _isRegistering;

    #endregion

    #region Backing Fields

    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;

    private bool _isPasswordHidden = true;
    private bool _isConfirmPasswordHidden = true;

    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _email = string.Empty;

    private string _businessName = string.Empty;
    private string _businessEmail = string.Empty;
    private string _businessPhone = string.Empty;

    private string _businessAddress1 = string.Empty;
    private string _businessSuburb = string.Empty;
    private string _businessCity = string.Empty;
    private string _businessRegion = string.Empty;
    private string _businessPostcode = string.Empty;
    private string _businessCountry = string.Empty;

    private string _supplierGstNumber = string.Empty;
    private string _supplierNzbn = string.Empty;
    private string _bankAccountNumber = string.Empty;

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
    public bool IsPasswordHidden
    {
        get => _isPasswordHidden;
        set
        {
            if (SetProperty(ref _isPasswordHidden, value))
                OnPropertyChanged(nameof(PasswordToggleText));
        }
    }

    public string PasswordToggleText => IsPasswordHidden ? "Show" : "Hide";


    public bool IsConfirmPasswordHidden
    {
        get => _isConfirmPasswordHidden;
        set
        {
            if (SetProperty(ref _isConfirmPasswordHidden, value))
                OnPropertyChanged(nameof(ConfirmPasswordToggleText));
        }
    }

    public string ConfirmPasswordToggleText => IsConfirmPasswordHidden ? "Show" : "Hide";
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
    public string BankAccountNumber
    {
        get => _bankAccountNumber;
        set { SetProperty(ref _bankAccountNumber, value); RefreshRegisterState(); }
    }
    #endregion

    #region Validation UI

    public bool PasswordsMatch =>
        !string.IsNullOrEmpty(Password) &&
        Password == ConfirmPassword;

    private void RaisePasswordMatchState()
    {
        OnPropertyChanged(nameof(PasswordsMatch));
      
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
            if (string.IsNullOrWhiteSpace(ConfirmPassword)) return false;
            if (!PasswordsMatch) return false;

            if (string.IsNullOrWhiteSpace(BusinessName)) return false;

            if (string.IsNullOrWhiteSpace(BusinessEmail) || !BusinessEmail.Trim().Contains('@')) return false;
            if (string.IsNullOrWhiteSpace(BusinessPhone)) return false;
            if (string.IsNullOrWhiteSpace(BusinessAddress1)) return false;
            if (string.IsNullOrWhiteSpace(BusinessCity)) return false;
            if (string.IsNullOrWhiteSpace(BusinessCountry)) return false;

            return true;
        }
    }

    private void RefreshRegisterState()
    {
        OnPropertyChanged(nameof(CanRegister));
    }
    private string GetRegistrationValidationMessage()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(FirstName))
            errors.Add("First name is required.");

        if (string.IsNullOrWhiteSpace(LastName))
            errors.Add("Last name is required.");

        var email = Email?.Trim();
        if (string.IsNullOrWhiteSpace(email))
            errors.Add("Login email is required.");
        else if (!email.Contains('@'))
            errors.Add("Login email must be valid.");

        // Use PasswordPolicy (single source of truth)
        if (!PasswordPolicy.IsValid(Password, out var passwordError))
        {
            errors.Add(passwordError);
        }

        if (string.IsNullOrWhiteSpace(ConfirmPassword))
            errors.Add("Confirm password is required.");

        // Only check match if both entered AND password itself is valid
        if (PasswordPolicy.IsValid(Password, out _) &&
            !string.IsNullOrWhiteSpace(ConfirmPassword) &&
            Password != ConfirmPassword)
        {
            errors.Add("Passwords do not match.");
        }

        if (string.IsNullOrWhiteSpace(BusinessName))
            errors.Add("Business name is required.");

        if (string.IsNullOrWhiteSpace(BusinessEmail))
            errors.Add("Business email is required.");
        else if (!BusinessEmail.Trim().Contains('@'))
            errors.Add("Business email must be valid.");

        if (string.IsNullOrWhiteSpace(BusinessPhone))
            errors.Add("Business phone is required.");

        if (string.IsNullOrWhiteSpace(BusinessAddress1))
            errors.Add("Business address is required.");

        if (string.IsNullOrWhiteSpace(BusinessCity))
            errors.Add("City is required.");

        if (string.IsNullOrWhiteSpace(BusinessCountry))
            errors.Add("Country is required.");

        return errors.Count == 0
            ? string.Empty
            : string.Join(Environment.NewLine, errors);
    }
    private bool IsTaxDetailsEmpty()
    {
        return string.IsNullOrWhiteSpace(SupplierGstNumber)
            && string.IsNullOrWhiteSpace(SupplierNzbn)
            && string.IsNullOrWhiteSpace(BankAccountNumber);
    }
    #endregion

    #region Commands

    public ICommand RegisterCommand { get; }
    public ICommand TogglePasswordVisibilityCommand { get; }
    public ICommand ToggleConfirmPasswordVisibilityCommand { get; }
    public ICommand GoToLoginCommand { get; }
    #endregion

    #region Constructor

    public RegisterViewModel(
        AuthApiService authApiService,
        ISessionService sessionService,
        ICrashLogger crashLogger)
    {
        _authApiService = authApiService;
        _sessionService = sessionService;
        _crashLogger = crashLogger;

        RegisterCommand = new Command(async () => await ExecuteRegisterAsync());
        GoToLoginCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
        });

        TogglePasswordVisibilityCommand = new Command(() =>
        {
            IsPasswordHidden = !IsPasswordHidden;
        });

        ToggleConfirmPasswordVisibilityCommand = new Command(() =>
        {
            IsConfirmPasswordHidden = !IsConfirmPasswordHidden;
        });
        RefreshRegisterState();
    }

    #endregion

    #region Command Handlers

    private async Task ExecuteRegisterAsync()
    {
        if (_isRegistering)
            return;

        var validationMessage = GetRegistrationValidationMessage();
        if (!string.IsNullOrWhiteSpace(validationMessage))
        {
            await Shell.Current.DisplayAlertAsync(
                "Registration Failed",
                validationMessage,
                "OK");
            return;
        }
        if (IsTaxDetailsEmpty())
        {
            bool continueWithoutTaxDetails = await Shell.Current.DisplayAlertAsync(
                "Tax Details",
                "This information is used for automated invoicing. Continue without it?",
                "Continue",
                "Cancel");

            if (!continueWithoutTaxDetails)
                return;
        }
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

            await SafeRunner.RunAsync(
                async () =>
                {
                    var registerRequest = new RegisterRequest
                    {
                        FirstName = FirstName?.Trim() ?? string.Empty,
                        LastName = LastName?.Trim() ?? string.Empty,
                        Email = email,

                        BusinessName = BusinessName?.Trim() ?? string.Empty,
                        BusinessEmail = string.IsNullOrWhiteSpace(BusinessEmail) ? null : BusinessEmail.Trim().ToLowerInvariant(),
                        BusinessPhone = string.IsNullOrWhiteSpace(BusinessPhone) ? null : BusinessPhone.Trim(),

                        BusinessAddress1 = string.IsNullOrWhiteSpace(BusinessAddress1) ? null : BusinessAddress1.Trim(),
                        BusinessSuburb = string.IsNullOrWhiteSpace(BusinessSuburb) ? null : BusinessSuburb.Trim(),
                        BusinessCity = string.IsNullOrWhiteSpace(BusinessCity) ? null : BusinessCity.Trim(),
                        BusinessRegion = string.IsNullOrWhiteSpace(BusinessRegion) ? null : BusinessRegion.Trim(),
                        BusinessPostcode = string.IsNullOrWhiteSpace(BusinessPostcode) ? null : BusinessPostcode.Trim(),
                        BusinessCountry = string.IsNullOrWhiteSpace(BusinessCountry) ? null : BusinessCountry.Trim(),

                        SupplierGstNumber = string.IsNullOrWhiteSpace(SupplierGstNumber) ? null : SupplierGstNumber.Trim(),
                        SupplierNzbn = string.IsNullOrWhiteSpace(SupplierNzbn) ? null : SupplierNzbn.Trim(),
                        BankAccountNumber = string.IsNullOrWhiteSpace(BankAccountNumber) ? null : BankAccountNumber.Trim(),
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

                    await SecureStorage.Default.SetAsync("HaulitCore_api_token", response.Token);

                    await Shell.Current.DisplayAlertAsync(
                        "Welcome",
                        "Main account created.",
                        "OK");

                    await Shell.Current.GoToAsync("///DashboardPage");
                },
                _crashLogger,
                "RegisterViewModel.ExecuteRegisterAsync",
                nameof(Views.RegisterPage),
                metadataJson: $"{{\"Email\":\"{email}\",\"BusinessName\":\"{BusinessName?.Trim()}\"}}",
                onError: async ex =>
                {
                    var title = ex is HttpRequestException ? "Connection Error" : "Registration Error";

                    await Shell.Current.DisplayAlertAsync(
                        title,
                        ex.Message,
                        "OK");
                });
        }
        finally
        {
            _isRegistering = false;
            RefreshRegisterState();
        }
    }

    #endregion
}