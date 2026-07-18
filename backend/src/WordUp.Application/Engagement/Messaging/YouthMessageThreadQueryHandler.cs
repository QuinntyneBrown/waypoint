using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Engagement.Messaging;

public sealed class YouthMessageThreadQueryHandler
    : IRequestHandler<YouthMessageThreadQuery, YouthMessageThreadView>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public YouthMessageThreadQueryHandler(IAppDbContext db, ICurrentUser currentUser, IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<YouthMessageThreadView> Handle(
        YouthMessageThreadQuery request,
        CancellationToken cancellationToken)
    {
        var youthId = _currentUser.UserId ?? throw new ForbiddenException();
        var mentorId = await _db.Users
            .Where(item => item.Id == youthId)
            .Select(item => item.MentorId)
            .FirstOrDefaultAsync(cancellationToken);
        if (mentorId is null)
            return new YouthMessageThreadView(null, null, 0, []);

        var mentorName = await _db.Users.AsNoTracking()
            .Where(item => item.Id == mentorId && item.Role == Roles.Mentor)
            .Select(item => item.DisplayName)
            .FirstOrDefaultAsync(cancellationToken);
        if (mentorName is null)
            return new YouthMessageThreadView(null, null, 0, []);

        var threadKey = Message.KeyFor(youthId, mentorId.Value);
        var messages = await _db.Messages
            .Where(item => item.ThreadKey == threadKey)
            .OrderBy(item => item.SentAt)
            .ToListAsync(cancellationToken);
        var unread = messages.Count(item => item.RecipientId == youthId && item.ReadAt == null);
        var now = _clock.UtcNow;
        foreach (var message in messages.Where(item => item.RecipientId == youthId && item.ReadAt == null))
            message.ReadAt = now;
        if (unread > 0)
            await _db.SaveChangesAsync(cancellationToken);

        return new YouthMessageThreadView(
            mentorId,
            mentorName,
            unread,
            messages.Select(item => new MessageView(
                item.Id,
                item.SenderId,
                item.RecipientId,
                WebUtility.HtmlDecode(item.Body),
                item.SentAt,
                item.ReadAt)).ToList());
    }
}
