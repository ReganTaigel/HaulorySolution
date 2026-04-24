using HaulitCore.Mobile.ViewModels;

namespace HaulitCore.Mobile.Views;

public partial class NewJobPage : ContentPage, IQueryAttributable
{
    private readonly NewJobViewModel _viewModel;

    public NewJobPage(NewJobViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("jobId", out var raw))
        {
            if (raw is Guid guid)
            {
                _viewModel.SetEditingJobId(guid);
                return;
            }

            if (raw is string text && Guid.TryParse(Uri.UnescapeDataString(text), out var parsed))
            {
                _viewModel.SetEditingJobId(parsed);
                return;
            }
        }

        _viewModel.SetEditingJobId(null);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}