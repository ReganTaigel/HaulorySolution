using HaulitCore.Application.Interfaces.Services;
using HaulitCore.Contracts.Vehicles;


namespace HaulitCore.Mobile.Services;

public sealed class HubodometerApiService : ApiServiceBase
{
    public HubodometerApiService(HttpClient httpClient, ISessionService sessionService)
        : base(httpClient, sessionService)
    {
    }

    public async Task<IReadOnlyList<HubodometerAssetDto>> GetAssetsAsync(CancellationToken cancellationToken = default)
        => await GetAsync<List<HubodometerAssetDto>>("api/Hubodometer/assets", cancellationToken) ?? new List<HubodometerAssetDto>();

    public Task RecordReadingAsync(HubodometerReadingRequest request, CancellationToken cancellationToken = default)
        => PostAsync<object>("api/Hubodometer/readings", request, cancellationToken);
}