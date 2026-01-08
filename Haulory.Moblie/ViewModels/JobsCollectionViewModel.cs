using Haulory.Moblie.ViewModels;
using Haulory.Moblie.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

public class JobsCollectionViewModel : BaseViewModel
{
   // public ObservableCollection<Job> Jobs { get; } = new();

    public ICommand AddJobCommand { get; }

    public JobsCollectionViewModel()
    {
        AddJobCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync(nameof(NewJobPage));
        });
    }
}
