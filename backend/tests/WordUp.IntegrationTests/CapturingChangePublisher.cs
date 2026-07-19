using System.Collections.Concurrent;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.IntegrationTests;

public sealed record CapturedChange(
    string Audience,
    Guid? AudienceId,
    string? AudienceName,
    string EventName,
    object Payload);

public sealed class CapturingChangePublisher : IChangePublisher
{
    private readonly ConcurrentQueue<CapturedChange> _changes = new();
    public IReadOnlyList<CapturedChange> Changes => _changes.ToArray();

    public Task PublishToUserAsync(
        Guid userId,
        string eventName,
        object payload,
        CancellationToken cancellationToken = default)
    {
        _changes.Enqueue(new CapturedChange("user", userId, null, eventName, payload));
        return Task.CompletedTask;
    }

    public Task PublishToRoleAsync(
        string role,
        string eventName,
        object payload,
        CancellationToken cancellationToken = default)
    {
        _changes.Enqueue(new CapturedChange("role", null, role, eventName, payload));
        return Task.CompletedTask;
    }

    public Task PublishToCohortAsync(
        Guid cohortId,
        string eventName,
        object payload,
        CancellationToken cancellationToken = default)
    {
        _changes.Enqueue(new CapturedChange("cohort", cohortId, null, eventName, payload));
        return Task.CompletedTask;
    }
}
