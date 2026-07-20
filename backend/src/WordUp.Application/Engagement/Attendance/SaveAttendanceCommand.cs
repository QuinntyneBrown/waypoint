using MediatR;

namespace WordUp.Application.Engagement.Attendance;

public record SaveAttendanceCommand(
    Guid SessionId,
    IReadOnlyList<AttendanceMark> Marks,
    string? IdempotencyKey = null)
    : IRequest<AttendanceTally>, IAtomicOutboxRequest;

// ExpectedVersion (base64 RowVersion from a prior read) is optional; when supplied it is checked
// against the stored row so a stale save is rejected with 409 instead of silently overwriting
// (L2-NFR-006 AC1). Omitting it preserves the offline-friendly last-write-wins upsert (L2-ENGAGE-003).
public record AttendanceMark(Guid YouthId, string Status, string? Note, string? ExpectedVersion = null);

public record AttendanceTally(int Present, int Late, int Excused, int Absent, int Marked, int Total);
