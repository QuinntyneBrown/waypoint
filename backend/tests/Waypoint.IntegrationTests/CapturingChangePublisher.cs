using System.Collections.Concurrent;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Api.Realtime;

namespace Waypoint.IntegrationTests;

public sealed record CapturedChange(
    string Audience,
    Guid? AudienceId,
    string? AudienceName,
    string EventName,
    object Payload);

public sealed class CapturingChangePublisher : IChangePublisher
{
    private readonly ConcurrentQueue<CapturedChange> _changes = new();
    private readonly SignalRChangePublisher _inner;

    public CapturingChangePublisher(SignalRChangePublisher inner) => _inner = inner;

    public IReadOnlyList<CapturedChange> Changes => _changes.ToArray();

    public Task PublishToUserAsync(
        Guid userId,
        string eventName,
        object payload,
        CancellationToken cancellationToken = default)
    {
        _changes.Enqueue(new CapturedChange("user", userId, null, eventName, payload));
        return _inner.PublishToUserAsync(userId, eventName, payload, cancellationToken);
    }

    public Task PublishToRoleAsync(
        string role,
        string eventName,
        object payload,
        CancellationToken cancellationToken = default)
    {
        _changes.Enqueue(new CapturedChange("role", null, role, eventName, payload));
        return _inner.PublishToRoleAsync(role, eventName, payload, cancellationToken);
    }

    public Task PublishToCohortAsync(
        Guid cohortId,
        string eventName,
        object payload,
        CancellationToken cancellationToken = default)
    {
        _changes.Enqueue(new CapturedChange("cohort", cohortId, null, eventName, payload));
        return _inner.PublishToCohortAsync(cohortId, eventName, payload, cancellationToken);
    }
}
