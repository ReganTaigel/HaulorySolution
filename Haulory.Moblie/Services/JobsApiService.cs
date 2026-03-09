using System.Net.Http.Headers;
using System.Net.Http.Json;
using Haulory.Application.Interfaces.Services;
using Haulory.Mobile.Contracts.Jobs;

namespace Haulory.Mobile.Services;

public sealed class JobsApiService
{
    private readonly HttpClient _httpClient;
    private readonly ISessionService _sessionService;

    public JobsApiService(HttpClient httpClient, ISessionService sessionService)
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

    public async Task<IReadOnlyList<JobDto>> GetActiveJobsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync();

        var result = await _httpClient.GetFromJsonAsync<List<JobDto>>(
            "api/jobs/active",
            cancellationToken);

        return result ?? new List<JobDto>();
    }

    public async Task<JobDto?> GetJobByIdAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync();

        return await _httpClient.GetFromJsonAsync<JobDto>(
            $"api/jobs/{jobId}",
            cancellationToken);
    }

    public async Task<IReadOnlyList<TrailerLookupDto>> GetAvailableTrailersAsync(CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync();

        var result = await _httpClient.GetFromJsonAsync<List<TrailerLookupDto>>(
            "api/jobs/trailers",
            cancellationToken);

        return result ?? new List<TrailerLookupDto>();
    }

    public async Task<CreateJobResponse> CreateJobAsync(
        CreateJobRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync();

        var response = await _httpClient.PostAsJsonAsync(
            "api/jobs",
            request,
            cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Failed to create job. Status: {(int)response.StatusCode}. Body: {responseBody}");
        }

        var result = await response.Content.ReadFromJsonAsync<CreateJobResponse>(
            cancellationToken: cancellationToken);

        if (result is null)
            throw new InvalidOperationException("API returned an empty job creation response.");

        return result;
    }

    public async Task CompleteJobAsync(Guid jobId, CompleteJobRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync();

        var response = await _httpClient.PostAsJsonAsync(
            $"api/jobs/{jobId}/complete",
            request,
            cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Failed to complete job. Status: {(int)response.StatusCode}. Body: {responseBody}");
        }
    }
}