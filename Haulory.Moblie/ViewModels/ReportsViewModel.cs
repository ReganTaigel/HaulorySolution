using System.Collections.ObjectModel;
using System.Windows.Input;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;

namespace Haulory.Mobile.ViewModels;

// Reporting dashboard for Delivery Receipts.
// Loads receipts and filters them by a selected LOCAL date (receipts are stored in UTC).
[QueryProperty(nameof(FocusJobId), "jobId")]
public class ReportsViewModel : BaseViewModel
{
    #region Dependencies

    private readonly IDeliveryReceiptRepository _receiptRepository;

    #endregion

    #region State

    private bool _isLoading;

    // Selected LOCAL date used for filtering (date-only semantics).
    private DateTime _selectedDate = DateTime.Today;

    // Optional: used when navigated from a completion flow to focus the day automatically.
    private Guid? _focusJobId;

    #endregion

    #region Collections

    public ObservableCollection<DeliveryReceipt> Receipts { get; } = new();

    #endregion

    #region Computed Stats

    public int DeliveredCount => Receipts.Count;

    public decimal TotalRevenue => Receipts.Sum(r => r.Total);

    #endregion

    #region Commands

    public ICommand RefreshCommand { get; }

    #endregion

    #region Constructor

    public ReportsViewModel(IDeliveryReceiptRepository receiptRepository)
    {
        _receiptRepository = receiptRepository;
        RefreshCommand = new Command(async () => await LoadAsync());
    }

    #endregion

    #region Query Param (jobId)

    // Receives "?jobId=..." from Shell navigation.
    // When set, we auto-jump the report date to the receipt's LOCAL date and reload.
    public string FocusJobId
    {
        get => _focusJobId?.ToString() ?? string.Empty;
        set
        {
            if (!Guid.TryParse(value, out var id))
                return;

            _focusJobId = id;

            // Safe fire-and-forget: LoadAsync has its own guard + try/finally.
            _ = JumpToReceiptDateAndReloadAsync(id);
        }
    }

    private async Task JumpToReceiptDateAndReloadAsync(Guid jobId)
    {
        // Pull receipts (later: repo method GetByJobIdAsync for scalability)
        var all = await _receiptRepository.GetAllAsync();
        var receipt = all.FirstOrDefault(r => r.JobId == jobId);
        if (receipt == null)
        {
            // Fallback: just reload current date view
            await LoadAsync();
            return;
        }

        // Align the report day to the receipt's LOCAL date so it shows immediately
        SelectedDate = ToLocalDate(receipt.DeliveredAtUtc);

        // SelectedDate setter triggers LoadAsync already, but call anyway to guarantee
        await LoadAsync();
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

            var all = await _receiptRepository.GetAllAsync();

            var filtered = all
                .Where(r => ToLocalDate(r.DeliveredAtUtc) == SelectedDate.Date)
                .OrderByDescending(r => r.DeliveredAtUtc);

            foreach (var r in filtered)
                Receipts.Add(r);

            OnPropertyChanged(nameof(DeliveredCount));
            OnPropertyChanged(nameof(TotalRevenue));
        }
        finally
        {
            _isLoading = false;
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