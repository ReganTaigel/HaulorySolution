using System.Text.Json;

namespace Haulory.Mobile.Diagnostics;

public static class CrashMetadataBuilder
{
    public static string Build(
        string? route = null,
        string? feature = null,
        object? extra = null)
    {
        var payload = new
        {
            Route = route,
            Feature = feature,
            DeviceModel = DeviceInfo.Model,
            Manufacturer = DeviceInfo.Manufacturer,
            DeviceType = DeviceInfo.DeviceType.ToString(),
            Idiom = DeviceInfo.Idiom.ToString(),
            Platform = DeviceInfo.Platform.ToString(),
            PlatformVersion = DeviceInfo.VersionString,
            AppVersion = AppInfo.VersionString,
            AppBuild = AppInfo.BuildString,
            TimestampUtc = DateTime.UtcNow,
            Extra = extra
        };

        return JsonSerializer.Serialize(payload);
    }
}