using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.VolunteerPortal;

public class VolunteerScheduleQueryHandler : IRequestHandler<VolunteerScheduleQuery, IReadOnlyList<VolunteerScheduleItem>>
{
    private const string StatusUpcoming = "upcoming";
    private const string StatusAttendanceTaken = "attendance-taken";

    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;

    public VolunteerScheduleQueryHandler(IAppDbContext db, IClock clock, ICurrentUser currentUser)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<VolunteerScheduleItem>> Handle(VolunteerScheduleQuery request, CancellationToken cancellationToken)
    {
        var volunteerId = _currentUser.UserId ?? throw new ForbiddenException();
        var now = _clock.UtcNow;

        // Upcoming sessions assigned to this volunteer (L2-VOLUNTEER-002 AC1).
        var sessions = await _db.Sessions
            .Where(s => s.VolunteerId == volunteerId && s.StartsAt >= now)
            .OrderBy(s => s.StartsAt)
            .Select(s => new { s.Id, s.Title, s.StartsAt, s.DurationMinutes, s.Location })
            .ToListAsync(cancellationToken);

        // Empty state (AC2).
        if (sessions.Count == 0)
        {
            return new List<VolunteerScheduleItem>();
        }

        // Status is derived from whether any attendance record exists for the session.
        var sessionIds = sessions.Select(s => s.Id).ToList();
        var takenSessionIds = await _db.AttendanceRecords
            .Where(r => sessionIds.Contains(r.SessionId))
            .Select(r => r.SessionId)
            .Distinct()
            .ToListAsync(cancellationToken);
        var taken = takenSessionIds.ToHashSet();

        return sessions
            .Select(s => new VolunteerScheduleItem(
                s.Id, s.Title, s.StartsAt, s.DurationMinutes, s.Location,
                taken.Contains(s.Id) ? StatusAttendanceTaken : StatusUpcoming))
            .ToList();
    }
}
