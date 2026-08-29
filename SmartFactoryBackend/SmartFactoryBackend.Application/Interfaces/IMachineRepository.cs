using SmartFactoryBackend.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace SmartFactoryBackend.Application.Interfaces
{
    public interface IMachineRepository
    {
        Task<List<ChocolateMachine>> GetAllAsync();
        Task<ChocolateMachine?> GetByIdAsync(Guid id);
        Task<List<TelemetryReading>> GetHistoryAsync(Guid machineId, int take);
        Task<bool> ExistsByNameAsync(string name);
        Task AddAsync(ChocolateMachine machine);          
        Task<bool> DeleteAsync(Guid id);
    }
}
