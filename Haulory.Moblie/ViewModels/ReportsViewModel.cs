using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Haulory.Application.Features.Reports;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Domain.Entities;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;

namespace Haulory.Mobile.ViewModels;

[QueryProperty(nameof(FocusJobId), "jobId")]
public class ReportsViewModel : BaseViewModel
{
    #region Dependencies

    private readonly IDeliveryReceiptRepository _receiptRepository;
    private readonly ISessionService _session;

    private readonly InvoiceReportHandler _invoiceReport;
    private readonly IPdfInvoiceGenerator _pdfInvoice;

    private readonly PodReportHandler _podReport;
    private readonly IPdfPodGenerator _pdfPod;

    #endregion

    #region State

    private bool _isLoading;
    private DateTime _selectedDate = DateTime.Today;
    private Guid? _focusJobId;

    private bool _includeGst = true;

    #endregion

    #region Collections

    public ObservableCollection<DeliveryReceipt> Receipts { get; } = new();

    #endregion

    #region Computed Stats

    public int DeliveredCount => Receipts.Count;
    public decimal TotalRevenue => Receipts.Sum(r => r.Total);

    #endregion

    #region Options

    public bool IncludeGst
    {
        get => _includeGst;
        set
        {
            if (_includeGst == value) return;
            _includeGst = value;
            OnPropertyChanged();
        }
    }

    // v1 default
    public decimal GstRate { get; set; } = 0.15m;

    #endregion

    #region Commands

    public ICommand RefreshCommand { get; }
    public ICommand ExportInvoiceCommand { get; }
    public ICommand ExportPodCommand { get; }

    #endregion

    #region Ctor

    public ReportsViewModel(
        IDeliveryReceiptRepository receiptRepository,
        ISessionService session,
        InvoiceReportHandler invoiceReport,
        IPdfInvoiceGenerator pdfInvoice,
        PodReportHandler podReport,
        IPdfPodGenerator pdfPod)
    {
        _receiptRepository = receiptRepository;
        _session = session;

        _invoiceReport = invoiceReport;
        _pdfInvoice = pdfInvoice;

        _podReport = podReport;
        _pdfPod = pdfPod;

        RefreshCommand = new Command(async () => await LoadAsync());
        ExportInvoiceCommand = new Command<Guid>(async (receiptId) => await ExportInvoiceAsync(receiptId));
        ExportPodCommand = new Command<Guid>(async (receiptId) => await ExportPodAsync(receiptId));
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
        if (_isLoading) return;
        _isLoading = true;

        try
        {
            Receipts.Clear();

            var ownerUserId = _session.CurrentOwnerId ?? Guid.Empty;
            if (ownerUserId == Guid.Empty)
                return;

            // Local day -> UTC range [start, end)
            var localStart = SelectedDate.Date;
            var localEnd = localStart.AddDays(1);

            var utcStart = DateTime.SpecifyKind(localStart, DateTimeKind.Local).ToUniversalTime();
            var utcEnd = DateTime.SpecifyKind(localEnd, DateTimeKind.Local).ToUniversalTime();

            var filtered = await _receiptRepository
                .GetByOwnerDeliveredBetweenUtcAsync(ownerUserId, utcStart, utcEnd);

            foreach (var r in filtered.OrderByDescending(r => r.DeliveredAtUtc))
                Receipts.Add(r);

            OnPropertyChanged(nameof(DeliveredCount));
            OnPropertyChanged(nameof(TotalRevenue));
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task JumpToReceiptDateAndReloadAsync(Guid jobId)
    {
        var ownerUserId = _session.CurrentOwnerId ?? Guid.Empty;
        if (ownerUserId == Guid.Empty)
            return;

        var receipts = await _receiptRepository.GetByJobIdAsync(ownerUserId, jobId);
        var receipt = receipts.FirstOrDefault();

        if (receipt == null)
        {
            await LoadAsync();
            return;
        }

        // Set without causing double-load
        _selectedDate = ToLocalDate(receipt.DeliveredAtUtc);
        OnPropertyChanged(nameof(SelectedDate));

        await LoadAsync();
    }

    #endregion

    #region Export Invoice

    private async Task ExportInvoiceAsync(Guid receiptId)
    {
        try
        {
            var ownerUserId = _session.CurrentOwnerId ?? Guid.Empty;
            if (ownerUserId == Guid.Empty)
                return;

            var dto = await _invoiceReport.HandleAsync(ownerUserId, receiptId, IncludeGst, GstRate);

            // Signature optional (you can wire this later if you want invoice signature)
            var pdfBytes = _pdfInvoice.GenerateInvoicePdf(dto, Array.Empty<byte>());

            var safeInvoice = string.IsNullOrWhiteSpace(dto.InvoiceNumber) ? "invoice" : dto.InvoiceNumber.Trim();
            var filename = $"Invoice_{safeInvoice}_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
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
        try
        {
            var ownerUserId = _session.CurrentOwnerId ?? Guid.Empty;
            if (ownerUserId == Guid.Empty)
                return;

            var dto = await _podReport.HandleAsync(ownerUserId, receiptId);
            var pdfBytes = _pdfPod.GeneratePodPdf(dto);

            var safeRef = string.IsNullOrWhiteSpace(dto.ReferenceNumber) ? "pod" : dto.ReferenceNumber.Trim();
            var filename = $"POD_{safeRef}_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
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

    private static DateTime ToLocalDate(DateTime utc)
    {
        var safeUtc = utc.Kind == DateTimeKind.Utc
            ? utc
            : DateTime.SpecifyKind(utc, DateTimeKind.Utc);

        return safeUtc.ToLocalTime().Date;
    }

    #endregion
}