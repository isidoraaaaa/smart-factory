using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartFactoryBackend.Domain.Enums;
using SmartFactoryBackend.Domain.Models;
using SmartFactoryBackend.Infrastructure.Hubs;
using SmartFactoryBackend.Infrastructure.Persistence;

namespace SmartFactoryBackend.Infrastructure.Services;

public class TelemetrySimulatorService : BackgroundService
{
    private readonly IHubContext<TelemetryHub> _hubContext;
    private readonly ILogger<TelemetrySimulatorService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Random _random = new();

    private const double MinOptimalTemp = 30.0;
    private const double MaxOptimalTemp = 35.0;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);

    private readonly List<ChocolateMachine> _machines;

    public TelemetrySimulatorService(
        IHubContext<TelemetryHub> hubContext,
        ILogger<TelemetrySimulatorService> logger, IServiceScopeFactory scopeFactory)
    {
        _hubContext = hubContext;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _machines = new List<ChocolateMachine>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var runningMachineIds = new HashSet<Guid>();
        var tasks = new List<Task>();

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<SmartFactoryDbContext>();
                var currentMachines = await dbContext.Machines.ToListAsync(stoppingToken);

                foreach (var machine in currentMachines)
                {
                    if (!runningMachineIds.Contains(machine.Id))
                    {
                        runningMachineIds.Add(machine.Id);
                        tasks.Add(SimulateMachineTelemetryAsync(machine, stoppingToken));
                        _logger.LogInformation("Started simulating new machine: {Name}", machine.Name);
                    }
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
        }

        await Task.WhenAll(tasks);
    }

    private async Task SimulateMachineTelemetryAsync(ChocolateMachine machine, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Telemetry simulator started for {MachineName}", machine.Name);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                GenerateTelemetry(machine);
                await SaveReadingAsync(machine, stoppingToken);
                await _hubContext.Clients.All.SendAsync(
                    "ReceiveTelemetry",
                    machine,
                    cancellationToken: stoppingToken);
                _logger.LogInformation(
                    "Emitted telemetry for {MachineName}: {Temp}°C - {Status}",
                    machine.Name,
                    machine.LastTemperature,
                    machine.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while generating/emitting telemetry for {MachineName}", machine.Name);
            }
            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task SaveReadingAsync(ChocolateMachine machine, CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SmartFactoryDbContext>();

        var reading = new TelemetryReading
        {
            MachineId = machine.Id,
            Temperature = machine.LastTemperature,
            Status = machine.Status,
            Timestamp = machine.Timestamp
        };

        dbContext.Readings.Add(reading);

        var machineEntity = await dbContext.Machines.FindAsync(new object[] { machine.Id }, stoppingToken);
        if (machineEntity is not null)
        {
            machineEntity.LastTemperature = machine.LastTemperature;
            machineEntity.Status = machine.Status;
            machineEntity.Timestamp = machine.Timestamp;
        }

        await dbContext.SaveChangesAsync(stoppingToken);
    }


    private void GenerateTelemetry(ChocolateMachine machine)
    {
        double temperature = Math.Round(_random.NextDouble() * (40.0 - 25.0) + 25.0, 1);

        machine.LastTemperature = temperature;
        machine.Timestamp = DateTime.UtcNow;
        machine.Status = (temperature < MinOptimalTemp || temperature > MaxOptimalTemp)
            ? MachineStatus.Warning
            : MachineStatus.Normal;
    }
}