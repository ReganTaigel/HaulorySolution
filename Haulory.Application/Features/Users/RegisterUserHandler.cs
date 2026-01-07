using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Security;
using Haulory.Core.Security;
using Haulory.Domain.Entities;


namespace Haulory.Application.Features.Users
{
    public class RegisterUserHandler
    {
        private readonly IUserRepository _userRepository;

        public RegisterUserHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> HandleAsync(RegisterUserCommand command)
        {
            // NORMALISE EMAIL
            var email = command.Email?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email))
                return false;

            // PASSWORD POLICY CHECK
            if (!PasswordPolicy.IsValid(command.Password, out var error))
                return false;

            // CHECK FOR EXISTING USER
            var existing = await _userRepository.GetByEmailAsync(email);
            if (existing != null)
                return false;

            var hash = PasswordHasher.Hash(command.Password);

            var user = new User(
                command.FirstName.Trim(),
                command.LastName.Trim(),
                email,
                hash);

            await _userRepository.AddAsync(user);
            return true;
        }

    }
}
