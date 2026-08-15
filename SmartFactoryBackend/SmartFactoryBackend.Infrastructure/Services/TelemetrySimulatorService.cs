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
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(3);

    private readonly ChocolateMachine _machine = new()
    {
        Name = "Chocolate Line #1"
    };

    public TelemetrySimulatorService(
        IHubContext<TelemetryHub> hubContext,
        ILogger<TelemetrySimulatorService> logger, IServiceScopeFactory scopeFactory)
    {
        _hubContext = hubContext;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Telemetry simulator started for {MachineName}", _machine.Name);

        await EnsureMachineExistsAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                GenerateTelemetry();

                await SaveReadingAsync(stoppingToken);

                await _hubContext.Clients.All.SendAsync(
                    "ReceiveTelemetry",
                    _machine,
                    cancellationToken: stoppingToken);

                _logger.LogInformation(
                    "Emitted telemetry: {Temp}°C - {Status}",
                    _machine.LastTemperature,
                    _machine.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while generating/emitting telemetry");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task EnsureMachineExistsAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SmartFactoryDbContext>();

        var existing = await dbContext.Machines
            .FirstOrDefaultAsync(m => m.Name == _machine.Name, stoppingToken);

        if (existing is null)
        {
            dbContext.Machines.Add(_machine);
            await dbContext.SaveChangesAsync(stoppingToken);
        }
        else
        {
            // Koristi postojeći Id iz baze da FK veza radi ispravno
            _machine.Id = existing.Id;
        }
    }

    private async Task SaveReadingAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SmartFactoryDbContext>();

        var reading = new TelemetryReading
        {
            MachineId = _machine.Id,
            Temperature = _machine.LastTemperature,
            Status = _machine.Status,
            Timestamp = _machine.Timestamp
        };

        dbContext.Readings.Add(reading);

        var machineEntity = await dbContext.Machines.FindAsync(new object[] { _machine.Id }, stoppingToken);
        if (machineEntity is not null)
        {
            machineEntity.LastTemperature = _machine.LastTemperature;
            machineEntity.Status = _machine.Status;
            machineEntity.Timestamp = _machine.Timestamp;
        }

        await dbContext.SaveChangesAsync(stoppingToken);
    }


    private void GenerateTelemetry()
    {
        double temperature = Math.Round(_random.NextDouble() * (40.0 - 25.0) + 25.0, 1);

        _machine.LastTemperature = temperature;
        _machine.Timestamp = DateTime.UtcNow;
        _machine.Status = (temperature < MinOptimalTemp || temperature > MaxOptimalTemp)
            ? MachineStatus.Warning
            : MachineStatus.Normal;
    }
}