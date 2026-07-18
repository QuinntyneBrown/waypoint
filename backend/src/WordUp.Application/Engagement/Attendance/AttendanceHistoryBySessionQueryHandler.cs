using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Engagement.Attendance;

public class AttendanceHistoryBySessionQueryHandler
    : IRequestHandler<AttendanceHistoryBySessionQuery, IReadOnlyList<SessionAttendanceSummary>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public AttendanceHistoryBySessionQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<SessionAttendanceSummary>> Handle(
        AttendanceHistoryBySessionQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException();
        IQueryable<Session> sessionQuery = _db.Sessions.AsNoTracking();
        if (_currentUser.Role == Roles.Mentor)
        {
            sessionQuery = sessionQuery.Where(session => session.MentorId == userId);
        }

        if (request.CohortId is { } cohortId)
        {
            sessionQuery = sessionQuery.Where(s => s.CohortId == cohortId);
        }
        if (request.From is { } from)
        {
            sessionQuery = sessionQuery.Where(s => s.StartsAt >= from);
        }
        if (request.To is { } to)
        {
            sessionQuery = sessionQuery.Where(s => s.StartsAt <= to);
        }

        var sessions = await sessionQuery.OrderBy(s => s.StartsAt).ToListAsync(cancellationToken);
        var sessionIds = sessions.Select(s => s.Id).ToList();
        var cohortIds = sessions.Select(session => session.CohortId).Distinct().ToList();
        var cohortNames = await _db.Cohorts.AsNoTracking()
            .Where(cohort => cohortIds.Contains(cohort.Id))
            .ToDictionaryAsync(cohort => cohort.Id, cohort => cohort.Name, cancellationToken);
        var rosterCounts = await _db.Users.AsNoTracking()
            .Where(user => user.CohortId != null
                && cohortIds.Contains(user.CohortId.Value)
                && user.Role == Roles.Youth
                && user.Status == UserStatus.Active)
            .GroupBy(user => user.CohortId!.Value)
            .Select(group => new { CohortId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => row.CohortId, row => row.Count, cancellationToken);

        var records = await _db.AttendanceRecords
            .Where(r => sessionIds.Contains(r.SessionId))
            .Select(r => new { r.SessionId, r.Status })
            .ToListAsync(cancellationToken);

        var summaries = new List<SessionAttendanceSummary>(sessions.Count);
        foreach (var session in sessions)
        {
            var marks = records.Where(r => r.SessionId == session.Id).ToList();
            var marked = marks.Count;
            var present = marks.Count(r => r.Status == AttendanceStatus.Present);
            var attended = marks.Count(r =>
                r.Status is AttendanceStatus.Present or AttendanceStatus.Late);
            var rosterTotal = rosterCounts.GetValueOrDefault(session.CohortId);
            var rate = rosterTotal > 0 ? (double)attended / rosterTotal : 0d;
            var status = marked < rosterTotal
                ? "needs review"
                : rate < 0.5
                    ? "low turnout"
                    : "saved";
            summaries.Add(new SessionAttendanceSummary(
                session.Id,
                session.Title,
                session.StartsAt,
                present,
                marked,
                rate,
                status,
                session.CohortId,
                cohortNames.GetValueOrDefault(session.CohortId, "Cohort"),
                rosterTotal,
                session.Location));
        }

        return summaries;
    }
}
