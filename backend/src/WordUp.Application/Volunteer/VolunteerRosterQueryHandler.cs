using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;
using WordUp.Domain;

namespace WordUp.Application.Volunteer;

public class VolunteerRosterQueryHandler : IRequestHandler<VolunteerRosterQuery, IReadOnlyList<VolunteerRosterEntry>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public VolunteerRosterQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<VolunteerRosterEntry>> Handle(VolunteerRosterQuery request, CancellationToken cancellationToken)
    {
        var volunteerId = _currentUser.UserId ?? throw new ForbiddenException();

        var session = await _db.Sessions
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken)
            ?? throw new NotFoundException("Session not found.");

        // Fine-grained ownership: a volunteer may only view the roster of a session assigned to them
        // (L2-VOLUNTEER-001 AC4 / L2-AUTH-010). Out-of-scope resources are denied 403.
        if (session.VolunteerId != volunteerId)
        {
            throw new ForbiddenException("This session is not assigned to you.");
        }

        // Name only (L2-VOLUNTEER-001 AC2) — youth in the session's cohort. No progress/flags/notes.
        var youth = await _db.Users
            .Where(u => u.CohortId == session.CohortId && u.Role == Roles.Youth)
            .OrderBy(u => u.DisplayName)
            .Select(u => new { u.Id, u.DisplayName })
            .ToListAsync(cancellationToken);

        // Current mark (if any) so the client can render the Present/Absent control's state.
        var marks = await _db.AttendanceRecords
            .Where(r => r.SessionId == session.Id)
            .Select(r => new { r.YouthUserId, r.Status })
            .ToListAsync(cancellationToken);
        var markByYouth = marks.ToDictionary(m => m.YouthUserId, m => m.Status.ToString());

        return youth
            .Select(y => new VolunteerRosterEntry(
                y.Id,
                y.DisplayName,
                markByYouth.TryGetValue(y.Id, out var mark) ? mark : null))
            .ToList();
    }
}
