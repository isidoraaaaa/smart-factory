using SmartFactoryBackend.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFactoryBackend.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> ExistsByUsernameAsync(string username);
        Task<User?> GetByUsernameAsync(string username);
        Task AddUserAsync(User user);
        Task<List<User>> GetAllAsync(); 
        Task<User?> GetByIdAsync(Guid id);   
        Task<bool> DeleteAsync(Guid id);
    }
}
