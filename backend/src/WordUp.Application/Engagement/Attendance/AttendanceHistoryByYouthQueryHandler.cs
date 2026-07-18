using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.Engagement.Attendance;

public class AttendanceHistoryByYouthQueryHandler
    : IRequestHandler<AttendanceHistoryByYouthQuery, YouthAttendanceSummary>
{
    private readonly IAppDbContext _db;

    public AttendanceHistoryByYouthQueryHandler(IAppDbContext db) => _db = db;

    public async Task<YouthAttendanceSummary> Handle(
        AttendanceHistoryByYouthQuery request, CancellationToken cancellationToken)
    {
        var statuses = await _db.AttendanceRecords
            .Where(r => r.YouthUserId == request.YouthId)
            .Select(r => r.Status)
            .ToListAsync(cancellationToken);

        var late = statuses.Count(s => s == AttendanceStatus.Late);
        var excused = statuses.Count(s => s == AttendanceStatus.Excused);
        var absent = statuses.Count(s => s == AttendanceStatus.Absent);
        var attended = statuses.Count(s => s is AttendanceStatus.Present or AttendanceStatus.Late);
        var total = statuses.Count;

        var rate = total > 0 ? (double)attended / total : 0d;
        // Attendance concern threshold (L2-ENGAGE-004): below 70% attended flags a follow-up.
        var belowThreshold = rate < 0.7;

        return new YouthAttendanceSummary(
            request.YouthId, attended, late, excused, absent, total, rate, belowThreshold);
    }
}
