namespace Haulory.Application.Features.Users;

public record CreateSubUserCommand(
    Guid RequestorAccountId,   // who is calling this (CurrentAccountId)
    Guid OwnerMainUserId,       // tenant root (CurrentOwnerId)
    string FirstName,
    string LastName,
    string Email,
    string Password
);