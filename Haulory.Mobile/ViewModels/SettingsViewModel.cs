using Haulory.Contracts.Settings;
using Haulory.Mobile.Services;
using System.Windows.Input;

namespace Haulory.Mobile.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private readonly DocumentSettingsApiService _settingsApiService;

    public bool GstEnabled { get; set; }
    public decimal GstRatePercent { get; set; }
    public bool FuelSurchargeEnabled { get; set; }
    public decimal FuelSurchargePercent { get; set; }
    public string InvoicePrefix { get; set; } = "INV";
    public string PodPrefix { get; set; } = "POD";
    public int PaymentTermsDays { get; set; } = 7;
    public bool ShowDamageNotesOnPod { get; set; }
    public bool ShowWaitTimeOnPod { get; set; }


    public ICommand SaveCommand { get; }

    public SettingsViewModel(DocumentSettingsApiService settingsApiService)
    {
        _settingsApiService = settingsApiService;
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
            GstRatePercent = settings.GstRatePercent;
            FuelSurchargeEnabled = settings.FuelSurchargeEnabled;
            FuelSurchargePercent = settings.FuelSurchargePercent;
            InvoicePrefix = settings.InvoicePrefix;
            PodPrefix = settings.PodPrefix;
            PaymentTermsDays = settings.PaymentTermsDays;
            ShowDamageNotesOnPod = settings.ShowDamageNotesOnPod;
            ShowWaitTimeOnPod = settings.ShowWaitTimeOnPod;

            OnPropertyChanged(nameof(GstEnabled));
            OnPropertyChanged(nameof(GstRatePercent));
            OnPropertyChanged(nameof(FuelSurchargeEnabled));
            OnPropertyChanged(nameof(FuelSurchargePercent));
            OnPropertyChanged(nameof(InvoicePrefix));
            OnPropertyChanged(nameof(PodPrefix));
            OnPropertyChanged(nameof(PaymentTermsDays));
            OnPropertyChanged(nameof(ShowDamageNotesOnPod));
            OnPropertyChanged(nameof(ShowWaitTimeOnPod));
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
            var request = new UpdateDocumentSettingsRequest
            {
                GstEnabled = GstEnabled,
                GstRatePercent = GstRatePercent,
                FuelSurchargeEnabled = FuelSurchargeEnabled,
                FuelSurchargePercent = FuelSurchargePercent,
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