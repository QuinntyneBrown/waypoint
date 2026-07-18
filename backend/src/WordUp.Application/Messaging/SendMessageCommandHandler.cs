using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;
using WordUp.Domain;

namespace WordUp.Application.Messaging;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, MessageResult>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public SendMessageCommandHandler(IAppDbContext db, ICurrentUser currentUser, IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
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
            allowed = target.MentorId == senderId;
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

        // In-app notification for the recipient, unless they have opted out of in-app "new-message" (ENGAGE-008 AC4).
        var preference = await _db.NotificationPreferences
            .FirstOrDefaultAsync(p => p.UserId == request.ToUserId && p.EventKey == "new-message", cancellationToken);

        if (preference is null || preference.InApp)
        {
            _db.Notifications.Add(new Notification
            {
                UserId = request.ToUserId,
                EventKey = "new-message",
                Title = "New message",
                Body = "You have a new message.",
                LinkPath = $"/messages/{senderId}",
                CreatedAt = _clock.UtcNow,
            });
            await _db.SaveChangesAsync(cancellationToken);
        }

        return new MessageResult(message.Id);
    }
}
