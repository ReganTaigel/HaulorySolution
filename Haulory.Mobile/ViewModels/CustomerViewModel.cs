using Haulory.Contracts.Customers;
using Haulory.Mobile.Features;
using Haulory.Mobile.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Haulory.Mobile.ViewModels;

public sealed class CustomerViewModel : BaseViewModel
{
    private readonly CustomersApiService _customersApiService;
    private string _searchText = string.Empty;
    private bool _isBusy;

    public ObservableCollection<CustomerDto> Customers { get; } = new();

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public ICommand LoadCommand { get; }
    public ICommand SearchCommand { get; }

    public CustomerViewModel(
        CustomersApiService customersApiService,
        IFeatureAccessService featureAccessService)
        : base(featureAccessService)
    {
        _customersApiService = customersApiService;
        LoadCommand = new Command(async () => await LoadAsync());
        SearchCommand = new Command(async () => await LoadAsync(SearchText));
    }

    public async Task LoadAsync(string? search = null)
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            Customers.Clear();

            var items = await _customersApiService.GetCustomersAsync(search);
            foreach (var item in items.OrderBy(x => x.CompanyName).ThenBy(x => x.ContactName))
                Customers.Add(item);
        }
        finally
        {
            IsBusy = false;
        }
    }
}