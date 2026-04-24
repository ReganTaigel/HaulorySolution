using Microsoft.Extensions.DependencyInjection;

namespace HaulitCore.Mobile.DependencyInjection;

public static class HttpClientRegistrationExtensions
{
    public static IServiceCollection AddMobileHttpClients(this IServiceCollection services)
    {
#if DEBUG
        var useLocalApi = false;

        var baseUrl = useLocalApi
            ? "http://10.0.2.2:5158/"
            : "https://haulitcore-api-dev-hwbrh9gwe2h3a3ex.australiaeast-01.azurewebsites.net/";
#else
        var baseUrl = "https://haulitcore-api-bmgaffh5c3c6gfd9.australiaeast-01.azurewebsites.net/";
#endif

        services.AddSingleton(sp =>
        {
#if ANDROID
            var handler = new Xamarin.Android.Net.AndroidMessageHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.All
            };
#else
            var handler = new HttpClientHandler();
#endif

            return new HttpClient(handler)
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
        });

        return services;
    }
}