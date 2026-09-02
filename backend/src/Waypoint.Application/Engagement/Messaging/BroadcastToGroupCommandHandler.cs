using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Engagement.Notifications;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.Engagement.Messaging;

public class BroadcastToGroupCommandHandler : IRequestHandler<BroadcastToGroupCommand, BroadcastResult>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IChangePublisher _publisher;
    private readonly NotificationDeliveryService _notifications;
    private readonly IAuditLogger _audit;
    private readonly IAfterCommitDispatcher _afterCommit;

    public BroadcastToGroupCommandHandler(
        IAppDbContext db,
        ICurrentUser currentUser,
        IClock clock,
        IChangePublisher publisher,
        NotificationDeliveryService notifications,
        IAuditLogger audit,
        IAfterCommitDispatcher afterCommit)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _publisher = publisher;
        _notifications = notifications;
        _audit = audit;
        _afterCommit = afterCommit;
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

        var deliveries = new List<(Guid YouthId, Message Message, NotificationDeliveryPlan Plan)>();

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
            var plan = await _notifications.PrepareAsync(
                youth.Id,
                "mentor-messages",
                "New message",
                "You have a new message from your mentor.",
                "/youth/messages",
                now,
                cancellationToken);
            deliveries.Add((youth.Id, message, plan));
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
            await _afterCommit.DispatchBestEffortAsync(
                callbackCancellationToken => _publisher.PublishToUserAsync(
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
                    callbackCancellationToken),
                cancellationToken);
            await _notifications.DispatchAsync(delivery.Plan, cancellationToken);
        }

        return new BroadcastResult(youths.Count);
    }
}
