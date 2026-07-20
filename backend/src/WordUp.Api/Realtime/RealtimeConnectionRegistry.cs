using System.Collections.Concurrent;

namespace WordUp.Api.Realtime;

/// <summary>
/// Tracks authenticated SignalR connections without treating their connect-time authorization as
/// durable. <see cref="SignalRChangePublisher"/> revalidates every snapshot against current user
/// state before sending an event.
/// </summary>
public sealed class RealtimeConnectionRegistry
{
    private readonly ConcurrentDictionary<string, RealtimeConnectionRegistration> _connections = new();

    internal void Register(
        string connectionId,
        Guid userId,
        int sessionVersion,
        string role,
        DateTime expiresAtUtc) =>
        _connections[connectionId] = new RealtimeConnectionRegistration(
            connectionId,
            userId,
            sessionVersion,
            role,
            expiresAtUtc);

    internal void Remove(string connectionId) => _connections.TryRemove(connectionId, out _);

    internal IReadOnlyList<RealtimeConnectionRegistration> Snapshot() =>
        _connections.Values.ToArray();
}

internal sealed record RealtimeConnectionRegistration(
    string ConnectionId,
    Guid UserId,
    int SessionVersion,
    string Role,
    DateTime ExpiresAtUtc);
