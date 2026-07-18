using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;
using WordUp.Domain;

namespace WordUp.Application.Messaging;

public class MessageThreadQueryHandler : IRequestHandler<MessageThreadQuery, IReadOnlyList<MessageView>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public MessageThreadQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<MessageView>> Handle(MessageThreadQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;
        var other = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.OtherUserId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        // Only a genuine participant of the conversation may read it (L2-ENGAGE-007): youth↔their mentor, or admin.
        bool isParticipant;
        if (_currentUser.Role == Roles.ProgramAdministrator)
        {
            isParticipant = true;
        }
        else if (_currentUser.Role == Roles.Mentor)
        {
            isParticipant = other.MentorId == userId;
        }
        else if (_currentUser.Role == Roles.Youth)
        {
            var self = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            isParticipant = self?.MentorId == request.OtherUserId;
        }
        else
        {
            isParticipant = false;
        }

        if (!isParticipant)
        {
            throw new ForbiddenException("You are not a participant in this conversation.");
        }

        var threadKey = Message.KeyFor(userId, request.OtherUserId);
        return await _db.Messages
            .Where(m => m.ThreadKey == threadKey)
            .OrderBy(m => m.SentAt)
            .Select(m => new MessageView(m.Id, m.SenderId, m.RecipientId, m.Body, m.SentAt, m.ReadAt))
            .ToListAsync(cancellationToken);
    }
}
