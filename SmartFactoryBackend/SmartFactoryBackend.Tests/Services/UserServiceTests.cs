using Moq;
using SmartFactoryBackend.Application.Interfaces;
using SmartFactoryBackend.Application.Services;
using SmartFactoryBackend.Domain.Enums;
using SmartFactoryBackend.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SmartFactoryBackend.Tests.Services
{
    public class UserServiceTests
    {   
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly UserService _userService;
        public UserServiceTests()
        { 
            _userRepositoryMock = new Mock<IUserRepository>();
            _userService = new UserService(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task DeleteUserAsync_ExistingUser_ReturnsTrue()
        {
            var userId = Guid.NewGuid();

            _userRepositoryMock.Setup(r=> r.DeleteAsync(userId))
                .ReturnsAsync(true);

            var result = await _userService.DeleteUserAsync(userId, Guid.NewGuid());

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteUserAsync_NonExistingUser_ReturnsFalse()
        {
            var userId = Guid.NewGuid();

            _userRepositoryMock.Setup(r => r.DeleteAsync(userId))
                .ReturnsAsync(false);

            var result = await _userService.DeleteUserAsync(userId, Guid.NewGuid());

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteUserAsync_DeleteOwnAccount_ThrowsInvalidOperationException()
        {
            var userId = Guid.NewGuid();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _userService.DeleteUserAsync(userId, userId)
            );

            Assert.Equal("You cannot delete your own account.", exception.Message);
            _userRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);

        }

        [Fact]
        public async Task GetUserAsync_NonExistingUser_ReturnsNull()
        {
            var userId = Guid.NewGuid();

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync((User?)null);

            var result = await _userService.GetUserAsync(userId);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserAsync_ReturnsExistingUser()
        {
            var userId = Guid.NewGuid();
            var existingUser = new User
            {
                Id = userId,
                Name = "John",
                Lastname = "Doe",
                Username = "johndoe",
                Email = "johndoe@gmail.com",
                UserType = UserType.Admin
            };

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);

            var result = await _userService.GetUserAsync(userId);

            Assert.NotNull(result);
            Assert.Equal(existingUser.Id, result.Id);

        }

        [Fact]
        public async Task GetUsersAsync_ReturnsAllUsersMappedToDto()
        {
            var users = new List<User>
            {
                new() { Id = Guid.NewGuid(), Name = "John", Lastname = "Doe", Username = "johndoe", Email = "john@test.com", UserType = UserType.Operator },
                new() { Id = Guid.NewGuid(), Name = "Jane", Lastname = "Smith", Username = "janesmith", Email = "jane@test.com", UserType = UserType.Admin }
            };

            _userRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(users);
            var result = await _userService.GetUsersAsync();

            Assert.NotNull(result);
            Assert.Equal(users.Count, result.Count());
            Assert.Contains(result, u => u.Username == "johndoe" && u.UserType == UserType.Operator);
            Assert.Contains(result, u => u.Username == "janesmith" && u.UserType == UserType.Admin);
        }

        [Fact]
        public async Task GetUsersAsync_NoUsers_ReturnsEmptyList()
        {
            _userRepositoryMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<User>());

            var result = await _userService.GetUsersAsync();

            Assert.Empty(result);
        }
    }
}
