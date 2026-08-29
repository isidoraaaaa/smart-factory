using ChocolateFactoryBackend.Application.DTO;
using SmartFactoryBackend.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFactoryBackend.Application.Interfaces
{
    public interface IMachineService
    {
        Task<List<ChocolateMachine>> GetMachinesAsync();
        Task<List<TelemetryReadingDTO>?> GetHistoryAsync(Guid machineId, int take);
        Task<ChocolateMachine> AddMachineAsync(string name);  
        Task<bool> DeleteMachineAsync(Guid id);
    }
}
