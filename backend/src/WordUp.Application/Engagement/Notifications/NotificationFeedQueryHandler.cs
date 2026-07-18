using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.Engagement.Notifications;

public class NotificationFeedQueryHandler : IRequestHandler<NotificationFeedQuery, IReadOnlyList<NotificationView>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public NotificationFeedQueryHandler(IAppDbContext db, ICurrentUser currentUser, IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<IReadOnlyList<NotificationView>> Handle(NotificationFeedQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;
        var now = _clock.UtcNow;
        var due = await _db.ScheduledNotifications
            .Where(item => item.UserId == userId
                && item.DeliveredAt == null
                && item.CancelledAt == null
                && item.DeliverAt <= now)
            .ToListAsync(cancellationToken);

        if (due.Count > 0)
        {
            var eventKeys = due.Select(item => item.EventKey).Distinct().ToList();
            var preferences = await _db.NotificationPreferences
                .Where(item => item.UserId == userId && eventKeys.Contains(item.EventKey))
                .ToDictionaryAsync(item => item.EventKey, item => item.InApp, cancellationToken);

            foreach (var scheduled in due)
            {
                if (!preferences.TryGetValue(scheduled.EventKey, out var inApp) || inApp)
                {
                    _db.Notifications.Add(new Notification
                    {
                        UserId = userId,
                        EventKey = scheduled.EventKey,
                        Title = scheduled.Title,
                        Body = scheduled.Body,
                        LinkPath = scheduled.LinkPath,
                        CreatedAt = now,
                    });
                }

                // Consume the schedule even when the user opted out, so it cannot be emitted later.
                scheduled.DeliveredAt = now;
            }

            await _db.SaveChangesAsync(cancellationToken);
        }

        return await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationView(
                n.Id,
                n.EventKey,
                n.Title,
                n.Body,
                n.LinkPath,
                n.ReadAt != null,
                n.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
