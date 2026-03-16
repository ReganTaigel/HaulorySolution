using Microsoft.Extensions.DependencyInjection;

namespace Haulory.Mobile.DependencyInjection;

public static class HttpClientRegistrationExtensions
{
    public static IServiceCollection AddMobileHttpClients(this IServiceCollection services)
    {
#if DEBUG
        var useLocalApi = false;

        var baseUrl = useLocalApi
            ? "http://10.0.2.2:5158/"
            : "https://haulory-api-b5a4h9crbmd4a2hh.australiaeast-01.azurewebsites.net/";
#else
        var baseUrl = "https://haulory-api-b5a4h9crbmd4a2hh.australiaeast-01.azurewebsites.net/";
#endif

        services.AddSingleton(new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        });

        return services;
    }
}