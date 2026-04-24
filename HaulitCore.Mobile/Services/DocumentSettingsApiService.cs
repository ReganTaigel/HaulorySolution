using System.Net;
using System.Net.Http.Json;
using HaulitCore.Contracts.Settings;

namespace HaulitCore.Mobile.Services;

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
        System.Diagnostics.Debug.WriteLine(
            $"[API Service Sending] GST={request.GstEnabled}, GST Rate={request.GstRatePercent}, Fuel={request.FuelSurchargeEnabled}, Fuel Rate={request.FuelSurchargePercent}");

        var response = await _httpClient.PutAsJsonAsync("api/document-settings", request);

        var body = await response.Content.ReadAsStringAsync();

        System.Diagnostics.Debug.WriteLine($"[Settings Save Response] Status={(int)response.StatusCode}");
        System.Diagnostics.Debug.WriteLine($"[Settings Save Response] Body={body}");

        return response.IsSuccessStatusCode;
    }
}