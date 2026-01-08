using Haulory.Domain.Entities;

namespace Haulory.Application.Interfaces.Repositories;

public interface IJobRepository
{
    Task AddAsync(Job job);
    Task<IReadOnlyList<Job>> GetAllAsync();
}
