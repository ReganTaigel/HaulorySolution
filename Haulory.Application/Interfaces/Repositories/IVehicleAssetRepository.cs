using Haulory.Domain.Entities;

namespace Haulory.Application.Interfaces.Repositories;

public interface IVehicleAssetRepository
{
    Task AddAsync(VehicleAsset asset);
    Task AddRangeAsync(IReadOnlyList<VehicleAsset> assets);

    Task<IReadOnlyList<VehicleAsset>> GetAllAsync();
    Task<VehicleAsset?> GetByIdAsync(Guid id);

    Task UpdateAsync(VehicleAsset asset);
    Task DeleteAsync(Guid id);
}
