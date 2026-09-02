using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Infrastructure;

public class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
