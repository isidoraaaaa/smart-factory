using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartFactoryBackend.Infrastructure.Persistence;

namespace SmartFactoryBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MachinesController : ControllerBase
    {
        private readonly SmartFactoryDbContext _dbContext;

        public MachinesController(SmartFactoryDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET /api/machines
        [HttpGet]
        public async Task<IActionResult> GetMachines()
        {
            var machines = await _dbContext.Machines.ToListAsync();
            return Ok(machines);
        }

        // GET /api/machines/{id}/history?take=5
        [HttpGet("{id:guid}/history")]
        public async Task<IActionResult> GetHistory(Guid id, [FromQuery] int take = 5)
        {
            var machineExists = await _dbContext.Machines.AnyAsync(m => m.Id == id);
            if (!machineExists)
            {
                return NotFound($"Machine with id {id} not found.");
            }

            var history = await _dbContext.Readings
                .Where(r => r.MachineId == id)
                .OrderByDescending(r => r.Timestamp)
                .Take(take)
                .ToListAsync();

            return Ok(history);
        }

    }
}
