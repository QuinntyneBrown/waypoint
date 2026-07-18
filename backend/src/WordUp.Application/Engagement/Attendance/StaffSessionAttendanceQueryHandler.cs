using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Engagement.Attendance;

public sealed class StaffSessionAttendanceQueryHandler
    : IRequestHandler<StaffSessionAttendanceQuery, StaffSessionAttendance>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public StaffSessionAttendanceQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<StaffSessionAttendance> Handle(
        StaffSessionAttendanceQuery request,
        CancellationToken cancellationToken)
    {
        var session = await _db.Sessions.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.SessionId, cancellationToken)
            ?? throw new NotFoundException("Session not found.");
        if (_currentUser.Role == Roles.Mentor && session.MentorId != _currentUser.UserId)
        {
            throw new ForbiddenException("This session is not assigned to you.");
        }

        var cohortName = await _db.Cohorts.AsNoTracking()
            .Where(cohort => cohort.Id == session.CohortId)
            .Select(cohort => cohort.Name)
            .FirstOrDefaultAsync(cancellationToken) ?? "Cohort";
        var youth = await _db.Users.AsNoTracking()
            .Where(user => user.CohortId == session.CohortId
                && user.Role == Roles.Youth
                && user.Status == UserStatus.Active)
            .OrderBy(user => user.DisplayName)
            .Select(user => new { user.Id, user.DisplayName })
            .ToListAsync(cancellationToken);
        var youthIds = youth.Select(user => user.Id).ToList();
        var records = await _db.AttendanceRecords.AsNoTracking()
            .Where(record => record.SessionId == session.Id
                && youthIds.Contains(record.YouthUserId))
            .ToDictionaryAsync(record => record.YouthUserId, cancellationToken);

        return new StaffSessionAttendance(
            session.Id,
            session.Title,
            session.StartsAt,
            session.DurationMinutes,
            session.Location,
            session.CohortId,
            cohortName,
            youth.Select(user =>
            {
                records.TryGetValue(user.Id, out var record);
                return new StaffAttendanceRosterEntry(
                    user.Id,
                    user.DisplayName,
                    record?.Status.ToString(),
                    record?.Note,
                    record?.ConcurrencyStamp.ToString() ?? "none");
            }).ToList());
    }
}
