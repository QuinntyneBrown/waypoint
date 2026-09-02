using MediatR;

namespace Waypoint.Application.Engagement.Sessions;

public record SessionCalendarQuery(Guid? CohortId, DateTime? From, DateTime? To)
    : IRequest<IReadOnlyList<SessionView>>;

public record SessionView(
    Guid Id,
    Guid CohortId,
    Guid? MentorId,
    string Title,
    DateTime StartsAt,
    int? DurationMinutes,
    string? Location,
    string CohortName,
    string MentorName,
    bool AttendanceTaken,
    int RosterCount);
