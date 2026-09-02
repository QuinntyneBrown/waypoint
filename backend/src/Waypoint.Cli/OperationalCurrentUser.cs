using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Cli;

public sealed class OperationalCurrentUser : ICurrentUser
{
    public Guid? UserId { get; private set; }
    public string? Role { get; private set; }
    public int? SessionVersion => null;
    public bool IsAuthenticated => UserId.HasValue;

    public void Set(Guid userId, string role)
    {
        UserId = userId;
        Role = role;
    }
}
