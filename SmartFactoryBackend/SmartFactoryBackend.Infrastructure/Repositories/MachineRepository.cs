using Microsoft.EntityFrameworkCore;
using SmartFactoryBackend.Application.Interfaces;
using SmartFactoryBackend.Domain.Models;
using SmartFactoryBackend.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace SmartFactoryBackend.Infrastructure.Repositories
{
    public class MachineRepository : IMachineRepository
    {
        SmartFactoryDbContext _dbContext;
        public MachineRepository(SmartFactoryDbContext dbContext) 
        { 
            _dbContext = dbContext;

        }

        public async Task<List<ChocolateMachine>> GetAllAsync()
        {
             return await _dbContext.Machines.ToListAsync();
        }


        public async Task<ChocolateMachine?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Machines.FindAsync(id);
        }

        public async Task<List<TelemetryReading>> GetHistoryAsync(Guid machineId, int take)
        {
            return await _dbContext.Readings
                        .Where(r => r.MachineId == machineId)
                        .OrderByDescending(r => r.Timestamp)
                        .Take(take)
                        .ToListAsync();
        }
    }
}
