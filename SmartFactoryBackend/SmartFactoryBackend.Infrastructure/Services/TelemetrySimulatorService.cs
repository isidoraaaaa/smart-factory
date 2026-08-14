using SmartFactoryBackend.Domain.Enums;
using SmartFactoryBackend.Domain.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartFactoryBackend.Infrastructure.Hubs;

namespace SmartFactoryBackend.Infrastructure.Services;

public class TelemetrySimulatorService : BackgroundService
{
    private readonly IHubContext<TelemetryHub> _hubContext;
    private readonly ILogger<TelemetrySimulatorService> _logger;
    private readonly Random _random = new();

    private const double MinOptimalTemp = 30.0;
    private const double MaxOptimalTemp = 35.0;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(3);

    // Simulacija jedne mašine na traci; lako proširivo na listu mašina
    private readonly ChocolateMachine _machine = new()
    {
        Name = "Chocolate Line #1"
    };

    public TelemetrySimulatorService(
        IHubContext<TelemetryHub> hubContext,
        ILogger<TelemetrySimulatorService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Telemetry simulator started for {MachineName}", _machine.Name);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                GenerateTelemetry();

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

    private void GenerateTelemetry()
    {
        // Generiši temperaturu u širem opsegu (npr. 25–40°C) da bi se
        // povremeno prirodno dobijalo i stanje Warning
        double temperature = Math.Round(_random.NextDouble() * (40.0 - 25.0) + 25.0, 1);

        _machine.LastTemperature = temperature;
        _machine.Timestamp = DateTime.UtcNow;
        _machine.Status = (temperature < MinOptimalTemp || temperature > MaxOptimalTemp)
            ? MachineStatus.Warning
            : MachineStatus.Normal;
    }
}