using Haulory.Application.Interfaces.Services;
using Haulory.Mobile.Contracts.Jobs;
using System.Net.Http.Json;

namespace Haulory.Mobile.Services;

public sealed class JobsApiService : ApiServiceBase
{
    public JobsApiService(HttpClient httpClient, ISessionService sessionService)
        : base(httpClient, sessionService)
    {
    }

    public async Task<IReadOnlyList<JobDto>> GetActiveJobsAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<JobDto>>("api/jobs/active", cancellationToken) ?? new List<JobDto>();

    public async Task<JobDto?> GetJobByIdAsync(Guid jobId, CancellationToken cancellationToken = default)
        => await GetAsync<JobDto>($"api/jobs/{jobId}", cancellationToken);

    public Task<CreateJobResponse> CreateJobAsync(CreateJobRequest request, CancellationToken cancellationToken = default)
        => PostAsync<CreateJobResponse>("api/jobs", request, cancellationToken);

    public async Task CompleteJobAsync(Guid jobId, CompleteJobRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync();

        var response = await HttpClient.PostAsJsonAsync($"api/jobs/{jobId}/complete", request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to complete job. Status: {(int)response.StatusCode}. Body: {body}");
    }
}