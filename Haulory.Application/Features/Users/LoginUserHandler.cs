using Haulory.Application.Interfaces.Repositories;
using Haulory.Core.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace Haulory.Application.Features.Users
{
    public class LoginUserHandler
    {
        private readonly IUserRepository _userRepository;
        
        public LoginUserHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository; 
        }

        public async Task<bool> HandleAsync(LoginUserCommand command)
        {

            var user = await _userRepository.GetByEmailAsync(command.Email);
            if (user == null) 
                return false;

            return PasswordHasher.Verify(command.Password, user.PasswordHash );
        }
    }
}
