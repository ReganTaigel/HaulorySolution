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
    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }

    public JobsCollectionViewModel(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;

        AddJobCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync(nameof(NewJobPage));
        });

        MoveUpCommand = new Command<Job>(async job => await MoveAsync(job, -1));
        MoveDownCommand = new Command<Job>(async job => await MoveAsync(job, +1));
    }

    public async Task LoadAsync()
    {
        Jobs.Clear();

        var jobs = await _jobRepository.GetAllAsync();

        foreach (var job in jobs.OrderBy(j => j.SortOrder))
            Jobs.Add(job);
    }

    private async Task MoveAsync(Job job, int direction)
    {
        var list = Jobs.ToList();
        var index = list.FindIndex(j => j.Id == job.Id);
        if (index < 0) return;

        var newIndex = index + direction;
        if (newIndex < 0 || newIndex >= list.Count) return;

        // swap in UI list
        (list[index], list[newIndex]) = (list[newIndex], list[index]);

        // re-assign SortOrder sequentially
        for (int i = 0; i < list.Count; i++)
            list[i].SetSortOrder(i + 1);

        // persist
        await _jobRepository.UpdateAllAsync(list);

        // reload UI collection
        Jobs.Clear();
        foreach (var j in list.OrderBy(x => x.SortOrder))
            Jobs.Add(j);
    }

}
