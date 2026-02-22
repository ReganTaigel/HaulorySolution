using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Haulory.Domain.Enums;

namespace Haulory.Application.Features.Drivers;

public class CreateDriverFromUserHandler
{
    #region Dependencies

    private readonly IDriverRepository _repository;
    private readonly IComplianceEnsurer _complianceEnsurer;
    private readonly IUserAccountRepository _users;

    #endregion

    #region Constructor

    public CreateDriverFromUserHandler(
        IDriverRepository repository,
        IComplianceEnsurer complianceEnsurer,
        IUserAccountRepository users)
    {
        _repository = repository;
        _complianceEnsurer = complianceEnsurer;
        _users = users;
    }

    #endregion

    #region Public API

    // Creates a Driver profile linked to an existing authenticated User account.
    // Rules:
    // - UserId must exist.
    // - One Driver profile per User account.
    // - OwnerUserId must always resolve to the Main account.
    // - Automatically seeds compliance/induction records.
    public async Task<Driver?> HandleAsync(CreateDriverFromUserCommand command)
    {
        #region Basic Validation

        if (command.UserId == Guid.Empty)
            return null;

        var email = command.Email?.Trim().ToLowerInvariant();
        var firstName = command.FirstName?.Trim();
        var lastName = command.LastName?.Trim();

        if (string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(email))
            return null;

        #endregion

        #region Load Actor Account

        // The authenticated user we are converting into a Driver profile
        var actor = await _users.GetByIdAsync(command.UserId);
        if (actor == null)
            return null;

        #endregion

        #region Resolve Owner (Main Account Enforcement)

        // OwnerUserId must ALWAYS be the main/root account
        var ownerUserId = actor.Role == UserRole.Main
            ? actor.Id
            : (actor.ParentMainUserId ?? Guid.Empty);

        if (ownerUserId == Guid.Empty)
            return null;

        #endregion

        #region Prevent Duplicate Driver Profiles

        // One driver profile per user account
        var existing = await _repository.GetByUserIdAsync(actor.Id);
        if (existing != null)
            return existing;

        #endregion

        #region Create Driver Aggregate

        var driver = new Driver(
            ownerUserId: ownerUserId,  // Main account
            userId: actor.Id,          // Linked login account
            firstName: firstName!,
            lastName: lastName!,
            email: email!
        );

        await _repository.SaveAsync(driver);

        #endregion

        #region Seed Compliance Records

        // Automatically create required induction/compliance rows
        await _complianceEnsurer
            .EnsureDriverInductionsExistForDriverAsync(ownerUserId, driver.Id);

        #endregion

        return driver;
    }

    #endregion
}
