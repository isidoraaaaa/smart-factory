using SmartFactoryBackend.Application.DTO;
using SmartFactoryBackend.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFactoryBackend.Application.Interfaces
{
    public interface IUserService
    {
        Task<List<UserDTO>> GetUsersAsync();
        Task<UserDTO?> GetUserAsync(Guid id);
        Task<bool> DeleteUserAsync(Guid id, Guid requestingUserId);
    }
}
