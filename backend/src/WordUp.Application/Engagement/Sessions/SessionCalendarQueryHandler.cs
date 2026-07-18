using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.Engagement.Sessions;

public class SessionCalendarQueryHandler : IRequestHandler<SessionCalendarQuery, IReadOnlyList<SessionView>>
{
    private readonly IAppDbContext _db;

    public SessionCalendarQueryHandler(IAppDbContext db) => _db = db;

    public async Task<IReadOnlyList<SessionView>> Handle(SessionCalendarQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Session> query = _db.Sessions;

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
                s.Id, s.CohortId, s.MentorId, s.Title, s.StartsAt, s.DurationMinutes, s.Location))
            .ToListAsync(cancellationToken);
    }
}
