using System.Net.Http.Headers;
using System.Net.Http.Json;
using Haulory.Application.Interfaces.Services;
using Haulory.Mobile.Contracts.Drivers;

namespace Haulory.Mobile.Services;

public sealed class DriversApiService
{
    private readonly HttpClient _httpClient;
    private readonly ISessionService _sessionService;

    public DriversApiService(HttpClient httpClient, ISessionService sessionService)
    {
        _httpClient = httpClient;
        _sessionService = sessionService;
    }

    private async Task EnsureAuthenticatedAsync()
    {
        if (!_sessionService.IsAuthenticated)
            await _sessionService.RestoreAsync();

        var token = _sessionService.JwtToken;

        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("User is not authenticated.");

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<IReadOnlyList<DriverDto>> GetDriversAsync(CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync();

        var result = await _httpClient.GetFromJsonAsync<List<DriverDto>>(
            "api/drivers",
            cancellationToken);

        return result ?? new List<DriverDto>();
    }

    public async Task<DriverDto?> GetDriverByIdAsync(Guid driverId, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync();

        return await _httpClient.GetFromJsonAsync<DriverDto>(
            $"api/drivers/{driverId}",
            cancellationToken);
    }

    public async Task<DriverDto> UpdateDriverAsync(Guid driverId, UpdateDriverRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync();

        var response = await _httpClient.PutAsJsonAsync(
            $"api/drivers/{driverId}",
            request,
            cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Failed to update driver. Status: {(int)response.StatusCode}. Body: {responseBody}");
        }

        var driver = await response.Content.ReadFromJsonAsync<DriverDto>(cancellationToken: cancellationToken);

        if (driver is null)
            throw new InvalidOperationException("API returned an empty driver response.");

        return driver;
    }

    public async Task<DriverDto> CreateDriverAsync(CreateDriverRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync();

        var response = await _httpClient.PostAsJsonAsync(
            "api/drivers",
            request,
            cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Failed to create driver. Status: {(int)response.StatusCode}. Body: {responseBody}");
        }

        var driver = await response.Content.ReadFromJsonAsync<DriverDto>(cancellationToken: cancellationToken);

        if (driver is null)
            throw new InvalidOperationException("API returned an empty driver response.");

        return driver;
    }
}