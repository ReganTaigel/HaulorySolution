using System.Text.Json;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;

namespace Haulory.Infrastructure.Persistence.Json;

public class JobRepository : IJobRepository
{
    private readonly string _filePath;
    private static readonly SemaphoreSlim _lock = new(1, 1);

    public JobRepository()
    {
        _filePath = Path.Combine(
            FileSystem.AppDataDirectory,
            "jobs.json");
    }

    public async Task AddAsync(Job job)
    {
        await _lock.WaitAsync();
        try
        {
            var jobs = await LoadAsync();
            jobs.Add(job);
            await SaveAsync(jobs);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IReadOnlyList<Job>> GetAllAsync()
    {
        return await LoadAsync();
    }

    // Determine next manual sort position
    public async Task<int> GetNextSortOrderAsync()
    {
        var jobs = await LoadAsync();

        return jobs.Count == 0
            ? 1
            : jobs.Max(j => j.SortOrder) + 1;
    }

    // Persist reordered jobs
    public async Task UpdateAllAsync(IReadOnlyList<Job> jobs)
    {
        await _lock.WaitAsync();
        try
        {
            await SaveAsync(jobs.ToList());
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<List<Job>> LoadAsync()
    {
        if (!File.Exists(_filePath))
            return new List<Job>();

        var json = await File.ReadAllTextAsync(_filePath);

        return JsonSerializer.Deserialize<List<Job>>(json)
               ?? new List<Job>();
    }

    private async Task SaveAsync(List<Job> jobs)
    {
        var json = JsonSerializer.Serialize(
            jobs,
            new JsonSerializerOptions { WriteIndented = true });

        await File.WriteAllTextAsync(_filePath, json);
    }
}
