using Haulory.Contracts.Diagnostics;

namespace Haulory.Mobile.Diagnostics;

public static class CrashLogMappings
{
    public static CrashLogDto ToDto(this CrashLog entity)
    {
        return new CrashLogDto
        {
            Id = entity.Id,
            Source = entity.Source,
            Severity = entity.Severity,
            Message = entity.Message,
            StackTrace = entity.StackTrace,
            InnerException = entity.InnerException,
            ExceptionType = entity.ExceptionType,
            AccountId = entity.AccountId,
            OwnerId = entity.OwnerId,
            PageName = entity.PageName,
            Platform = entity.Platform,
            AppVersion = entity.AppVersion,
            AppBuild = entity.AppBuild,
            IsHandled = entity.IsHandled,
            CreatedUtc = entity.CreatedUtc,
            MetadataJson = entity.MetadataJson
        };
    }
}