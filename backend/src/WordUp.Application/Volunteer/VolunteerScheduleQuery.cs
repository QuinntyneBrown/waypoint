using MediatR;

namespace WordUp.Application.Volunteer;

/// <summary>The current volunteer's upcoming assigned sessions, each with a derived status
/// ("upcoming" or "attendance-taken") (L2-VOLUNTEER-002 AC1). Empty list when there are none (AC2).</summary>
public record VolunteerScheduleQuery() : IRequest<IReadOnlyList<VolunteerScheduleItem>>;

public record VolunteerScheduleItem(
    Guid SessionId,
    string Title,
    DateTime StartsAt,
    int? DurationMinutes,
    string? Location,
    string Status);
