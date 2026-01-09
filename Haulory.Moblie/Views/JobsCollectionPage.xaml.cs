namespace Haulory.Moblie.Views;

public partial class JobsCollectionPage : ContentPage
{
    private readonly JobsCollectionViewModel _vm;

    public JobsCollectionPage(JobsCollectionViewModel vm)
	{
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}