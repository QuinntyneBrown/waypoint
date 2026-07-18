using MediatR;

namespace WordUp.Application.Attendance;

public record SessionAttendanceQuery(Guid SessionId) : IRequest<IReadOnlyList<AttendanceMarkView>>;

// Version lets a client echo the value back on the next save so concurrent edits are detected and a
// stale write is rejected (L2-NFR-006).
public record AttendanceMarkView(Guid YouthUserId, string Status, string? Note, DateTime RecordedAt, string Version);
