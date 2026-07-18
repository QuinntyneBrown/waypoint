namespace WordUp.Api.Realtime;

/// <summary>
/// Canonical SignalR group-name strings so the hub (which joins connections to groups on connect) and
/// the publisher (which sends to groups) always compute identical names. L2-NFR-005.
/// </summary>
public static class RealtimeGroups
{
    public static string User(Guid userId) => $"user:{userId:N}";
    public static string Role(string role) => $"role:{role}";
    public static string Cohort(Guid cohortId) => $"cohort:{cohortId:N}";
}
