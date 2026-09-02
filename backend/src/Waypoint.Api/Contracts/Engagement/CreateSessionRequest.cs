namespace Waypoint.Api.Contracts.Engagement;

public record CreateSessionRequest(
    Guid CohortId,
    Guid? MentorId,
    string Title,
    DateTime StartsAt,
    int? DurationMinutes,
    string? Location,
    bool RepeatsWeekly);
