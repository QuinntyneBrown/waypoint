namespace Waypoint.Api.Contracts.TestSupport;

/// <summary>Testing-only request for a populated same-day volunteer assignment.</summary>
public sealed record SeedVolunteerSessionBody(Guid VolunteerUserId, int DaysFromToday = 0);
