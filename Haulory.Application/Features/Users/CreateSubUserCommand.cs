using Haulory.Domain.Enums;

namespace Haulory.Application.Features.Users;

public record CreateSubUserCommand(
    Guid OwnerMainUserId,   // tenant/main user id
    string FirstName,
    string LastName,
    string Email,
    string Password
);