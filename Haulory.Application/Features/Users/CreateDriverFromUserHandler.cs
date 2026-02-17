using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;

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
        var email = command.Email?.Trim().ToLowerInvariant();
        var firstName = command.FirstName?.Trim();
        var lastName = command.LastName?.Trim();

        if (command.UserId == Guid.Empty)
            return null;

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            return null;

        if (string.IsNullOrWhiteSpace(email))
            return null;

        // actor = the login account we are creating a Driver profile for
        var actor = await _users.GetByIdAsync(command.UserId);
        if (actor == null)
            return null;

        // ownerUserId must ALWAYS be the main account id
        var ownerUserId = actor.Role == UserRole.Main
            ? actor.Id
            : (actor.ParentMainUserId ?? Guid.Empty);

        if (ownerUserId == Guid.Empty)
            return null;

        // one driver profile per user account
        var existing = await _repository.GetByUserIdAsync(actor.Id);
        if (existing != null)
            return existing;

        var driver = new Driver(
            ownerUserId: ownerUserId,   // main account id
            userId: actor.Id,           // linked user account id
            firstName: firstName!,
            lastName: lastName!,
            email: email!);

        await _repository.SaveAsync(driver);

        await _complianceEnsurer.EnsureDriverInductionsExistForDriverAsync(ownerUserId, driver.Id);

        return driver;
    }

}
