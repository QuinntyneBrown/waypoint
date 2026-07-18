using MediatR;

namespace WordUp.Application.Attendance;

public record SessionAttendanceQuery(Guid SessionId) : IRequest<IReadOnlyList<AttendanceMarkView>>;

public record AttendanceMarkView(Guid YouthUserId, string Status, string? Note, DateTime RecordedAt);
