// Infrastructure/Hubs/TelemetryHub.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SmartFactoryBackend.Infrastructure.Hubs;

[Authorize]
public class TelemetryHub : Hub
{
}