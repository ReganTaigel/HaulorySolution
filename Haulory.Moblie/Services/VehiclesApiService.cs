using Haulory.Application.Interfaces.Services;
using Haulory.Mobile.Contracts.Vehicles;

namespace Haulory.Mobile.Services;

public sealed class VehiclesApiService : ApiServiceBase
{
    public VehiclesApiService(HttpClient httpClient, ISessionService sessionService)
        : base(httpClient, sessionService)
    {
    }

    public async Task<IReadOnlyList<VehicleDto>> GetVehiclesAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<VehicleDto>>("api/vehicles", cancellationToken) ?? new List<VehicleDto>();

    public async Task<IReadOnlyList<VehicleDto>> GetTrailersAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<VehicleDto>>("api/vehicles/trailers", cancellationToken) ?? new List<VehicleDto>();

    public async Task<VehicleDto?> GetVehicleByIdAsync(Guid vehicleId, CancellationToken cancellationToken = default)
        => await GetAsync<VehicleDto>($"api/vehicles/{vehicleId}", cancellationToken);

    public Task<CreateVehicleSetResponse> CreateVehicleSetAsync(
        CreateVehicleSetRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<CreateVehicleSetResponse>("api/vehicles/sets", request, cancellationToken);
}