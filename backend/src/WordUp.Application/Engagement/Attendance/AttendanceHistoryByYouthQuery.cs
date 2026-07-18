using MediatR;

namespace WordUp.Application.Engagement.Attendance;

public record AttendanceHistoryByYouthQuery(Guid YouthId, DateTime? From = null, DateTime? To = null)
    : IRequest<YouthAttendanceSummary>;

public record YouthAttendanceSummary(
    Guid YouthUserId,
    int Attended,
    int Late,
    int Excused,
    int Absent,
    int Total,
    double Rate,
    bool BelowThreshold,
    string DisplayName,
    string Trend);
