using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Haulory.Moblie.ViewModels;
using Haulory.Moblie.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

public class JobsCollectionViewModel : BaseViewModel
{
    private readonly IJobRepository _jobRepository;

    public ObservableCollection<Job> Jobs { get; } = new();

    public ICommand AddJobCommand { get; }

    public JobsCollectionViewModel(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;

        AddJobCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync(nameof(NewJobPage));
        });
    }

    public async Task LoadAsync()
    {
        Jobs.Clear();

        var jobs = await _jobRepository.GetAllAsync();
        foreach (var job in jobs)
            Jobs.Add(job);
    }
}
