using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.Engagement.Attendance;

public sealed class AttendanceHistoryYouthListQueryHandler
    : IRequestHandler<AttendanceHistoryYouthListQuery, IReadOnlyList<YouthAttendanceSummary>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public AttendanceHistoryYouthListQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<YouthAttendanceSummary>> Handle(
        AttendanceHistoryYouthListQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException();
        var youthQuery = _db.Users.AsNoTracking().Where(user =>
            user.Role == Roles.Youth && user.Status == UserStatus.Active);
        if (_currentUser.Role == Roles.Mentor)
        {
            youthQuery = youthQuery.Where(user => user.MentorId == userId);
        }
        if (request.CohortId is { } cohortId)
        {
            youthQuery = youthQuery.Where(user => user.CohortId == cohortId);
        }
        var youth = await youthQuery
            .OrderBy(user => user.DisplayName)
            .Select(user => new { user.Id, user.DisplayName })
            .ToListAsync(cancellationToken);
        var youthIds = youth.Select(user => user.Id).ToList();

        var sessions = _db.Sessions.AsNoTracking();
        if (_currentUser.Role == Roles.Mentor)
        {
            sessions = sessions.Where(session => session.MentorId == userId);
        }
        if (request.CohortId is { } filterCohortId)
        {
            sessions = sessions.Where(session => session.CohortId == filterCohortId);
        }
        if (request.From is { } from)
        {
            sessions = sessions.Where(session => session.StartsAt >= from);
        }
        if (request.To is { } to)
        {
            sessions = sessions.Where(session => session.StartsAt <= to);
        }
        var sessionRows = await sessions
            .Select(session => new { session.Id, session.StartsAt })
            .ToListAsync(cancellationToken);
        var sessionIds = sessionRows.Select(session => session.Id).ToList();
        var startsAt = sessionRows.ToDictionary(session => session.Id, session => session.StartsAt);
        var records = await _db.AttendanceRecords.AsNoTracking()
            .Where(record => youthIds.Contains(record.YouthUserId)
                && sessionIds.Contains(record.SessionId))
            .Select(record => new { record.YouthUserId, record.SessionId, record.Status })
            .ToListAsync(cancellationToken);

        return youth.Select(person =>
        {
            var statuses = records
                .Where(record => record.YouthUserId == person.Id)
                .OrderBy(record => startsAt.GetValueOrDefault(record.SessionId))
                .Select(record => record.Status)
                .ToList();
            var late = statuses.Count(status => status == AttendanceStatus.Late);
            var excused = statuses.Count(status => status == AttendanceStatus.Excused);
            var absent = statuses.Count(status => status == AttendanceStatus.Absent);
            var attended = statuses.Count(status =>
                status is AttendanceStatus.Present or AttendanceStatus.Late);
            var total = statuses.Count;
            var rate = total > 0 ? attended / (double)total : 0d;
            return new YouthAttendanceSummary(
                person.Id,
                attended,
                late,
                excused,
                absent,
                total,
                rate,
                total > 0 && rate < 0.7,
                person.DisplayName,
                AttendanceHistoryByYouthQueryHandler.AttendanceTrend(statuses));
        }).ToList();
    }
}
