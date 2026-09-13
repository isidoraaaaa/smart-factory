using Microsoft.EntityFrameworkCore;
using SmartFactoryBackend.Application.Interfaces;
using SmartFactoryBackend.Domain.Models;
using SmartFactoryBackend.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFactoryBackend.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly SmartFactoryDbContext _dbContext;

        public UserRepository(SmartFactoryDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddUserAsync(User user)
        {
             _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user is null) return false;
            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            return await _dbContext.Users
                .AnyAsync(u => u.Username == username);
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _dbContext.Users.ToListAsync();
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Users.FindAsync(id);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

      
    }
}
