using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.Engagement.Notifications;

public class NotificationFeedQueryHandler : IRequestHandler<NotificationFeedQuery, IReadOnlyList<NotificationView>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly NotificationDeliveryService _notifications;

    public NotificationFeedQueryHandler(
        IAppDbContext db,
        ICurrentUser currentUser,
        IClock clock,
        NotificationDeliveryService notifications)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _notifications = notifications;
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
            var deliveryPlans = new List<NotificationDeliveryPlan>();
            foreach (var scheduled in due)
            {
                deliveryPlans.Add(await _notifications.PrepareAsync(
                    userId,
                    scheduled.EventKey,
                    scheduled.Title,
                    scheduled.Body,
                    scheduled.LinkPath,
                    now,
                    cancellationToken));

                // Consume the schedule even when the user opted out, so it cannot be emitted later.
                scheduled.DeliveredAt = now;
            }

            await _db.SaveChangesAsync(cancellationToken);
            foreach (var plan in deliveryPlans)
            {
                await _notifications.DispatchAsync(plan, cancellationToken);
            }
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
