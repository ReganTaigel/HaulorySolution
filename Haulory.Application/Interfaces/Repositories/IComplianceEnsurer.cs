public interface IComplianceEnsurer
{
    Task EnsureAllDriverInductionsExistAsync(Guid ownerUserId);
    Task EnsureDriverInductionsExistForDriverAsync(Guid ownerUserId, Guid driverId);

    Task EnsureDriverSiteInductionsExistAsync(
        Guid ownerUserId,
        Guid driverId,
        Guid workSiteId,
        DateTime issueDateUtc);
}
