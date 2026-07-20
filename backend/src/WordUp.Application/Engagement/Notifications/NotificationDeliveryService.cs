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
    private readonly IApplicationLinkBuilder _links;
    private readonly IAfterCommitDispatcher _afterCommit;
    private readonly IUserPrivacyLock _privacyLock;

    public NotificationDeliveryService(
        IAppDbContext db,
        IEmailSender email,
        IChangePublisher publisher,
        IApplicationLinkBuilder links,
        IAfterCommitDispatcher afterCommit,
        IUserPrivacyLock privacyLock)
    {
        _db = db;
        _email = email;
        _publisher = publisher;
        _links = links;
        _afterCommit = afterCommit;
        _privacyLock = privacyLock;
    }

    public async Task<NotificationDeliveryPlan> PrepareAsync(
        Guid userId,
        string eventKey,
        string title,
        string body,
        string? linkPath,
        DateTime createdAt,
        CancellationToken cancellationToken,
        Guid? scheduledNotificationId = null,
        Guid? subjectUserId = null)
    {
        // Every producer and the SMTP worker acquire linked account locks in the same global order.
        // SQL Server keeps them through the caller's transaction commit, preventing both lock-order
        // inversion and a subject erasure scan from racing an in-app-only notification write.
        var linkedUserIds = new Guid?[] { userId, subjectUserId }
            .Where(item => item.HasValue)
            .Select(item => item!.Value)
            .Distinct()
            .OrderBy(item => item)
            .ToList();
        foreach (var linkedUserId in linkedUserIds)
        {
            await _privacyLock.AcquireAsync(linkedUserId, cancellationToken);
        }

        var linkedUsers = await _db.Users.AsNoTracking()
            .Where(item => linkedUserIds.Contains(item.Id))
            .Select(item => new NotificationLinkedUser(
                item.Id,
                item.Email,
                item.Status,
                item.PersonalDataAnonymizedAt))
            .ToListAsync(cancellationToken);
        var recipient = linkedUsers.SingleOrDefault(item => item.Id == userId);
        var subject = subjectUserId is Guid linkedSubjectId
            ? linkedUsers.SingleOrDefault(item => item.Id == linkedSubjectId)
            : null;
        if (!IsActive(recipient)
            || (subjectUserId is not null && !IsActive(subject)))
        {
            return new NotificationDeliveryPlan(
                null,
                null,
                title,
                body,
                null,
                subjectUserId,
                userId);
        }

        var preference = await _db.NotificationPreferences.AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.UserId == userId && item.EventKey == eventKey,
                cancellationToken);
        Notification? notification = scheduledNotificationId is Guid sourceId
            ? await _db.Notifications.FirstOrDefaultAsync(
                item => item.ScheduledNotificationId == sourceId,
                cancellationToken)
            : null;
        if (preference?.InApp != false)
        {
            if (notification is null)
            {
                notification = new Notification
                {
                    UserId = userId,
                    EventKey = eventKey,
                    Title = title,
                    Body = body,
                    LinkPath = linkPath,
                    CreatedAt = createdAt,
                    ScheduledNotificationId = scheduledNotificationId,
                };
                _db.Notifications.Add(notification);
            }
        }

        string? emailAddress = null;
        if (preference?.Email == true)
        {
            emailAddress = recipient!.Email;
        }

        var emailBody = string.IsNullOrWhiteSpace(linkPath)
            ? body
            : $"{body}\n\nOpen Word Up: {_links.Portal(linkPath)}";
        return new NotificationDeliveryPlan(
            notification,
            emailAddress,
            title,
            emailBody,
            scheduledNotificationId is Guid id ? $"scheduled-notification:{id:N}" : null,
            subjectUserId,
            userId);
    }

    private static bool IsActive(NotificationLinkedUser? user) =>
        user is { Status: UserStatus.Active, PersonalDataAnonymizedAt: null };

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
                cancellationToken,
                plan.EmailIdempotencyKey,
                plan.SubjectUserId,
                recipientUserId: plan.RecipientUserId);
        }

        if (plan.InAppNotification is { } notification)
        {
            await _afterCommit.DispatchBestEffortAsync(
                callbackCancellationToken => _publisher.PublishToUserAsync(
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
                    callbackCancellationToken),
                cancellationToken);
        }
    }
}

internal sealed record NotificationLinkedUser(
    Guid Id,
    string Email,
    UserStatus Status,
    DateTime? PersonalDataAnonymizedAt);

public sealed record NotificationDeliveryPlan(
    Notification? InAppNotification,
    string? EmailAddress,
    string EmailSubject,
    string EmailBody,
    string? EmailIdempotencyKey,
    Guid? SubjectUserId,
    Guid RecipientUserId);
