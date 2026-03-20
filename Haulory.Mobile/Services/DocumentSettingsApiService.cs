using System.Net;
using System.Net.Http.Json;
using Haulory.Contracts.Settings;

namespace Haulory.Mobile.Services;

public class DocumentSettingsApiService
{
    private readonly HttpClient _httpClient;

    public DocumentSettingsApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DocumentSettingsDto?> GetAsync()
    {
        var response = await _httpClient.GetAsync("api/document-settings");

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<DocumentSettingsDto>();
    }

    public async Task<bool> SaveAsync(UpdateDocumentSettingsRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync("api/document-settings", request);
        return response.IsSuccessStatusCode;
    }
}