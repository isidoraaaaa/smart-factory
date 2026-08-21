using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocolateFactoryBackend.Application.DTO;

public record TelemetryReadingDTO(
    Guid Id,
    double Temperature,
    string Status,
    DateTime Timestamp
);

