using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Features.Drivers;
using Haulory.Application.Security;
using Haulory.Core.Security;
using Haulory.Domain.Entities;

namespace Haulory.Application.Features.Users
{
    public class RegisterUserHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly CreateDriverFromUserHandler _createDriverFromUserHandler;
        public record RegisterUserResult(bool Success, string? Error);
        public RegisterUserHandler(
            IUserRepository userRepository,
            CreateDriverFromUserHandler createDriverFromUserHandler)
        {
            _userRepository = userRepository;
            _createDriverFromUserHandler = createDriverFromUserHandler;
        }

        public async Task<RegisterUserResult> HandleAsync(RegisterUserCommand command)
        {
            var email = command.Email?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email))
                return new(false, "Email is required.");

            if (string.IsNullOrWhiteSpace(command.FirstName))
                return new(false, "First name is required.");

            if (string.IsNullOrWhiteSpace(command.LastName))
                return new(false, "Last name is required.");

            if (!PasswordPolicy.IsValid(command.Password, out var error))
                return new(false, error ?? "Password does not meet requirements.");

            var existing = await _userRepository.GetByEmailAsync(email);
            if (existing != null)
                return new(false, "A user with this email already exists.");

            var hash = PasswordHasher.Hash(command.Password);

            var user = new User(
                command.FirstName.Trim().ToUpper(),
                command.LastName.Trim().ToUpper(),
                email,
                hash);

            await _userRepository.AddAsync(user);

            // Create Driver profile for Main user (idempotent)
            await _createDriverFromUserHandler.HandleAsync(
                new CreateDriverFromUserCommand(
                    user.Id,
                    user.FirstName ?? string.Empty,
                    user.LastName ?? string.Empty,
                    user.Email ?? string.Empty
                )
            );

            return new(true, null);
        }
    }
}
