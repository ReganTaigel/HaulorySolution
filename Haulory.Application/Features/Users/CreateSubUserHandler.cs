using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Limits;
using Haulory.Application.Security;
using Haulory.Core.Security;
using Haulory.Domain.Entities;
using Haulory.Domain.Enums;

namespace Haulory.Application.Features.Users;

public class CreateSubUserHandler
{
    private readonly IUserAccountRepository _repo;
    private readonly IDriverRepository _drivers;

    public CreateSubUserHandler(
        IUserAccountRepository repo,
        IDriverRepository drivers)
    {
        _repo = repo;
        _drivers = drivers;
    }

    public async Task<UserAccount?> HandleAsync(CreateSubUserCommand command)
    {
        // Must have valid ids
        if (command.RequestorAccountId == Guid.Empty) return null;
        if (command.OwnerMainUserId == Guid.Empty) return null;

        // Must be logged in as MAIN of this tenant
        var requestor = await _repo.GetByIdAsync(command.RequestorAccountId);
        if (requestor == null) return null;

        var isMain = requestor.Role == UserRole.Main;
        var isTenantRoot = requestor.Id == command.OwnerMainUserId;

        if (!isMain || !isTenantRoot)
            return null;

        // Validate fields
        var email = command.Email?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email)) return null;

        if (string.IsNullOrWhiteSpace(command.FirstName)) return null;
        if (string.IsNullOrWhiteSpace(command.LastName)) return null;

        if (!PasswordPolicy.IsValid(command.Password, out _))
            return null;

        // Email must be unique globally (your repo enforces uniqueness)
        var existing = await _repo.GetByEmailAsync(email);
        if (existing != null)
            return null;

        // Hash password
        var hash = PasswordHasher.Hash(command.Password);

        // Create SUB user under this tenant
        var sub = UserAccount.CreateSubUser(
            parentMainUserId: command.OwnerMainUserId,
            firstName: command.FirstName.Trim(),
            lastName: command.LastName.Trim(),
            email: email,
            passwordHash: hash
        );

        var subDriverCount = await _drivers.CountSubDriversAsync(command.OwnerMainUserId);

        if (subDriverCount >= PlanLimits.MaxSubDrivers)
            throw new InvalidOperationException(
                $"Sub-user limit reached (max {PlanLimits.MaxSubDrivers}).");

        await _repo.AddAsync(sub);
        return sub;
    }
}