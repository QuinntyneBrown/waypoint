using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Engagement.Attendance;

public class AttendanceHistoryByYouthQueryHandler
    : IRequestHandler<AttendanceHistoryByYouthQuery, YouthAttendanceSummary>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public AttendanceHistoryByYouthQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<YouthAttendanceSummary> Handle(
        AttendanceHistoryByYouthQuery request, CancellationToken cancellationToken)
    {
        var youth = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == request.YouthId
                && user.Role == Roles.Youth, cancellationToken)
            ?? throw new NotFoundException("Youth not found.");
        if (_currentUser.Role == Roles.Mentor && youth.MentorId != _currentUser.UserId)
        {
            throw new ForbiddenException("This youth is not assigned to you.");
        }

        var records = _db.AttendanceRecords.AsNoTracking()
            .Where(record => record.YouthUserId == request.YouthId);
        if (_currentUser.Role == Roles.Mentor)
        {
            var mentorId = _currentUser.UserId!.Value;
            records = records.Where(record => _db.Sessions.Any(session =>
                session.Id == record.SessionId && session.MentorId == mentorId));
        }
        if (request.From is { } from)
        {
            records = records.Where(record => _db.Sessions.Any(session =>
                session.Id == record.SessionId && session.StartsAt >= from));
        }
        if (request.To is { } to)
        {
            records = records.Where(record => _db.Sessions.Any(session =>
                session.Id == record.SessionId && session.StartsAt <= to));
        }
        var statuses = await records
            .OrderBy(record => _db.Sessions.Where(session => session.Id == record.SessionId)
                .Select(session => session.StartsAt)
                .FirstOrDefault())
            .Select(record => record.Status)
            .ToListAsync(cancellationToken);

        var late = statuses.Count(s => s == AttendanceStatus.Late);
        var excused = statuses.Count(s => s == AttendanceStatus.Excused);
        var absent = statuses.Count(s => s == AttendanceStatus.Absent);
        var attended = statuses.Count(s => s is AttendanceStatus.Present or AttendanceStatus.Late);
        var total = statuses.Count;

        var rate = total > 0 ? (double)attended / total : 0d;
        // Attendance concern threshold (L2-ENGAGE-004): below 70% attended flags a follow-up.
        var belowThreshold = total > 0 && rate < 0.7;
        var trend = AttendanceTrend(statuses);

        return new YouthAttendanceSummary(
            request.YouthId,
            attended,
            late,
            excused,
            absent,
            total,
            rate,
            belowThreshold,
            youth.DisplayName,
            trend);
    }

    internal static string AttendanceTrend(IReadOnlyList<AttendanceStatus> statuses)
    {
        if (statuses.Count < 2) return statuses.Count == 0 ? "no data" : "steady";
        var split = statuses.Count / 2;
        var earlier = statuses.Take(split).Count(Attended) / (double)split;
        var laterCount = statuses.Count - split;
        var later = statuses.Skip(split).Count(Attended) / (double)laterCount;
        if (later > earlier + 0.1) return "improving";
        if (later < earlier - 0.1) return "declining";
        return "steady";
    }

    private static bool Attended(AttendanceStatus status) =>
        status is AttendanceStatus.Present or AttendanceStatus.Late;
}
