using WordUp.Application.Shared.Abstractions;

namespace WordUp.Cli;

public sealed class OperationalCurrentUser : ICurrentUser
{
    public Guid? UserId { get; private set; }
    public string? Role { get; private set; }
    public bool IsAuthenticated => UserId.HasValue;

    public void Set(Guid userId, string role)
    {
        UserId = userId;
        Role = role;
    }
}
