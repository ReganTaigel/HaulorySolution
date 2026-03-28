using Haulory.Contracts.Settings;
using Haulory.Mobile.Services;
using System.Windows.Input;

namespace Haulory.Mobile.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private readonly DocumentSettingsApiService _settingsApiService;

    private bool _gstEnabled;
    private string _gstRatePercent = string.Empty;
    private bool _fuelSurchargeEnabled;
    private string _fuelSurchargePercent = string.Empty;
    private string _invoicePrefix = "INV";
    private string _podPrefix = "POD";
    private int _paymentTermsDays = 7;
    private bool _showDamageNotesOnPod;
    private bool _showWaitTimeOnPod;

    public SettingsSectionViewModel InvoiceSection { get; }
    public SettingsSectionViewModel PodSection { get; }

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

    public ICommand SaveCommand { get; }

    public SettingsViewModel(DocumentSettingsApiService settingsApiService)
    {
        _settingsApiService = settingsApiService;

        InvoiceSection = new SettingsSectionViewModel("Invoice Settings", true);
        PodSection = new SettingsSectionViewModel("POD Settings", false);

        SaveCommand = new Command(async () => await SaveAsync());
    }

    public async Task LoadAsync()
    {
        try
        {
            var settings = await _settingsApiService.GetAsync();
            if (settings == null)
                return;
            GstEnabled = settings.GstEnabled;
            GstRatePercent = settings.GstRatePercent.ToString("0.##");
            OnPropertyChanged(nameof(GstRatePercent));

            FuelSurchargeEnabled = settings.FuelSurchargeEnabled;
            FuelSurchargePercent = settings.FuelSurchargePercent.ToString("0.##");
            OnPropertyChanged(nameof(FuelSurchargePercent));
            InvoicePrefix = settings.InvoicePrefix;
            PodPrefix = settings.PodPrefix;
            PaymentTermsDays = settings.PaymentTermsDays;
            ShowDamageNotesOnPod = settings.ShowDamageNotesOnPod;
            ShowWaitTimeOnPod = settings.ShowWaitTimeOnPod;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Settings", $"Unable to load settings: {ex.Message}", "OK");
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            if (!decimal.TryParse(GstRatePercent, out var gstRate))
            {
                await Shell.Current.DisplayAlertAsync("Validation", "Enter a valid GST rate.", "OK");
                return;
            }

            if (!decimal.TryParse(FuelSurchargePercent, out var fuelRate))
            {
                await Shell.Current.DisplayAlertAsync("Validation", "Enter a valid fuel surcharge rate.", "OK");
                return;
            }

            var request = new UpdateDocumentSettingsRequest
            {
                GstEnabled = GstEnabled,
                GstRatePercent = gstRate,
                FuelSurchargeEnabled = FuelSurchargeEnabled,
                FuelSurchargePercent = fuelRate,
                InvoicePrefix = InvoicePrefix,
                PodPrefix = PodPrefix,
                PaymentTermsDays = PaymentTermsDays,
                ShowDamageNotesOnPod = ShowDamageNotesOnPod,
                ShowWaitTimeOnPod = ShowWaitTimeOnPod,
            };

            var success = await _settingsApiService.SaveAsync(request);

            if (success)
            {
                await Shell.Current.DisplayAlertAsync("Saved", "Settings updated.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync(
                    "Save failed",
                    "Settings endpoint did not accept the request.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Save failed", ex.Message, "OK");
        }
    }
}