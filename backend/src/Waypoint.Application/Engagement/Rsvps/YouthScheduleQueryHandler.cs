using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.Engagement.Rsvps;

public sealed class YouthScheduleQueryHandler
    : IRequestHandler<YouthScheduleQuery, IReadOnlyList<YouthSessionView>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public YouthScheduleQueryHandler(IAppDbContext db, ICurrentUser currentUser, IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<IReadOnlyList<YouthSessionView>> Handle(
        YouthScheduleQuery request,
        CancellationToken cancellationToken)
    {
        var youthId = _currentUser.UserId ?? throw new ForbiddenException();
        var cohortId = await _db.Users.AsNoTracking()
            .Where(item => item.Id == youthId)
            .Select(item => item.CohortId)
            .FirstOrDefaultAsync(cancellationToken);
        if (cohortId is null) return [];

        var sessions = await _db.Sessions.AsNoTracking()
            .Where(item => item.CohortId == cohortId && item.StartsAt > _clock.UtcNow)
            .OrderBy(item => item.StartsAt)
            .ToListAsync(cancellationToken);
        var sessionIds = sessions.Select(item => item.Id).ToList();
        var rsvps = await _db.SessionRsvps.AsNoTracking()
            .Where(item => item.YouthUserId == youthId && sessionIds.Contains(item.SessionId))
            .ToDictionaryAsync(item => item.SessionId, cancellationToken);
        var rsvpIds = rsvps.Values.Select(item => item.Id).ToList();
        var reminders = await _db.ScheduledNotifications.AsNoTracking()
            .Where(item => item.UserId == youthId
                && item.SourceType == "SessionRsvp"
                && rsvpIds.Contains(item.SourceId)
                && item.DeliveredAt == null
                && item.CancelledAt == null)
            .ToDictionaryAsync(item => item.SourceId, cancellationToken);

        return sessions.Select(session =>
        {
            rsvps.TryGetValue(session.Id, out var rsvp);
            var reminder = rsvp is null ? null : reminders.GetValueOrDefault(rsvp.Id);
            return new YouthSessionView(
                session.Id,
                session.Title,
                session.StartsAt,
                session.DurationMinutes,
                session.Location,
                rsvp?.Response.ToString(),
                rsvp?.Note,
                rsvp?.ReminderSet ?? false,
                reminder?.DeliverAt);
        }).ToList();
    }
}
