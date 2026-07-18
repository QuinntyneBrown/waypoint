using MediatR;

namespace WordUp.Application.Engagement.Attendance;

public record AttendanceHistoryByYouthQuery(Guid YouthId) : IRequest<YouthAttendanceSummary>;

public record YouthAttendanceSummary(
    Guid YouthUserId,
    int Attended,
    int Late,
    int Excused,
    int Absent,
    int Total,
    double Rate,
    bool BelowThreshold);
