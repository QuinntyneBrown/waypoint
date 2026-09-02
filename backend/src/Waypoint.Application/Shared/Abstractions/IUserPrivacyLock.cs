namespace Waypoint.Application.Shared.Abstractions;

/// <summary>
/// Serializes account-directed notification and email work with privacy correction/removal.
/// Relational implementations own the lock through the current transaction, not merely through
/// this method call, so callers must execute inside an atomic transaction.
/// </summary>
public interface IUserPrivacyLock
{
    Task AcquireAsync(Guid userId, CancellationToken cancellationToken = default);
}
