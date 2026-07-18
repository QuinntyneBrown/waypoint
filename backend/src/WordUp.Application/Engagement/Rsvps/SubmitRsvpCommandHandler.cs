using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Engagement.Rsvps;

public class SubmitRsvpCommandHandler : IRequestHandler<SubmitRsvpCommand, RsvpResult>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public SubmitRsvpCommandHandler(IAppDbContext db, ICurrentUser currentUser, IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<RsvpResult> Handle(SubmitRsvpCommand request, CancellationToken cancellationToken)
    {
        var youthId = _currentUser.UserId ?? throw new ForbiddenException();
        var youthCohortId = await _db.Users
            .Where(item => item.Id == youthId)
            .Select(item => item.CohortId)
            .FirstOrDefaultAsync(cancellationToken);
        var session = await _db.Sessions.FirstOrDefaultAsync(
            item => item.Id == request.SessionId,
            cancellationToken) ?? throw new NotFoundException("Session not found.");
        if (youthCohortId is null || youthCohortId != session.CohortId)
            throw new ForbiddenException("This session is not in your cohort.");
        if (session.StartsAt <= _clock.UtcNow)
            throw new ConflictException("RSVPs are closed for this session.");

        SubmitRsvpCommand.TryParseResponse(request.Response, out var response);
        var now = _clock.UtcNow;
        var reminderSet = request.ReminderSet && response != RsvpResponse.CantAttend;
        var rsvp = await _db.SessionRsvps.FirstOrDefaultAsync(
            item => item.SessionId == request.SessionId && item.YouthUserId == youthId,
            cancellationToken);
        if (rsvp is null)
        {
            rsvp = new SessionRsvp
            {
                SessionId = request.SessionId,
                YouthUserId = youthId,
                CreatedAt = now,
            };
            _db.SessionRsvps.Add(rsvp);
        }
        rsvp.Response = response;
        rsvp.Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
        rsvp.ReminderSet = reminderSet;
        rsvp.UpdatedAt = now;

        var reminder = await _db.ScheduledNotifications.FirstOrDefaultAsync(
            item => item.UserId == youthId
                && item.EventKey == "session-reminders"
                && item.SourceType == "SessionRsvp"
                && item.SourceId == rsvp.Id,
            cancellationToken);
        if (reminderSet)
        {
            var deliverAt = session.StartsAt.AddHours(-24);
            if (deliverAt < now) deliverAt = now;
            if (reminder is null)
            {
                reminder = new ScheduledNotification
                {
                    UserId = youthId,
                    EventKey = "session-reminders",
                    SourceType = "SessionRsvp",
                    SourceId = rsvp.Id,
                    CreatedAt = now,
                };
                _db.ScheduledNotifications.Add(reminder);
            }
            reminder.Title = "Session reminder";
            reminder.Body = $"{session.Title} starts soon.";
            reminder.LinkPath = "/youth/schedule";
            reminder.DeliverAt = deliverAt;
            reminder.CancelledAt = null;
        }
        else if (reminder is not null && reminder.DeliveredAt is null)
        {
            reminder.CancelledAt = now;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return new RsvpResult(rsvp.Id);
    }
}
