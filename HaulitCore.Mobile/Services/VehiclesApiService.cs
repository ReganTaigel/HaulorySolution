using HaulitCore.Application.Interfaces.Services;
using HaulitCore.Contracts.Vehicles;

using System.Net.Http.Json;

namespace HaulitCore.Mobile.Services;

public sealed class VehiclesApiService : ApiServiceBase
{
    public VehiclesApiService(HttpClient httpClient, ISessionService sessionService)
        : base(httpClient, sessionService)
    {
    }

    public async Task<IReadOnlyList<VehicleDto>> GetVehiclesAsync(CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync();

        var response = await HttpClient.GetAsync("api/vehicles", cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<List<VehicleDto>>(cancellationToken: cancellationToken);

        return result ?? new List<VehicleDto>();
    }

    public async Task<IReadOnlyList<VehicleDto>> GetTrailersAsync(CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync();

        var response = await HttpClient.GetAsync("api/vehicles/trailers", cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<List<VehicleDto>>(cancellationToken: cancellationToken);

        return result ?? new List<VehicleDto>();
    }

    public async Task<VehicleDto?> GetVehicleByIdAsync(Guid vehicleId, CancellationToken cancellationToken = default)
        => await GetAsync<VehicleDto>($"api/vehicles/{vehicleId}", cancellationToken);

    public Task<CreateVehicleSetResponse> CreateVehicleSetAsync(
        CreateVehicleSetRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<CreateVehicleSetResponse>("api/vehicles/sets", request, cancellationToken);

    public Task<VehicleDto> UpdateVehicleAsync(
        Guid vehicleId,
        UpdateVehicleRequest request,
        CancellationToken cancellationToken = default)
        => PutAsync<VehicleDto>($"api/vehicles/{vehicleId}", request, cancellationToken);


    public Task DeleteVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    => DeleteAsync($"api/vehicles/{vehicleId}", cancellationToken);

    public async Task<IReadOnlyList<VehicleDto>> GetVehicleSetByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync();
        return await GetAsync<List<VehicleDto>>($"api/vehicles/{vehicleId}/set", cancellationToken)
               ?? new List<VehicleDto>();
    }

    public Task UpdateVehicleSetAsync(
        Guid vehicleId,
        UpdateVehicleSetRequest request,
        CancellationToken cancellationToken = default)
        => PutAsync<object>($"api/vehicles/{vehicleId}/set", request, cancellationToken);

}
