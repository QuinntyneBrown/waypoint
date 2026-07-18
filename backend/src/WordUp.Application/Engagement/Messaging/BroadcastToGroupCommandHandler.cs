using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Engagement.Messaging;

public class BroadcastToGroupCommandHandler : IRequestHandler<BroadcastToGroupCommand, BroadcastResult>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IChangePublisher _publisher;
    private readonly IAuditLogger _audit;

    public BroadcastToGroupCommandHandler(
        IAppDbContext db,
        ICurrentUser currentUser,
        IClock clock,
        IChangePublisher publisher,
        IAuditLogger audit)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _publisher = publisher;
        _audit = audit;
    }

    public async Task<BroadcastResult> Handle(BroadcastToGroupCommand request, CancellationToken cancellationToken)
    {
        var mentorId = _currentUser.UserId!.Value;
        var cohortExists = await _db.Cohorts.AsNoTracking()
            .AnyAsync(cohort => cohort.Id == request.CohortId, cancellationToken);
        if (!cohortExists)
            throw new NotFoundException("Cohort not found.");

        var plainBody = request.Body.Trim();
        var body = WebUtility.HtmlEncode(plainBody);
        var now = _clock.UtcNow;

        var youths = await _db.Users
            .Where(user => user.MentorId == mentorId
                && user.CohortId == request.CohortId
                && user.Role == Roles.Youth
                && user.Status == UserStatus.Active)
            .Select(user => new { user.Id })
            .ToListAsync(cancellationToken);
        if (youths.Count == 0)
            throw new ForbiddenException("This cohort has no active youth assigned to you.");

        var youthIds = youths.Select(youth => youth.Id).ToList();
        var optedOut = await _db.NotificationPreferences.AsNoTracking()
            .Where(preference => youthIds.Contains(preference.UserId)
                && preference.EventKey == "mentor-messages"
                && !preference.InApp)
            .Select(preference => preference.UserId)
            .ToListAsync(cancellationToken);
        var optedOutSet = optedOut.ToHashSet();
        var deliveries = new List<(Guid YouthId, Message Message, Notification? Notification)>();

        foreach (var youth in youths)
        {
            var message = new Message
            {
                ThreadKey = Message.KeyFor(mentorId, youth.Id),
                SenderId = mentorId,
                RecipientId = youth.Id,
                Body = body,
                SentAt = now,
            };
            _db.Messages.Add(message);

            Notification? notification = null;
            if (!optedOutSet.Contains(youth.Id))
            {
                notification = new Notification
                {
                    UserId = youth.Id,
                    EventKey = "mentor-messages",
                    Title = "New message",
                    Body = "You have a new message from your mentor.",
                    LinkPath = "/youth/messages",
                    CreatedAt = now,
                };
                _db.Notifications.Add(notification);
            }
            deliveries.Add((youth.Id, message, notification));
        }

        await _db.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(
            "MentorMessageBroadcast",
            mentorId,
            "Cohort",
            request.CohortId,
            details: $"Recipients={youths.Count}",
            cancellationToken: cancellationToken);

        foreach (var delivery in deliveries)
        {
            await _publisher.PublishToUserAsync(
                delivery.YouthId,
                "message",
                new
                {
                    id = delivery.Message.Id,
                    senderId = delivery.Message.SenderId,
                    recipientId = delivery.Message.RecipientId,
                    body = plainBody,
                    sentAt = delivery.Message.SentAt,
                    readAt = delivery.Message.ReadAt,
                },
                cancellationToken);
            if (delivery.Notification is not { } notification)
                continue;
            await _publisher.PublishToUserAsync(
                delivery.YouthId,
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

        return new BroadcastResult(youths.Count);
    }
}
