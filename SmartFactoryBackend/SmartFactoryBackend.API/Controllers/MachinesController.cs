using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartFactoryBackend.Application.Interfaces;
using SmartFactoryBackend.Infrastructure.Persistence;

namespace SmartFactoryBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MachinesController : ControllerBase
    {
        private readonly IMachineService _machineService;

        public MachinesController(IMachineService machineService)
        {
            _machineService = machineService;
        }

        // GET /api/machines
        [HttpGet]
        public async Task<IActionResult> GetMachines()
        {
            var machines = await _machineService.GetMachinesAsync();
            return Ok(machines);
        }

        // GET /api/machines/{id}/history?take=5
        [HttpGet("{id:guid}/history")]
        public async Task<IActionResult> GetHistory(Guid id, [FromQuery] int take = 5)
        {
            var history = await _machineService.GetHistoryAsync(id,take);
            if (history == null)
            {
                return NotFound($"Machine with id {id} not found.");
            }

            return Ok(history);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddMachine([FromBody] AddMachineRequest request)
        {
            var machine = await _machineService.AddMachineAsync(request.Name);
            return CreatedAtAction(nameof(GetMachines), new { id = machine.Id }, machine);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMachine(Guid id)
        {
            var deleted = await _machineService.DeleteMachineAsync(id);
            return deleted ? NoContent() : NotFound($"Machine with id {id} not found.");
        }

        public record AddMachineRequest(string Name);
    }
}
