using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Engagement.Notifications;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.Engagement.Messaging;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, MessageResult>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IChangePublisher _publisher;
    private readonly NotificationDeliveryService _notifications;
    private readonly IAfterCommitDispatcher _afterCommit;

    public SendMessageCommandHandler(
        IAppDbContext db,
        ICurrentUser currentUser,
        IClock clock,
        IChangePublisher publisher,
        NotificationDeliveryService notifications,
        IAfterCommitDispatcher afterCommit)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _publisher = publisher;
        _notifications = notifications;
        _afterCommit = afterCommit;
    }

    public async Task<MessageResult> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var senderId = _currentUser.UserId!.Value;
        var target = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.ToUserId, cancellationToken)
            ?? throw new NotFoundException("Recipient not found.");

        // Messaging scope (L2-AUTH-010 / L2-ENGAGE-007 AC4): only within a youth↔mentor relationship, or admin.
        bool allowed;
        if (_currentUser.Role == Roles.ProgramAdministrator)
        {
            allowed = true;
        }
        else if (_currentUser.Role == Roles.Mentor)
        {
            allowed = target.Role == Roles.Youth
                && target.Status == UserStatus.Active
                && target.MentorId == senderId;
        }
        else if (_currentUser.Role == Roles.Youth)
        {
            var sender = await _db.Users.FirstOrDefaultAsync(u => u.Id == senderId, cancellationToken);
            allowed = sender?.MentorId == request.ToUserId;
        }
        else
        {
            allowed = false;
        }

        if (!allowed)
        {
            throw new ForbiddenException("You are not permitted to message this user.");
        }

        var message = new Message
        {
            ThreadKey = Message.KeyFor(senderId, request.ToUserId),
            SenderId = senderId,
            RecipientId = request.ToUserId,
            // Neutralise stored XSS by encoding the raw body (L2-NFR-001).
            Body = WebUtility.HtmlEncode(request.Body),
            SentAt = _clock.UtcNow,
        };
        _db.Messages.Add(message);
        await _db.SaveChangesAsync(cancellationToken);

        await _afterCommit.DispatchBestEffortAsync(
            callbackCancellationToken => _publisher.PublishToUserAsync(
                request.ToUserId,
                "message",
                new
                {
                    id = message.Id,
                    senderId = message.SenderId,
                    recipientId = message.RecipientId,
                    body = request.Body,
                    sentAt = message.SentAt,
                    readAt = message.ReadAt,
                },
                callbackCancellationToken),
            cancellationToken);

        var notificationPlan = await _notifications.PrepareAsync(
            request.ToUserId,
            "mentor-messages",
            "New message",
            "You have a new message.",
            target.Role == Roles.Youth ? "/youth/messages" : "/mentor/messages",
            _clock.UtcNow,
            cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        await _notifications.DispatchAsync(notificationPlan, cancellationToken);

        return new MessageResult(message.Id);
    }
}
