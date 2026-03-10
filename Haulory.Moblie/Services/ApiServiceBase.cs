using System.Net.Http.Headers;
using System.Net.Http.Json;
using Haulory.Application.Interfaces.Services;

namespace Haulory.Mobile.Services;

public abstract class ApiServiceBase
{
    protected readonly HttpClient HttpClient;
    private readonly ISessionService _sessionService;

    protected ApiServiceBase(HttpClient httpClient, ISessionService sessionService)
    {
        HttpClient = httpClient;
        _sessionService = sessionService;
    }

    protected async Task EnsureAuthenticatedAsync()
    {
        if (!_sessionService.IsAuthenticated)
            await _sessionService.RestoreAsync();

        var token = _sessionService.JwtToken;

        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("User is not authenticated.");

        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    protected async Task<T?> GetAsync<T>(string uri, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync();
        return await HttpClient.GetFromJsonAsync<T>(uri, cancellationToken);
    }

    protected async Task<T> PostAsync<T>(string uri, object request, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync();

        var response = await HttpClient.PostAsJsonAsync(uri, request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Request failed: {(int)response.StatusCode}. Body: {body}");

        var result = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);

        if (result == null)
            throw new InvalidOperationException("API returned an empty response.");

        return result;
    }

    protected async Task PutAsync(string uri, object request, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync();

        var response = await HttpClient.PutAsJsonAsync(uri, request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Request failed: {(int)response.StatusCode}. Body: {body}");
    }

    protected async Task DeleteAsync(string uri, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync();

        var response = await HttpClient.DeleteAsync(uri, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Request failed: {(int)response.StatusCode}. Body: {body}");
    }

    protected async Task<T> PutAsync<T>(string uri, object request, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync();

        var response = await HttpClient.PutAsJsonAsync(uri, request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Request failed: {(int)response.StatusCode}. Body: {body}");

        var result = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);

        if (result == null)
            throw new InvalidOperationException("API returned an empty response.");

        return result;
    }
}