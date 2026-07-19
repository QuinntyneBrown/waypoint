using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Engagement.Notifications;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Engagement.Messaging;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, MessageResult>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IChangePublisher _publisher;
    private readonly NotificationDeliveryService _notifications;

    public SendMessageCommandHandler(
        IAppDbContext db,
        ICurrentUser currentUser,
        IClock clock,
        IChangePublisher publisher,
        NotificationDeliveryService notifications)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _publisher = publisher;
        _notifications = notifications;
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

        await _publisher.PublishToUserAsync(
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
