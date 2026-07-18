namespace WordUp.Application.Shared.Abstractions;

/// <summary>
/// Port for pushing near-real-time events to connected clients, scoped to the recipient's identity
/// (L2-NFR-005). Application handlers depend only on this seam; the SignalR adapter that fans events
/// out to identity-scoped groups (user / role / cohort) lives in the presentation layer so the
/// Application stays transport-agnostic. Payloads must contain only data the recipient may already see.
/// </summary>
public interface IChangePublisher
{
    Task PublishToUserAsync(Guid userId, string eventName, object payload, CancellationToken cancellationToken = default);
    Task PublishToRoleAsync(string role, string eventName, object payload, CancellationToken cancellationToken = default);
    Task PublishToCohortAsync(Guid cohortId, string eventName, object payload, CancellationToken cancellationToken = default);
}
