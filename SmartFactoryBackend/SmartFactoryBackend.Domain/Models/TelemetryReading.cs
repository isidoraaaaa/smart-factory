using SmartFactoryBackend.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFactoryBackend.Domain.Models
{
    public class TelemetryReading
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid MachineId { get; set; }
        public double Temperature { get; set; }
        public MachineStatus Status { get; set; } = MachineStatus.Normal;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
