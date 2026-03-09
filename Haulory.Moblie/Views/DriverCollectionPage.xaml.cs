using Haulory.Mobile.ViewModels;

namespace Haulory.Mobile.Views;

public partial class DriverCollectionPage : ContentPage
{
    private readonly DriverCollectionViewModel _vm;

    public DriverCollectionPage(DriverCollectionViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await _vm.LoadAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Load failed", ex.Message, "OK");
        }
    }

    private async void OnDriverSelected(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            var selectedItem = e.CurrentSelection?.FirstOrDefault() as DriverListItem;
            if (selectedItem == null || selectedItem.Id == Guid.Empty)
                return;

            ((CollectionView)sender).SelectedItem = null;

            await Shell.Current.GoToAsync($"{nameof(EditDriverPage)}?driverId={selectedItem.Id}");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Navigation failed", ex.Message, "OK");
        }
    }
}