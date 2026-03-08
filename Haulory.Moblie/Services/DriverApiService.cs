using Haulory.Mobile.Contracts.Drivers;
using System.Net.Http.Json;

namespace Haulory.Mobile.Services;

public sealed class DriversApiService
{
    private readonly HttpClient _httpClient;

    public DriversApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<DriverDto>> GetDriversAsync(CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<List<DriverDto>>(
            "api/drivers",
            cancellationToken);

        return result ?? new List<DriverDto>();
    }

    public async Task<DriverDto?> GetDriverByIdAsync(Guid driverId, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<DriverDto>(
            $"api/drivers/{driverId}",
            cancellationToken);
    }

    public async Task<DriverDto> CreateDriverAsync(CreateDriverRequest request, CancellationToken cancellationToken = default)
    {
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