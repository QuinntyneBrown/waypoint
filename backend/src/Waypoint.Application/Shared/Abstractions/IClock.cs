namespace Waypoint.Application.Shared.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
}
