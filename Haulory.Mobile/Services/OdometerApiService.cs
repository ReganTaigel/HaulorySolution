using Haulory.Application.Interfaces.Services;
using Haulory.Contracts.Vehicles;


namespace Haulory.Mobile.Services;

public sealed class OdometerApiService : ApiServiceBase
{
    public OdometerApiService(HttpClient httpClient, ISessionService sessionService)
        : base(httpClient, sessionService)
    {
    }

    public async Task<IReadOnlyList<OdometerAssetDto>> GetAssetsAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<OdometerAssetDto>>("api/odometer/assets", cancellationToken) ?? new List<OdometerAssetDto>();

    public Task RecordReadingAsync(OdometerReadingRequest request, CancellationToken cancellationToken = default)
        => PostAsync<object>("api/odometer/readings", request, cancellationToken);
}