using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Engagement.Sessions;

public class SessionCalendarQueryHandler : IRequestHandler<SessionCalendarQuery, IReadOnlyList<SessionView>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public SessionCalendarQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<SessionView>> Handle(SessionCalendarQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException();
        IQueryable<Session> query = _db.Sessions.AsNoTracking();

        if (_currentUser.Role == Roles.Mentor)
        {
            query = query.Where(session => session.MentorId == userId);
        }

        if (request.CohortId is { } cohortId)
        {
            query = query.Where(s => s.CohortId == cohortId);
        }
        if (request.From is { } from)
        {
            query = query.Where(s => s.StartsAt >= from);
        }
        if (request.To is { } to)
        {
            query = query.Where(s => s.StartsAt <= to);
        }

        return await query
            .OrderBy(s => s.StartsAt)
            .Select(s => new SessionView(
                s.Id,
                s.CohortId,
                s.MentorId,
                s.Title,
                s.StartsAt,
                s.DurationMinutes,
                s.Location,
                _db.Cohorts.Where(cohort => cohort.Id == s.CohortId)
                    .Select(cohort => cohort.Name)
                    .FirstOrDefault() ?? "Cohort",
                s.MentorId == null
                    ? "Unassigned"
                    : _db.Users.Where(user => user.Id == s.MentorId)
                        .Select(user => user.DisplayName)
                        .FirstOrDefault() ?? "Mentor",
                _db.AttendanceRecords.Any(record => record.SessionId == s.Id),
                _db.Users.Count(user => user.CohortId == s.CohortId
                    && user.Role == Roles.Youth
                    && user.Status == UserStatus.Active)))
            .ToListAsync(cancellationToken);
    }
}
