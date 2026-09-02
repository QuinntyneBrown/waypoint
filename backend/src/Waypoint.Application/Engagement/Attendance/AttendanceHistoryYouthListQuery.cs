using MediatR;

namespace Waypoint.Application.Engagement.Attendance;

public sealed record AttendanceHistoryYouthListQuery(
    Guid? CohortId,
    DateTime? From,
    DateTime? To) : IRequest<IReadOnlyList<YouthAttendanceSummary>>;
