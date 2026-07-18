using MediatR;

namespace WordUp.Application.Engagement.Attendance;

public sealed record StaffSessionAttendanceQuery(Guid SessionId) : IRequest<StaffSessionAttendance>;

public sealed record StaffSessionAttendance(
    Guid SessionId,
    string Title,
    DateTime StartsAt,
    int? DurationMinutes,
    string? Location,
    Guid CohortId,
    string CohortName,
    IReadOnlyList<StaffAttendanceRosterEntry> Roster);

public sealed record StaffAttendanceRosterEntry(
    Guid YouthUserId,
    string Name,
    string? Mark,
    string? Note,
    string Version);
