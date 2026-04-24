using HaulitCore.Application.Interfaces.Repositories;
using HaulitCore.Application.Limits;
using HaulitCore.Application.Security;
using HaulitCore.Core.Security;
using HaulitCore.Domain.Entities;
using HaulitCore.Domain.Enums;

namespace HaulitCore.Application.Features.Users;

// Handles the application use case for creating a sub-user account.
// A sub-user belongs to an existing main/owner account within the same tenant.
public class CreateSubUserHandler
{
    // Repository for user account operations.
    private readonly IUserAccountRepository _repo;

    // Repository used to check driver/sub-user limits for the tenant.
    private readonly IDriverRepository _drivers;

    // Constructor injection of dependencies.
    public CreateSubUserHandler(
        IUserAccountRepository repo,
        IDriverRepository drivers)
    {
        _repo = repo;
        _drivers = drivers;
    }

    // Creates a new sub-user account if the request is valid and authorised.
    public async Task<UserAccount?> HandleAsync(CreateSubUserCommand command)
    {
        // Require valid requestor and owner IDs.
        if (command.RequestorAccountId == Guid.Empty) return null;
        if (command.OwnerMainUserId == Guid.Empty) return null;

        // Load the authenticated requestor.
        var requestor = await _repo.GetByIdAsync(command.RequestorAccountId);
        if (requestor == null) return null;

        // Only the main account of the tenant may create sub-users.
        var isMain = requestor.Role == UserRole.Main;
        var isTenantRoot = requestor.Id == command.OwnerMainUserId;

        if (!isMain || !isTenantRoot)
            return null;

        // Normalise and validate the email address.
        var email = command.Email?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email)) return null;

        // Validate required identity fields.
        if (string.IsNullOrWhiteSpace(command.FirstName)) return null;
        if (string.IsNullOrWhiteSpace(command.LastName)) return null;

        // Enforce password policy.
        if (!PasswordPolicy.IsValid(command.Password, out _))
            return null;

        // Email address must be unique across all users.
        var existing = await _repo.GetByEmailAsync(email);
        if (existing != null)
            return null;

        // Hash password before storing it.
        var hash = PasswordHasher.Hash(command.Password);

        // Create the sub-user account under the tenant's main user.
        var sub = UserAccount.CreateSubUser(
            parentMainUserId: command.OwnerMainUserId,
            firstName: command.FirstName.Trim(),
            lastName: command.LastName.Trim(),
            email: email,
            passwordHash: hash
        );

        // Enforce plan limit for sub drivers/sub-users within the tenant.
        var subDriverCount = await _drivers.CountSubDriversAsync(command.OwnerMainUserId);

        if (subDriverCount >= PlanLimits.MaxSubDrivers)
            throw new InvalidOperationException(
                $"Sub-user limit reached (max {PlanLimits.MaxSubDrivers}).");

        // Persist the new sub-user.
        await _repo.AddAsync(sub);

        return sub;
    }
}