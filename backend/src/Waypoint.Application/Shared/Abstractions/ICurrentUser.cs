namespace Waypoint.Application.Shared.Abstractions;

public interface ICurrentUser
{
    Guid? UserId { get; }
    string? Role { get; }
    int? SessionVersion { get; }
    bool IsAuthenticated { get; }
}
