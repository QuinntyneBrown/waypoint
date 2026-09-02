using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Api.Realtime;

/// <summary>
/// SignalR adapter for <see cref="IChangePublisher"/> (L2-NFR-005). Each dispatch loads current user
/// state for the connected identities and sends directly only to connections whose account, session,
/// role, and requested audience are still authorized. Lives in the presentation layer so Application
/// stays transport-agnostic; registered against the port in Program.cs.
/// </summary>
public sealed class SignalRChangePublisher : IChangePublisher
{
    private readonly IHubContext<RealtimeHub> _hub;
    private readonly RealtimeConnectionRegistry _connections;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IClock _clock;

    public SignalRChangePublisher(
        IHubContext<RealtimeHub> hub,
        RealtimeConnectionRegistry connections,
        IServiceScopeFactory scopeFactory,
        IClock clock)
    {
        _hub = hub;
        _connections = connections;
        _scopeFactory = scopeFactory;
        _clock = clock;
    }

    public Task PublishToUserAsync(Guid userId, string eventName, object payload, CancellationToken cancellationToken = default)
        => PublishAsync(
            state => state.Id == userId,
            eventName,
            payload,
            cancellationToken);

    public Task PublishToRoleAsync(string role, string eventName, object payload, CancellationToken cancellationToken = default)
        => PublishAsync(
            state => string.Equals(state.Role, role, StringComparison.Ordinal),
            eventName,
            payload,
            cancellationToken);

    public Task PublishToCohortAsync(Guid cohortId, string eventName, object payload, CancellationToken cancellationToken = default)
        => PublishAsync(
            state => state.CohortId == cohortId,
            eventName,
            payload,
            cancellationToken);

    private async Task PublishAsync(
        Func<CurrentRealtimeUser, bool> isInAudience,
        string eventName,
        object payload,
        CancellationToken cancellationToken)
    {
        var registrations = _connections.Snapshot();
        if (registrations.Count == 0)
        {
            return;
        }

        var connectedUserIds = registrations
            .Select(item => item.UserId)
            .Distinct()
            .ToList();
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var currentUsers = await db.Users
            .AsNoTracking()
            .Where(item => connectedUserIds.Contains(item.Id))
            .Select(item => new CurrentRealtimeUser(
                item.Id,
                item.SessionVersion,
                item.Role,
                item.Status,
                item.CohortId,
                item.PersonalDataAnonymizedAt))
            .ToDictionaryAsync(item => item.Id, cancellationToken);

        var recipients = new List<string>(registrations.Count);
        foreach (var registration in registrations)
        {
            if (registration.ExpiresAtUtc <= _clock.UtcNow
                || !currentUsers.TryGetValue(registration.UserId, out var current)
                || current.Status != UserStatus.Active
                || current.PersonalDataAnonymizedAt is not null
                || current.SessionVersion != registration.SessionVersion
                || !string.Equals(current.Role, registration.Role, StringComparison.Ordinal))
            {
                _connections.Remove(registration.ConnectionId);
                continue;
            }

            if (isInAudience(current))
            {
                recipients.Add(registration.ConnectionId);
            }
        }

        if (recipients.Count > 0)
        {
            await _hub.Clients.Clients(recipients)
                .SendAsync(eventName, payload, cancellationToken);
        }
    }

    private sealed record CurrentRealtimeUser(
        Guid Id,
        int SessionVersion,
        string Role,
        UserStatus Status,
        Guid? CohortId,
        DateTime? PersonalDataAnonymizedAt);
}
