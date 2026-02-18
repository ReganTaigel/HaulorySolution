using Haulory.Domain.Entities;

namespace Haulory.Application.Interfaces.Repositories;

#region Interface: Vehicle Asset Repository

public interface IVehicleAssetRepository
{
    #region Create

    // Adds a single vehicle asset
    Task AddAsync(VehicleAsset asset);

    // Adds multiple vehicle assets (used for vehicle set creation)
    Task AddRangeAsync(IReadOnlyList<VehicleAsset> assets);

    #endregion

    #region Queries

    // Retrieves all vehicle assets
    // Typically filtered by owner in infrastructure layer
    Task<IReadOnlyList<VehicleAsset>> GetAllAsync();

    // Retrieves a specific vehicle asset by Id
    Task<VehicleAsset?> GetByIdAsync(Guid id);

    #endregion

    #region Update

    // Updates an existing vehicle asset
    Task UpdateAsync(VehicleAsset asset);

    #endregion

    #region Lifecycle

    // Deletes a vehicle asset by Id
    Task DeleteAsync(Guid id);

    #endregion
}

#endregion
