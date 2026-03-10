using System.Net.Http.Headers;
using Haulory.Application.Interfaces.Services;
using Haulory.Mobile.Contracts.Drivers;

namespace Haulory.Mobile.Services;

public sealed class DriversApiService : ApiServiceBase
{
    public DriversApiService(HttpClient httpClient, ISessionService sessionService)
        : base(httpClient, sessionService)
    {
    }

    public async Task<IReadOnlyList<DriverDto>> GetDriversAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<DriverDto>>("api/drivers", cancellationToken) ?? new List<DriverDto>();

    public async Task<DriverDto?> GetDriverByIdAsync(Guid driverId, CancellationToken cancellationToken = default)
        => await GetAsync<DriverDto>($"api/drivers/{driverId}", cancellationToken);

    public Task<DriverDto> CreateDriverAsync(CreateDriverRequest request, CancellationToken cancellationToken = default)
        => PostAsync<DriverDto>("api/drivers", request, cancellationToken);

    public Task<DriverDto> UpdateDriverAsync(Guid driverId, UpdateDriverRequest request, CancellationToken cancellationToken = default)
        => PutAsync<DriverDto>($"api/drivers/{driverId}", request, cancellationToken);

    public async Task UploadInductionEvidenceAsync(
        Guid driverId,
        Guid workSiteId,
        Guid requirementId,
        string localFilePath,
        CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync();

        if (!File.Exists(localFilePath))
            throw new FileNotFoundException("Selected file not found.", localFilePath);

        using var content = new MultipartFormDataContent();

        await using var stream = File.OpenRead(localFilePath);
        using var fileContent = new StreamContent(stream);

        fileContent.Headers.ContentType =
            new MediaTypeHeaderValue(GetContentType(localFilePath));

        content.Add(fileContent, "file", Path.GetFileName(localFilePath));

        var response = await HttpClient.PostAsync(
            $"api/drivers/{driverId}/inductions/{workSiteId}/{requirementId}/evidence",
            content,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Upload failed: {body}");
        }
    }

    public Task DeleteInductionEvidenceAsync(
        Guid driverId,
        Guid workSiteId,
        Guid requirementId,
        CancellationToken cancellationToken = default)
        => DeleteAsync(
            $"api/drivers/{driverId}/inductions/{workSiteId}/{requirementId}/evidence",
            cancellationToken);

    private static string GetContentType(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();

        return ext switch
        {
            ".pdf" => "application/pdf",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };
    }
}