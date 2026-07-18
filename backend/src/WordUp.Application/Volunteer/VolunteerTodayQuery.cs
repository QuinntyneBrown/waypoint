using MediatR;

namespace WordUp.Application.Volunteer;

/// <summary>The current volunteer's session dated today: logistics + roster count only, with no youth
/// records (L2-VOLUNTEER-001 AC1). Null when the volunteer has no session today.</summary>
public record VolunteerTodayQuery() : IRequest<VolunteerTodayView?>;

public record VolunteerTodayView(
    Guid SessionId,
    string Title,
    DateTime StartsAt,
    int? DurationMinutes,
    string? Location,
    int RosterCount);
