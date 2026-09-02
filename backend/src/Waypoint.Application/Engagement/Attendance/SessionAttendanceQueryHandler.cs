using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.Engagement.Attendance;

public class SessionAttendanceQueryHandler : IRequestHandler<SessionAttendanceQuery, IReadOnlyList<AttendanceMarkView>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public SessionAttendanceQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<AttendanceMarkView>> Handle(SessionAttendanceQuery request, CancellationToken cancellationToken)
    {
        var session = await _db.Sessions.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.SessionId, cancellationToken)
            ?? throw new NotFoundException("Session not found.");
        if (_currentUser.Role == Roles.Mentor && session.MentorId != _currentUser.UserId)
        {
            throw new ForbiddenException("This session is not assigned to you.");
        }

        var rows = await _db.AttendanceRecords
            .Where(r => r.SessionId == request.SessionId)
            .OrderBy(r => r.YouthUserId)
            .ToListAsync(cancellationToken);

        return rows
            .Select(r => new AttendanceMarkView(
                r.YouthUserId,
                r.Status.ToString(),
                r.Note,
                r.RecordedAt,
                Version: r.ConcurrencyStamp.ToString()))
            .ToList();
    }
}
