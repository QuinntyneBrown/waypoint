namespace Waypoint.Application.Shared.Abstractions;

public interface IDataRetentionPolicy
{
    int DeactivatedUserRetentionDays { get; }
}
