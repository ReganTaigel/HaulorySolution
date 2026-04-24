using System.Net;
using System.Net.Http.Json;
using HaulitCore.Contracts.Settings;

namespace HaulitCore.Mobile.Services;

public class BusinessProfileApiService
{
    private readonly HttpClient _httpClient;

    public BusinessProfileApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<UpdateBusinessProfileRequest?> GetAsync()
    {
        var response = await _httpClient.GetAsync("api/business-profile");

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<UpdateBusinessProfileRequest>();
    }

    public async Task<bool> SaveAsync(UpdateBusinessProfileRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync("api/business-profile", request);
        return response.IsSuccessStatusCode;
    }
}