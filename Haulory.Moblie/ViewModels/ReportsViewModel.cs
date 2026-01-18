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

    #region Load
    public async Task LoadAsync()
    {
        if (_isLoading) return;
        _isLoading = true;

        try
        {
            Receipts.Clear();

            var all = await _receiptRepository.GetAllAsync();

            // newest first (or change to whatever you want)
            foreach (var r in all.OrderByDescending(x => x.DeliveredAtUtc))
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
}
