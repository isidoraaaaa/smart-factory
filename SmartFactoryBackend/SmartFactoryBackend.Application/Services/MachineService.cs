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
    }
}
