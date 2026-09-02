using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.VolunteerPortal;

public class VolunteerTodayQueryHandler : IRequestHandler<VolunteerTodayQuery, VolunteerTodayView?>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;

    public VolunteerTodayQueryHandler(IAppDbContext db, IClock clock, ICurrentUser currentUser)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
    }

    public async Task<VolunteerTodayView?> Handle(VolunteerTodayQuery request, CancellationToken cancellationToken)
    {
        var volunteerId = _currentUser.UserId ?? throw new ForbiddenException();

        var todayStart = _clock.UtcNow.Date;
        var tomorrowStart = todayStart.AddDays(1);

        // Only today's session assigned to this volunteer (L2-VOLUNTEER-001 AC1). No youth records.
        var session = await _db.Sessions
            .Where(s => s.VolunteerId == volunteerId
                && s.StartsAt >= todayStart
                && s.StartsAt < tomorrowStart)
            .OrderBy(s => s.StartsAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (session is null)
        {
            return null;
        }

        // Roster count only — youth in the session's cohort. No per-youth data is surfaced.
        var rosterCount = await _db.Users
            .CountAsync(u => u.CohortId == session.CohortId && u.Role == Roles.Youth, cancellationToken);

        return new VolunteerTodayView(
            session.Id, session.Title, session.StartsAt, session.DurationMinutes, session.Location, rosterCount);
    }
}
