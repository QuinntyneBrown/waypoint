using Microsoft.AspNetCore.SignalR;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Api.Realtime;

/// <summary>
/// SignalR adapter for <see cref="IChangePublisher"/> (L2-NFR-005). Maps user/role/cohort targets to
/// the canonical <see cref="RealtimeGroups"/> names and pushes to those groups only. Lives in the
/// presentation layer (it depends on <see cref="IHubContext{THub}"/>) so the Application stays
/// transport-agnostic; registered against the port in Program.cs.
/// </summary>
public sealed class SignalRChangePublisher : IChangePublisher
{
    private readonly IHubContext<RealtimeHub> _hub;

    public SignalRChangePublisher(IHubContext<RealtimeHub> hub) => _hub = hub;

    public Task PublishToUserAsync(Guid userId, string eventName, object payload, CancellationToken cancellationToken = default)
        => _hub.Clients.Group(RealtimeGroups.User(userId)).SendAsync(eventName, payload, cancellationToken);

    public Task PublishToRoleAsync(string role, string eventName, object payload, CancellationToken cancellationToken = default)
        => _hub.Clients.Group(RealtimeGroups.Role(role)).SendAsync(eventName, payload, cancellationToken);

    public Task PublishToCohortAsync(Guid cohortId, string eventName, object payload, CancellationToken cancellationToken = default)
        => _hub.Clients.Group(RealtimeGroups.Cohort(cohortId)).SendAsync(eventName, payload, cancellationToken);
}
