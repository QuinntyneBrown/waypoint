namespace Waypoint.Application.Shared.Abstractions;

/// <summary>
/// Writes append-only audit entries (L2-CONSENT-003). The implementation captures the request source
/// (device + location) itself; callers supply the action and target. Never pass private note content.
/// </summary>
public interface IAuditLogger
{
    Task LogAsync(
        string action,
        Guid? actorUserId = null,
        string? targetType = null,
        Guid? targetId = null,
        bool isPrivate = false,
        string? details = null,
        CancellationToken cancellationToken = default);
}
