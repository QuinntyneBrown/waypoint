using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.Engagement.Messaging;

public class BroadcastToGroupCommandHandler : IRequestHandler<BroadcastToGroupCommand, BroadcastResult>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public BroadcastToGroupCommandHandler(IAppDbContext db, ICurrentUser currentUser, IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<BroadcastResult> Handle(BroadcastToGroupCommand request, CancellationToken cancellationToken)
    {
        var mentorId = _currentUser.UserId!.Value;
        var body = WebUtility.HtmlEncode(request.Body);
        var now = _clock.UtcNow;

        var youths = await _db.Users
            .Where(u => u.MentorId == mentorId)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        foreach (var youthId in youths)
        {
            _db.Messages.Add(new Message
            {
                ThreadKey = Message.KeyFor(mentorId, youthId),
                SenderId = mentorId,
                RecipientId = youthId,
                Body = body,
                SentAt = now,
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        return new BroadcastResult(youths.Count);
    }
}
