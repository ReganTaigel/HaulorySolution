using Haulory.Domain.Entities;

namespace Haulory.Application.Interfaces.Repositories;

public interface IVehicleDayRunRepository
{
    Task AddAsync(VehicleDayRun run, CancellationToken cancellationToken = default);
    Task<VehicleDayRun?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<VehicleDayRun?> GetLatestByUserAndVehicleAsync(Guid userId, Guid vehicleAssetId, CancellationToken cancellationToken = default);
    Task UpdateAsync(VehicleDayRun run, CancellationToken cancellationToken = default);
}