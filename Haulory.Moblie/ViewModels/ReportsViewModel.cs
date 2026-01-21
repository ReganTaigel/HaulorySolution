using System.Collections.ObjectModel;
using System.Windows.Input;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;

namespace Haulory.Moblie.ViewModels;

public class ReportsViewModel : BaseViewModel
{
    #region Fields
    private readonly IDeliveryReceiptRepository _receiptRepository;
    private bool _isLoading;
    private DateTime _selectedDate = DateTime.Today;
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

    #region Date Filter

    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (_selectedDate.Date == value.Date) return;

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

            // Filter by selected LOCAL date (DeliveredAtUtc stored in UTC)
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
