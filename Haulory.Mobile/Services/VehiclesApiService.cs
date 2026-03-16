using Haulory.Application.Interfaces.Services;
using Haulory.Contracts.Vehicles;

using System.Net.Http.Json;

namespace Haulory.Mobile.Services;

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

        System.Diagnostics.Debug.WriteLine($"[VehiclesApi] GET api/vehicles status={(int)response.StatusCode}");
        System.Diagnostics.Debug.WriteLine("[VehiclesApi] Raw JSON:");
        System.Diagnostics.Debug.WriteLine(body);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Get vehicles failed. Status: {(int)response.StatusCode}. Body: {body}");

        var result = await response.Content.ReadFromJsonAsync<List<VehicleDto>>(cancellationToken: cancellationToken);

        System.Diagnostics.Debug.WriteLine($"[VehiclesApi] Deserialized count = {result?.Count ?? 0}");

        return result ?? new List<VehicleDto>();
    }

    public async Task<IReadOnlyList<VehicleDto>> GetTrailersAsync(CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync();

        var response = await HttpClient.GetAsync("api/vehicles/trailers", cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        System.Diagnostics.Debug.WriteLine($"[VehiclesApi] GET api/vehicles/trailers status={(int)response.StatusCode}");
        System.Diagnostics.Debug.WriteLine("[VehiclesApi] Raw JSON trailers:");
        System.Diagnostics.Debug.WriteLine(body);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Get trailers failed. Status: {(int)response.StatusCode}. Body: {body}");

        var result = await response.Content.ReadFromJsonAsync<List<VehicleDto>>(cancellationToken: cancellationToken);

        System.Diagnostics.Debug.WriteLine($"[VehiclesApi] Deserialized trailer count = {result?.Count ?? 0}");

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

}