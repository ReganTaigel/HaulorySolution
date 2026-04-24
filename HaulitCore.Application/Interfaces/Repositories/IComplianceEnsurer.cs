namespace HaulitCore.Application.Interfaces.Repositories;

#region Interface: Compliance Ensurer

public interface IComplianceEnsurer
{
    #region Driver-Level Seeding

    // Ensures all required induction/compliance records exist
    // for every driver under the specified owner
    Task EnsureAllDriverInductionsExistAsync(Guid ownerUserId);

    // Ensures required induction/compliance records exist
    // for a specific driver under an owner
    Task EnsureDriverInductionsExistForDriverAsync(Guid ownerUserId, Guid driverId);

    #endregion

    #region Worksite-Specific Seeding

    // Ensures induction records exist for a driver at a specific worksite
    // issueDateUtc is used as the base date for calculating expiry
    Task EnsureDriverSiteInductionsExistAsync(
        Guid ownerUserId,
        Guid driverId,
        Guid workSiteId,
        DateTime issueDateUtc);

    #endregion
}

#endregion
