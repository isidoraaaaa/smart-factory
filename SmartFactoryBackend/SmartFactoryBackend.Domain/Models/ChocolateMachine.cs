// Domain/Models/ChocolateMachine.cs
using SmartFactoryBackend.Domain.Enums;

namespace SmartFactoryBackend.Domain.Models;

public class ChocolateMachine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public double LastTemperature { get; set; }
    public MachineStatus Status { get; set; } = MachineStatus.Normal;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public ICollection<TelemetryReading> Readings { get; set; } = new List<TelemetryReading>();
}