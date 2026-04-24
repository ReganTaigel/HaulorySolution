using HaulitCore.Contracts.Settings;
using HaulitCore.Mobile.Services;
using System.Globalization;
using System.Windows.Input;

namespace HaulitCore.Mobile.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    #region Dependencies

    private readonly DocumentSettingsApiService _settingsApiService;

    #endregion

    public SettingsSectionViewModel InvoiceSection { get; }
    public SettingsSectionViewModel PodSection { get; }
    public SettingsSectionViewModel BusinessTaxSection { get; }

    #region Backing Fields

    private bool _gstEnabled;
    private string _gstRatePercent = string.Empty;

    private bool _fuelSurchargeEnabled;
    private string _fuelSurchargePercent = string.Empty;

    private string _invoicePrefix = "INV";
    private string _podPrefix = "POD";

    private int _paymentTermsDays = 28;

    private bool _showDamageNotesOnPod;
    private bool _showWaitTimeOnPod;

    private string _waitTimeCharge = string.Empty;
    private bool _waitTimeChargeEnabled;

    private string _handUnloadCharge = string.Empty;
    private bool _handUnloadChargeEnabled;

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

    #region Bindable Properties

    public bool GstEnabled
    {
        get => _gstEnabled;
        set
        {
            if (_gstEnabled == value) return;
            _gstEnabled = value;
            OnPropertyChanged();
        }
    }

    public string GstRatePercent
    {
        get => _gstRatePercent;
        set
        {
            if (_gstRatePercent == value) return;
            _gstRatePercent = value;
            OnPropertyChanged();
        }
    }

    public bool FuelSurchargeEnabled
    {
        get => _fuelSurchargeEnabled;
        set
        {
            if (_fuelSurchargeEnabled == value) return;
            _fuelSurchargeEnabled = value;
            OnPropertyChanged();
        }
    }

    public string FuelSurchargePercent
    {
        get => _fuelSurchargePercent;
        set
        {
            if (_fuelSurchargePercent == value) return;
            _fuelSurchargePercent = value;
            OnPropertyChanged();
        }
    }

    public string InvoicePrefix
    {
        get => _invoicePrefix;
        set
        {
            if (_invoicePrefix == value) return;
            _invoicePrefix = value;
            OnPropertyChanged();
        }
    }

    public string PodPrefix
    {
        get => _podPrefix;
        set
        {
            if (_podPrefix == value) return;
            _podPrefix = value;
            OnPropertyChanged();
        }
    }

    public int PaymentTermsDays
    {
        get => _paymentTermsDays;
        set
        {
            if (_paymentTermsDays == value) return;
            _paymentTermsDays = value;
            OnPropertyChanged();
        }
    }

    public bool ShowDamageNotesOnPod
    {
        get => _showDamageNotesOnPod;
        set
        {
            if (_showDamageNotesOnPod == value) return;
            _showDamageNotesOnPod = value;
            OnPropertyChanged();
        }
    }

    public bool ShowWaitTimeOnPod
    {
        get => _showWaitTimeOnPod;
        set
        {
            if (_showWaitTimeOnPod == value) return;
            _showWaitTimeOnPod = value;
            OnPropertyChanged();
        }
    }

    public string WaitTimeCharge
    {
        get => _waitTimeCharge;
        set
        {
            if (_waitTimeCharge == value) return;
            _waitTimeCharge = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsWaitTimeInvalid));
        }
    }
    public bool WaitTimeChargeEnabled
    {
        get => _waitTimeChargeEnabled;
        set
        {
            if (_waitTimeChargeEnabled == value) return;
            _waitTimeChargeEnabled = value;
            OnPropertyChanged();
        }
    }

    public string HandUnloadCharge
    {
        get => _handUnloadCharge;
        set
        {
            if (_handUnloadCharge == value) return;
            _handUnloadCharge = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsHandUnloadInvalid));
        }
    }
    public bool HandUnloadChargeEnabled
    {
        get => _handUnloadChargeEnabled;
        set
        {
            if (_handUnloadChargeEnabled == value) return;
            _handUnloadChargeEnabled = value;
            OnPropertyChanged();
        }
    }

    public string BusinessAddress1
    {
        get => _businessAddress1;
        set
        {
            if (_businessAddress1 == value) return;
            _businessAddress1 = value;
            OnPropertyChanged();
        }
    }

    public string BusinessSuburb
    {
        get => _businessSuburb;
        set
        {
            if (_businessSuburb == value) return;
            _businessSuburb = value;
            OnPropertyChanged();
        }
    }

    public string BusinessCity
    {
        get => _businessCity;
        set
        {
            if (_businessCity == value) return;
            _businessCity = value;
            OnPropertyChanged();
        }
    }

    public string BusinessRegion
    {
        get => _businessRegion;
        set
        {
            if (_businessRegion == value) return;
            _businessRegion = value;
            OnPropertyChanged();
        }
    }

    public string BusinessPostcode
    {
        get => _businessPostcode;
        set
        {
            if (_businessPostcode == value) return;
            _businessPostcode = value;
            OnPropertyChanged();
        }
    }

    public string BusinessCountry
    {
        get => _businessCountry;
        set
        {
            if (_businessCountry == value) return;
            _businessCountry = value;
            OnPropertyChanged();
        }
    }

    public string SupplierGstNumber
    {
        get => _supplierGstNumber;
        set
        {
            if (_supplierGstNumber == value) return;
            _supplierGstNumber = value;
            OnPropertyChanged();
        }
    }

    public string SupplierNzbn
    {
        get => _supplierNzbn;
        set
        {
            if (_supplierNzbn == value) return;
            _supplierNzbn = value;
            OnPropertyChanged();
        }
    }

    public string BankAccountNumber
    {
        get => _bankAccountNumber;
        set
        {
            if (_bankAccountNumber == value) return;
            _bankAccountNumber = value;
            OnPropertyChanged();
        }
    }
    #endregion

    #region Validation

    public bool IsWaitTimeInvalid =>
        !string.IsNullOrWhiteSpace(WaitTimeCharge) &&
        !decimal.TryParse(WaitTimeCharge, out _);

    public bool IsHandUnloadInvalid =>
        !string.IsNullOrWhiteSpace(HandUnloadCharge) &&
        !decimal.TryParse(HandUnloadCharge, out _);

    #endregion

    #region Commands

    public ICommand SaveCommand { get; }

    #endregion

    #region Constructors

    public SettingsViewModel(DocumentSettingsApiService settingsApiService)
    {
        _settingsApiService = settingsApiService;

        InvoiceSection = new SettingsSectionViewModel("Invoice Settings");
        PodSection = new SettingsSectionViewModel("POD Settings");
        BusinessTaxSection = new SettingsSectionViewModel("Business & Tax Details");

        InvoiceSection.ExpandedChanged += OnSectionExpanded;
        PodSection.ExpandedChanged += OnSectionExpanded;
        BusinessTaxSection.ExpandedChanged += OnSectionExpanded;

        SaveCommand = new Command(async () => await SaveAsync());
        
    }

    #endregion

    public async Task LoadAsync()
    {
        try
        {
            // ----------------------------
            // Document Settings
            // ----------------------------
            var settings = await _settingsApiService.GetAsync();

            if (settings != null)
            {
                GstEnabled = settings.GstEnabled;
                GstRatePercent = settings.GstRatePercent.ToString("0.##");

                FuelSurchargeEnabled = settings.FuelSurchargeEnabled;
                FuelSurchargePercent = settings.FuelSurchargePercent.ToString("0.##");

                InvoicePrefix = settings.InvoicePrefix;
                PodPrefix = settings.PodPrefix;
                PaymentTermsDays = settings.PaymentTermsDays;
                ShowDamageNotesOnPod = settings.ShowDamageNotesOnPod;
                ShowWaitTimeOnPod = settings.ShowWaitTimeOnPod;

                WaitTimeCharge = settings.WaitTimeCharge.ToString("0.##");
                HandUnloadCharge = settings.HandUnloadCharge.ToString("0.##");
                WaitTimeChargeEnabled = settings.WaitTimeChargeEnabled;
                HandUnloadChargeEnabled = settings.HandUnloadChargeEnabled;

                BusinessAddress1 = settings.BusinessAddress1 ?? string.Empty;
                BusinessSuburb = settings.BusinessSuburb ?? string.Empty;
                BusinessCity = settings.BusinessCity ?? string.Empty;
                BusinessRegion = settings.BusinessRegion ?? string.Empty;
                BusinessPostcode = settings.BusinessPostcode ?? string.Empty;
                BusinessCountry = settings.BusinessCountry ?? string.Empty;

                SupplierGstNumber = settings.SupplierGstNumber ?? string.Empty;
                SupplierNzbn = settings.SupplierNzbn ?? string.Empty;
                BankAccountNumber = settings.BankAccountNumber ?? string.Empty;
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Settings",
                $"Unable to load settings: {ex.Message}",
                "OK");
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            decimal gstRate = 0m;
            decimal fuelRate = 0m;
            decimal waitTimeCharge = 0m;
            decimal handUnloadCharge = 0m;

            if (GstEnabled && !TryParseDecimal(GstRatePercent, out gstRate))
            {
                await Shell.Current.DisplayAlertAsync("Validation", "Enter a valid GST rate.", "OK");
                return;
            }

            if (FuelSurchargeEnabled && !TryParseDecimal(FuelSurchargePercent, out fuelRate))
            {
                await Shell.Current.DisplayAlertAsync("Validation", "Enter a valid fuel surcharge rate.", "OK");
                return;
            }

            if (WaitTimeChargeEnabled && !TryParseDecimal(WaitTimeCharge, out waitTimeCharge))
            {
                await Shell.Current.DisplayAlertAsync("Validation", "Enter a valid wait time charge.", "OK");
                return;
            }

            if (HandUnloadChargeEnabled && !TryParseDecimal(HandUnloadCharge, out handUnloadCharge))
            {
                await Shell.Current.DisplayAlertAsync("Validation", "Enter a valid hand unload charge.", "OK");
                return;
            }

            var documentSettingsRequest = new UpdateDocumentSettingsRequest
            {
                GstEnabled = GstEnabled,
                GstRatePercent = gstRate,

                FuelSurchargeEnabled = FuelSurchargeEnabled,
                FuelSurchargePercent = fuelRate,

                WaitTimeChargeEnabled = WaitTimeChargeEnabled,
                WaitTimeCharge = waitTimeCharge,

                HandUnloadChargeEnabled = HandUnloadChargeEnabled,
                HandUnloadCharge = handUnloadCharge,

                InvoicePrefix = InvoicePrefix,
                PodPrefix = PodPrefix,
                PaymentTermsDays = PaymentTermsDays,
                ShowDamageNotesOnPod = ShowDamageNotesOnPod,
                ShowWaitTimeOnPod = ShowWaitTimeOnPod,

                BusinessAddress1 = string.IsNullOrWhiteSpace(BusinessAddress1) ? null : BusinessAddress1.Trim(),
                BusinessSuburb = string.IsNullOrWhiteSpace(BusinessSuburb) ? null : BusinessSuburb.Trim(),
                BusinessCity = string.IsNullOrWhiteSpace(BusinessCity) ? null : BusinessCity.Trim(),
                BusinessRegion = string.IsNullOrWhiteSpace(BusinessRegion) ? null : BusinessRegion.Trim(),
                BusinessPostcode = string.IsNullOrWhiteSpace(BusinessPostcode) ? null : BusinessPostcode.Trim(),
                BusinessCountry = string.IsNullOrWhiteSpace(BusinessCountry) ? null : BusinessCountry.Trim(),
                SupplierGstNumber = string.IsNullOrWhiteSpace(SupplierGstNumber) ? null : SupplierGstNumber.Trim(),
                SupplierNzbn = string.IsNullOrWhiteSpace(SupplierNzbn) ? null : SupplierNzbn.Trim(),
                BankAccountNumber = string.IsNullOrWhiteSpace(BankAccountNumber)
                    ? null
                    : BankAccountNumber.Replace(" ", "").Trim()
            };

            var settingsSaved = await _settingsApiService.SaveAsync(documentSettingsRequest);
            System.Diagnostics.Debug.WriteLine(
    $"[Mobile Save Request] GST={documentSettingsRequest.GstEnabled}, GST Rate={documentSettingsRequest.GstRatePercent}, Fuel={documentSettingsRequest.FuelSurchargeEnabled}, Fuel Rate={documentSettingsRequest.FuelSurchargePercent}");
            if (settingsSaved)
            {
                await Shell.Current.DisplayAlertAsync("Saved", "Settings updated.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Save failed", "Settings could not be saved.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Save failed", ex.Message, "OK");
        }
    }

    private static bool TryParseDecimal(string? value, out decimal result)
    {
        result = 0m;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        var cleaned = value
            .Trim()
            .Replace("%", "")
            .Replace("$", "")
            .Replace("NZD", "", StringComparison.OrdinalIgnoreCase)
            .Replace(" ", "");

        return decimal.TryParse(
                   cleaned,
                   NumberStyles.Number,
                   CultureInfo.CurrentCulture,
                   out result)
               ||
               decimal.TryParse(
                   cleaned,
                   NumberStyles.Number,
                   CultureInfo.InvariantCulture,
                   out result);
    }

    private void OnSectionExpanded(SettingsSectionViewModel sender, bool isExpanded)
    {
        if (!isExpanded)
            return;

        if (sender != InvoiceSection) InvoiceSection.IsExpanded = false;
        if (sender != PodSection) PodSection.IsExpanded = false;
        if (sender != BusinessTaxSection) BusinessTaxSection.IsExpanded = false;
    }
}