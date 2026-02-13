using System.Text.Json;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Haulory.Infrastructure.Storage;

namespace Haulory.Infrastructure.Persistence.Json;

public class JobRepository : IJobRepository
{
    private readonly string _filePath;
    private static readonly SemaphoreSlim _lock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public JobRepository()
    {
        _filePath = Path.Combine(FileSystem.AppDataDirectory, "jobs.json.enc");
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
        finally { _lock.Release(); }
    }

    public async Task<IReadOnlyList<Job>> GetAllAsync()
    {
        await _lock.WaitAsync();
        try { return await LoadAsync(); }
        finally { _lock.Release(); }
    }

    public async Task<Job?> GetByIdAsync(Guid id)
    {
        var jobs = await LoadAsync();
        return jobs.FirstOrDefault(j => j.Id == id);
    }

    public async Task UpdateAsync(Job job)
    {
        await _lock.WaitAsync();
        try
        {
            var jobs = await LoadAsync();
            var index = jobs.FindIndex(j => j.Id == job.Id);
            if (index < 0) return;

            jobs[index] = job;
            await SaveAsync(jobs);
        }
        finally { _lock.Release(); }
    }

    public async Task DeleteAsync(Guid id)
    {
        await _lock.WaitAsync();
        try
        {
            var jobs = await LoadAsync();
            var removed = jobs.RemoveAll(j => j.Id == id);
            if (removed > 0)
                await SaveAsync(jobs);
        }
        finally { _lock.Release(); }
    }
    public async Task<int> GetNextSortOrderAsync()
    {
        await _lock.WaitAsync();
        try
        {
            var jobs = await LoadAsync();
            return jobs.Count == 0 ? 1 : jobs.Max(j => j.SortOrder) + 1;
        }
        finally { _lock.Release(); }
    }

    public async Task UpdateAllAsync(IReadOnlyList<Job> jobs)
    {
        await _lock.WaitAsync();
        try
        {
            await SaveAsync(jobs.ToList());
        }
        finally { _lock.Release(); }
    }

    private async Task<List<Job>> LoadAsync()
    {
        var data = await EncryptedJsonStore.LoadAsync<List<Job>>(_filePath, JsonOptions);
        return data ?? new List<Job>();
    }

    private async Task SaveAsync(List<Job> jobs)
    {
        await EncryptedJsonStore.SaveAsync(_filePath, jobs, JsonOptions);
    }
}
