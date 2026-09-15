using ChocolateFactoryBackend.Application.DTO;
using SmartFactoryBackend.Application.Interfaces;
using SmartFactoryBackend.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFactoryBackend.Application.Services
{
    public class MachineService : IMachineService
    {
        private readonly IMachineRepository _machineRepository;
        public MachineService(IMachineRepository machineRepository)
        {
            _machineRepository = machineRepository;
        }

        public async Task<List<TelemetryReadingDTO>?> GetHistoryAsync(Guid machineId, int take)
        {
            var machine = await _machineRepository.GetByIdAsync(machineId);
            if (machine == null)
            {
                return null;
            }

            var history =  await _machineRepository.GetHistoryAsync(machineId, take);
            return history.Select(r => new TelemetryReadingDTO(
                            r.Id,
                            r.Temperature,
                            r.Status.ToString(),
                            r.Timestamp
                        )).ToList();
        }

        public async Task<List<ChocolateMachine>> GetMachinesAsync()
        {
            return await _machineRepository.GetAllAsync();
        }

        public async Task<ChocolateMachine?> GetMachineAsync(Guid id)
        {
            return await _machineRepository.GetByIdAsync(id);
        }

        public async Task<ChocolateMachine> AddMachineAsync(string name)
        {
            var nameTaken = await _machineRepository.ExistsByNameAsync(name);

            if (nameTaken)
            {
                throw new InvalidOperationException("Name is already taken.");
            }

            var machine = new ChocolateMachine
            {
                Name = name
            };

            await _machineRepository.AddAsync(machine);
            return machine;
        }

        public async Task<bool> DeleteMachineAsync(Guid id)
        {
            return await _machineRepository.DeleteAsync(id);
        }
    }
}
