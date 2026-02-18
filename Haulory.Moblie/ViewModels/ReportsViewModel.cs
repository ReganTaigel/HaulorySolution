using System.Collections.ObjectModel;
using System.Windows.Input;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;

namespace Haulory.Mobile.ViewModels;

// Reporting dashboard for Delivery Receipts.
// Loads receipts and filters them by a selected LOCAL date (receipts are stored in UTC).
public class ReportsViewModel : BaseViewModel
{
    #region Dependencies

    private readonly IDeliveryReceiptRepository _receiptRepository;

    #endregion

    #region State

    // Prevents concurrent loads (e.g. user taps refresh repeatedly / date changes fast).
    private bool _isLoading;

    // Selected LOCAL date used for filtering (date-only semantics).
    private DateTime _selectedDate = DateTime.Today;

    #endregion

    #region Collections

    // Receipts shown in the report list after filtering by SelectedDate.
    public ObservableCollection<DeliveryReceipt> Receipts { get; } = new();

    #endregion

    #region Computed Stats

    // Number of delivered receipts for the selected date.
    public int DeliveredCount => Receipts.Count;

    // Total revenue across filtered receipts.
    public decimal TotalRevenue => Receipts.Sum(r => r.Total);

    #endregion

    #region Commands

    public ICommand RefreshCommand { get; }

    #endregion

    #region Constructor

    public ReportsViewModel(IDeliveryReceiptRepository receiptRepository)
    {
        _receiptRepository = receiptRepository;

        // Manual refresh (pull-to-refresh / button).
        RefreshCommand = new Command(async () => await LoadAsync());
    }

    #endregion

    #region Date Filter

    // Filter date (LOCAL). When the date changes we reload the list automatically.
    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            // Date-only comparison avoids reload when time component differs.
            if (_selectedDate.Date == value.Date) return;

            _selectedDate = value.Date;
            OnPropertyChanged();

            // Fire-and-forget refresh to keep UI responsive.
            // (Note: any exceptions inside LoadAsync would surface on the sync context; consider try/catch if needed.)
            _ = LoadAsync();
        }
    }

    #endregion

    #region Load

    // Loads receipts, filters them by SelectedDate (local date), and updates computed stats.
    public async Task LoadAsync()
    {
        if (_isLoading) return;
        _isLoading = true;

        try
        {
            Receipts.Clear();

            // NOTE: if this becomes large, consider adding a repository query for date range
            // rather than pulling everything into memory.
            var all = await _receiptRepository.GetAllAsync();

            // Filter by selected LOCAL date (DeliveredAtUtc stored in UTC)
            var filtered = all
                .Where(r => ToLocalDate(r.DeliveredAtUtc) == SelectedDate.Date)
                .OrderByDescending(r => r.DeliveredAtUtc);

            foreach (var r in filtered)
                Receipts.Add(r);

            // Update summary cards / labels.
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

    // Converts a stored UTC datetime into a LOCAL date for consistent "day" filtering.
    // Handles cases where a DateTime might not be explicitly marked as UTC.
    private static DateTime ToLocalDate(DateTime utc)
    {
        var safeUtc = utc.Kind == DateTimeKind.Utc
            ? utc
            : DateTime.SpecifyKind(utc, DateTimeKind.Utc);

        return safeUtc.ToLocalTime().Date;
    }

    #endregion
}
