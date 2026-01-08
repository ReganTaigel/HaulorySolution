namespace Haulory.Moblie.Views;

public partial class JobsCollectionPage : ContentPage
{
	public JobsCollectionPage(JobsCollectionViewModel vm)
	{
        InitializeComponent();
        BindingContext = vm;
    }
}