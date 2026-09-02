using MediatR;

namespace Waypoint.Application.Engagement.Attendance;

public record AttendanceHistoryBySessionQuery(Guid? CohortId, DateTime? From, DateTime? To)
    : IRequest<IReadOnlyList<SessionAttendanceSummary>>;

public record SessionAttendanceSummary(
    Guid SessionId,
    string Title,
    DateTime StartsAt,
    int Present,
    int Marked,
    double Rate,
    string Status,
    Guid CohortId,
    string CohortName,
    int RosterTotal,
    string? Location);
