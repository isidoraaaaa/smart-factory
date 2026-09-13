using SmartFactoryBackend.Application.DTO;
using SmartFactoryBackend.Application.Interfaces;
using SmartFactoryBackend.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFactoryBackend.Application.Services
{
    public class UserService : IUserService
    {
        IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> DeleteUserAsync(Guid userId, Guid requestingUserId)
        {
            if (userId == requestingUserId)
            {
                throw new InvalidOperationException("You cannot delete your own account.");
            }

            return await _userRepository.DeleteAsync(userId);
        }

        public async Task<UserDTO?> GetUserAsync(Guid id)
        {
            var user =  await _userRepository.GetByIdAsync(id);
            return user != null ? new UserDTO
            {
                Id = user.Id,
                Name = user.Name,
                Lastname = user.Lastname,
                Username = user.Username,
                Email = user.Email,
                UserType = user.UserType
            } : null;
        }

        public async Task<List<UserDTO>> GetUsersAsync()
        {
           var users = await _userRepository.GetAllAsync();
            return users.Select(u => new UserDTO
            {
                Id = u.Id,
                Name = u.Name,
                Lastname = u.Lastname,
                Username = u.Username,
                Email = u.Email,
                UserType = u.UserType
            }).ToList();
        }
    }
}
