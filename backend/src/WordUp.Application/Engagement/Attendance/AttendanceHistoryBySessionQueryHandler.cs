using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.Engagement.Attendance;

public class AttendanceHistoryBySessionQueryHandler
    : IRequestHandler<AttendanceHistoryBySessionQuery, IReadOnlyList<SessionAttendanceSummary>>
{
    private readonly IAppDbContext _db;

    public AttendanceHistoryBySessionQueryHandler(IAppDbContext db) => _db = db;

    public async Task<IReadOnlyList<SessionAttendanceSummary>> Handle(
        AttendanceHistoryBySessionQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Session> sessionQuery = _db.Sessions;

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
            var rate = marked > 0 ? (double)present / marked : 0d;
            // Turnout signal (L2-ENGAGE-004): flag sessions where fewer than half the marked youth attended.
            var status = rate < 0.5 ? "low turnout" : "saved";
            summaries.Add(new SessionAttendanceSummary(
                session.Id, session.Title, session.StartsAt, present, marked, rate, status));
        }

        return summaries;
    }
}
