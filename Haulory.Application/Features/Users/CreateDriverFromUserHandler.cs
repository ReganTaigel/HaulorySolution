using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;

namespace Haulory.Application.Features.Drivers;

public class CreateDriverFromUserHandler
{
    private readonly IDriverRepository _repository;

    public CreateDriverFromUserHandler(IDriverRepository repository)
    {
        _repository = repository;
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

        var existing = await _repository.GetByUserIdAsync(command.UserId);
        if (existing != null)
            return existing;

        var driver = new Driver(
            ownerUserId: command.UserId,
            userId: command.UserId,
            firstName: firstName!,
            lastName: lastName!,
            email: email!);

        await _repository.SaveAsync(driver);

        return driver;
    }
}
