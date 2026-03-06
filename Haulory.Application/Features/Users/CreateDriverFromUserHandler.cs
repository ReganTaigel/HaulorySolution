using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Limits;
using Haulory.Domain.Entities;
using Haulory.Domain.Enums;

namespace Haulory.Application.Features.Drivers;

public class CreateDriverFromUserHandler
{
    private readonly IDriverRepository _repository;
    private readonly IComplianceEnsurer _complianceEnsurer;
    private readonly IUserAccountRepository _users;

    public CreateDriverFromUserHandler(
        IDriverRepository repository,
        IComplianceEnsurer complianceEnsurer,
        IUserAccountRepository users)
    {
        _repository = repository;
        _complianceEnsurer = complianceEnsurer;
        _users = users;
    }

    public async Task<Driver?> HandleAsync(CreateDriverFromUserCommand command)
    {
        if (command.UserId == Guid.Empty)
            return null;

        var email = command.Email?.Trim().ToLowerInvariant();
        var firstName = command.FirstName?.Trim();
        var lastName = command.LastName?.Trim();

        if (string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(email))
            return null;

        var actor = await _users.GetByIdAsync(command.UserId);
        if (actor == null)
            return null;

        // OwnerUserId must ALWAYS be the main/root account
        var ownerUserId = actor.Role == UserRole.Main
            ? actor.Id
            : (actor.ParentMainUserId ?? Guid.Empty);

        if (ownerUserId == Guid.Empty)
            return null;

        // One driver profile per user account
        var existing = await _repository.GetByUserIdAsync(actor.Id);
        if (existing != null)
            return existing;

        // Limits: main user gets 1 "main driver", sub-users count toward sub limit
        if (actor.Role == UserRole.Main)
        {
            var mainCount = await _repository.CountMainDriversAsync(ownerUserId);
            if (mainCount >= PlanLimits.MaxMainDrivers)
                throw new InvalidOperationException("Main driver limit reached (max 1).");
        }
        else
        {
            var subCount = await _repository.CountSubDriversAsync(ownerUserId);
            if (subCount >= PlanLimits.MaxSubDrivers)
                throw new InvalidOperationException("Sub driver limit reached.");
        }

        var driver = new Driver(
            ownerUserId: ownerUserId,
            userId: actor.Id,
            firstName: firstName!,
            lastName: lastName!,
            email: email!
        );

        await _repository.SaveAsync(driver);

        await _complianceEnsurer
            .EnsureDriverInductionsExistForDriverAsync(ownerUserId, driver.Id);

        return driver;
    }
}