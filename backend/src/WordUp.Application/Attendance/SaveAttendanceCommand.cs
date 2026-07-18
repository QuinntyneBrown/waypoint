using MediatR;

namespace WordUp.Application.Attendance;

public record SaveAttendanceCommand(Guid SessionId, IReadOnlyList<AttendanceMark> Marks)
    : IRequest<AttendanceTally>;

public record AttendanceMark(Guid YouthId, string Status, string? Note);

public record AttendanceTally(int Present, int Late, int Excused, int Absent, int Marked, int Total);
