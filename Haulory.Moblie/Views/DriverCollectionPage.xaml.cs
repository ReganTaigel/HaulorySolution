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
        await _vm.LoadAsync();
    }

    private async void OnDriverSelected(object sender, SelectionChangedEventArgs e)
    {
        // Items are DriverListItem now
        var selectedItem = e.CurrentSelection?.FirstOrDefault() as DriverListItem;
        var driver = selectedItem?.Driver;
        if (driver == null) return;

        // clear selection so it can be tapped again
        ((CollectionView)sender).SelectedItem = null;

        // edit main OR sub
        await Shell.Current.GoToAsync($"{nameof(EditDriverPage)}?driverId={driver.Id}");
    }
}
