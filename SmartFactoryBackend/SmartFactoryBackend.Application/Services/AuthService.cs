using SmartFactoryBackend.Application.Interfaces;
using SmartFactoryBackend.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFactoryBackend.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _tokenGenerator;

        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator tokenGenerator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenGenerator = tokenGenerator;
        }
        public async Task<string?> LoginAsync(string username, string password)
        {
            var user = await _userRepository.GetByUsernameAsync(username);

            if (user is null || !_passwordHasher.Verify(password, user.Password))
            {
                return null;
            }

            return _tokenGenerator.GenerateToken(user);
        }

        public async Task<string?> RegisterAsync(string name, string lastname, string username, string email, string password)
        {
            var usernameTaken = await _userRepository.ExistsByUsernameAsync(username);

            if (usernameTaken)
            {
                throw new InvalidOperationException("Username is already taken.");
            }

            var user = new User
            {
                Name = name,
                Lastname = lastname,
                Username = username,
                Email = email,
                Password = _passwordHasher.Hash(password)
            };

            await _userRepository.AddUserAsync(user);

           return _tokenGenerator.GenerateToken(user);
        }
    }
}
