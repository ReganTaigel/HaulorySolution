using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Haulory.Application.Features.Reports;
using Haulory.Application.Interfaces.Services;
using Haulory.Mobile.Contracts.Reports;
using Haulory.Mobile.Features;
using Haulory.Mobile.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;

namespace Haulory.Mobile.ViewModels;

[QueryProperty(nameof(FocusJobId), "jobId")]
public class ReportsViewModel : BaseViewModel
{
    #region Dependencies

    private readonly ReportsApiService _reportsApiService;
    private readonly ISessionService _session;


    #endregion

    #region State

    private bool _isLoading;
    private DateTime _selectedDate = DateTime.Today;
    private Guid? _focusJobId;
    private bool _includeGst = true;

    #endregion

    #region Collections

    public ObservableCollection<DeliveryReceiptDto> Receipts { get; } = new();

    #endregion

    #region Computed Stats

    public int DeliveredCount => Receipts.Count;
    public decimal TotalRevenue => Receipts.Sum(r => r.Total);

    #endregion

    #region Feature Access

    public bool IsReportsVisible => IsFeatureVisible(AppFeature.Reports);
    public bool IsReportsEnabled => IsFeatureEnabled(AppFeature.Reports);

    public bool IsExportInvoiceVisible => IsFeatureVisible(AppFeature.ExportInvoice);
    public bool IsExportInvoiceEnabled => IsFeatureEnabled(AppFeature.ExportInvoice);

    public bool IsExportPodVisible => IsFeatureVisible(AppFeature.ExportPod);
    public bool IsExportPodEnabled => IsFeatureEnabled(AppFeature.ExportPod);

    #endregion

    #region Options

    public bool IncludeGst
    {
        get => _includeGst;
        set => SetProperty(ref _includeGst, value);
    }

    public decimal GstRate { get; set; } = 0.15m;

    #endregion

    #region Commands

    public ICommand RefreshCommand { get; }
    public ICommand ExportInvoiceCommand { get; }
    public ICommand ExportPodCommand { get; }

    #endregion

    #region Constructor

    public ReportsViewModel(
        ReportsApiService reportsApiService,
        ISessionService session,
        IFeatureAccessService featureAccessService)
        : base(featureAccessService)
    {
        _reportsApiService = reportsApiService;
        _session = session;

        RefreshCommand = new Command(async () => await LoadAsync());
        ExportInvoiceCommand = new Command<Guid>(async receiptId => await ExportInvoiceAsync(receiptId));
        ExportPodCommand = new Command<Guid>(async receiptId => await ExportPodAsync(receiptId));
    }

    #endregion

    #region Query Param

    public string FocusJobId
    {
        get => _focusJobId?.ToString() ?? string.Empty;
        set
        {
            if (!Guid.TryParse(value, out var id))
                return;

            _focusJobId = id;
            _ = JumpToReceiptDateAndReloadAsync(id);
        }
    }

    #endregion

    #region Date Filter

    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (_selectedDate.Date == value.Date)
                return;

            _selectedDate = value.Date;
            OnPropertyChanged();

            _ = LoadAsync();
        }
    }

    #endregion

    #region Load

    public async Task LoadAsync()
    {
        if (_isLoading)
            return;

        _isLoading = true;

        try
        {
            Receipts.Clear();

            if (!IsFeatureEnabled(AppFeature.Reports))
            {
                RaiseSummaryAndFeatureBindings();
                return;
            }

            if (!_session.IsAuthenticated)
                await _session.RestoreAsync();

            var ownerUserId = _session.CurrentOwnerId ?? Guid.Empty;
            if (ownerUserId == Guid.Empty)
            {
                RaiseSummaryAndFeatureBindings();
                return;
            }

            var filtered = await _reportsApiService.GetReceiptsAsync(SelectedDate);

            foreach (var r in filtered.OrderByDescending(r => r.DeliveredAtUtc))
                Receipts.Add(r);

            RaiseSummaryAndFeatureBindings();
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task JumpToReceiptDateAndReloadAsync(Guid jobId)
    {
        if (!IsFeatureEnabled(AppFeature.Reports))
            return;

        // Current API shape is date-based for receipts.
        // For now we just reload the current selected date.
        // Later, if you add GET /api/reports/receipts/by-job/{jobId}
        // this can jump directly to the correct date again.
        await LoadAsync();

        if (_focusJobId == null)
            return;

        var matchingReceipt = Receipts.FirstOrDefault(r => r.JobId == jobId);
        if (matchingReceipt == null)
            return;

        var localDate = ToLocalDate(matchingReceipt.DeliveredAtUtc);

        if (_selectedDate.Date != localDate)
        {
            _selectedDate = localDate;
            OnPropertyChanged(nameof(SelectedDate));
            await LoadAsync();
        }
    }

    #endregion

    #region Export Invoice

    private async Task ExportInvoiceAsync(Guid receiptId)
    {
        if (!await EnsureFeatureEnabledAsync(AppFeature.ExportInvoice))
            return;

        try
        {
            var pdfBytes = await _reportsApiService.ExportInvoicePdfAsync(receiptId, IncludeGst, GstRate);


            var filename = $"Invoice_{receiptId}_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
            var path = System.IO.Path.Combine(FileSystem.CacheDirectory, filename);

            System.IO.File.WriteAllBytes(path, pdfBytes);

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Export invoice",
                File = new ShareFile(path)
            });
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Export failed", ex.Message, "OK");
        }
    }

    #endregion

    #region Export POD

    private async Task ExportPodAsync(Guid receiptId)
    {
        if (!await EnsureFeatureEnabledAsync(AppFeature.ExportPod))
            return;

        try
        {
            var pdfBytes = await _reportsApiService.ExportPodPdfAsync(receiptId);

            var filename = $"POD_{receiptId}_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
            var path = System.IO.Path.Combine(FileSystem.CacheDirectory, filename);

            System.IO.File.WriteAllBytes(path, pdfBytes);

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Export POD",
                File = new ShareFile(path)
            });
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Export failed", ex.Message, "OK");
        }
    }

    #endregion

    #region Helpers

    private void RaiseSummaryAndFeatureBindings()
    {
        OnPropertyChanged(nameof(DeliveredCount));
        OnPropertyChanged(nameof(TotalRevenue));
        RefreshFeatureBindings();
    }

    private void RefreshFeatureBindings()
    {
        OnPropertyChanged(nameof(IsReportsVisible));
        OnPropertyChanged(nameof(IsReportsEnabled));

        OnPropertyChanged(nameof(IsExportInvoiceVisible));
        OnPropertyChanged(nameof(IsExportInvoiceEnabled));

        OnPropertyChanged(nameof(IsExportPodVisible));
        OnPropertyChanged(nameof(IsExportPodEnabled));
    }

    private static DateTime ToLocalDate(DateTime utc)
    {
        var safeUtc = utc.Kind == DateTimeKind.Utc
            ? utc
            : DateTime.SpecifyKind(utc, DateTimeKind.Utc);

        return safeUtc.ToLocalTime().Date;
    }

    #endregion
}