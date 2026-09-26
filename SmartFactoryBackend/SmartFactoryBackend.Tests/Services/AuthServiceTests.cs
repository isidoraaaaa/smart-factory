using Moq;
using SmartFactoryBackend.Application.Interfaces;
using SmartFactoryBackend.Application.Services;
using SmartFactoryBackend.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFactoryBackend.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<IJwtTokenGenerator> _tokenGeneratorMock;
        private readonly AuthService _authService;

        public AuthServiceTests() {
            _userRepositoryMock = new Mock<IUserRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _tokenGeneratorMock = new Mock<IJwtTokenGenerator>();
            _authService = new AuthService(_userRepositoryMock.Object, _passwordHasherMock.Object, _tokenGeneratorMock.Object);
        }

        [Fact]
        public async Task RegisterAsync_UsernameAlreadyTaken_ThrowsInvalidOperationException()
        {
            // Arrange
            _userRepositoryMock
                .Setup(r => r.ExistsByUsernameAsync("doki"))
                .ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authService.RegisterAsync("Name", "Lastname", "doki", "email@test.com", "password123")
            );

            Assert.Equal("Username is already taken.", exception.Message);
            _userRepositoryMock.Verify(r => r.AddUserAsync(It.IsAny<User>()), Times.Never);
        }


        [Fact]
        public async Task RegisterAsync_NewUsername_HashesPasswordAndReturnsToken()
        {
            // Arrange
            _userRepositoryMock
                .Setup(r => r.ExistsByUsernameAsync("newuser"))
                .ReturnsAsync(false);

            _passwordHasherMock
                .Setup(h => h.Hash("password123"))
                .Returns("hashed_password");

            _tokenGeneratorMock
                .Setup(t => t.GenerateToken(It.IsAny<User>()))
                .Returns("fake_jwt_token");

            // Act
            var result = await _authService.RegisterAsync("Name", "Lastname", "newuser", "email@test.com", "password123");

            // Assert
            Assert.Equal("fake_jwt_token", result);
            _userRepositoryMock.Verify(r => r.AddUserAsync(It.Is<User>(u =>
                u.Username == "newuser" && u.Password == "hashed_password")), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_UserDoesNotExist_ReturnsNull()
        {
            // Arrange
            _userRepositoryMock.Setup(r=> r.GetByUsernameAsync("nonexistent"))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _authService.LoginAsync("nonexistent", "password123");
            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_ReturnsNull()
        {
            _userRepositoryMock.Setup(r=>r.GetByUsernameAsync("existinguser"))
                                .ReturnsAsync(new User { Username = "existinguser", Password = "hashed_password" });
           _passwordHasherMock.Setup(h=>h.Verify("wrong_password", "hashed_password")).Returns(false);
            var result = await _authService.LoginAsync("existinguser", "wrong_password");
            Assert.Null(result);
        }


        [Fact]
        public async Task LoginAsync_CorrectCredentials_ReturnsToken()
        {    
            var existingUser = new User { Username = "existinguser", Password = "hashed_password" };
            _userRepositoryMock.Setup(r => r.GetByUsernameAsync("existinguser"))
                               .ReturnsAsync(existingUser);

            _passwordHasherMock.Setup(h=>h.Verify("correct_password", "hashed_password")).Returns(true);
           
            _tokenGeneratorMock.Setup(t=>t.GenerateToken(It.IsAny<User>())).Returns("fake_jwt_token");

            var result = await _authService.LoginAsync("existinguser", "correct_password");

            Assert.Equal("fake_jwt_token", result);
        }
    }
}
