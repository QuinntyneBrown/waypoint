using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.Engagement.Notifications;

/// <summary>Prepares a notification inside the caller's unit of work, then dispatches its selected
/// channels after persistence. Missing preferences mean in-app on and optional email off.</summary>
public sealed class NotificationDeliveryService
{
    private readonly IAppDbContext _db;
    private readonly IEmailSender _email;
    private readonly IChangePublisher _publisher;

    public NotificationDeliveryService(
        IAppDbContext db,
        IEmailSender email,
        IChangePublisher publisher)
    {
        _db = db;
        _email = email;
        _publisher = publisher;
    }

    public async Task<NotificationDeliveryPlan> PrepareAsync(
        Guid userId,
        string eventKey,
        string title,
        string body,
        string? linkPath,
        DateTime createdAt,
        CancellationToken cancellationToken)
    {
        var preference = await _db.NotificationPreferences.AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.UserId == userId && item.EventKey == eventKey,
                cancellationToken);
        Notification? notification = null;
        if (preference?.InApp != false)
        {
            notification = new Notification
            {
                UserId = userId,
                EventKey = eventKey,
                Title = title,
                Body = body,
                LinkPath = linkPath,
                CreatedAt = createdAt,
            };
            _db.Notifications.Add(notification);
        }

        string? emailAddress = null;
        if (preference?.Email == true)
        {
            emailAddress = await _db.Users.AsNoTracking()
                .Where(item => item.Id == userId)
                .Select(item => item.Email)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var emailBody = string.IsNullOrWhiteSpace(linkPath)
            ? body
            : $"{body}\n\nOpen Word Up: {linkPath}";
        return new NotificationDeliveryPlan(notification, emailAddress, title, emailBody);
    }

    public async Task DispatchAsync(
        NotificationDeliveryPlan plan,
        CancellationToken cancellationToken)
    {
        if (plan.EmailAddress is not null)
        {
            await _email.SendAsync(
                plan.EmailAddress,
                plan.EmailSubject,
                plan.EmailBody,
                cancellationToken);
        }

        if (plan.InAppNotification is { } notification)
        {
            await _publisher.PublishToUserAsync(
                notification.UserId,
                "notification",
                new
                {
                    id = notification.Id,
                    eventKey = notification.EventKey,
                    title = notification.Title,
                    body = notification.Body,
                    linkPath = notification.LinkPath,
                    createdAt = notification.CreatedAt,
                },
                cancellationToken);
        }
    }
}

public sealed record NotificationDeliveryPlan(
    Notification? InAppNotification,
    string? EmailAddress,
    string EmailSubject,
    string EmailBody);
