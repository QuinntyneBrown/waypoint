using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.Engagement.Notifications;

public class MarkAllNotificationsReadCommandHandler : IRequestHandler<MarkAllNotificationsReadCommand>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public MarkAllNotificationsReadCommandHandler(IAppDbContext db, ICurrentUser currentUser, IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;
        var now = _clock.UtcNow;

        var unread = await _db.Notifications
            .Where(n => n.UserId == userId && n.ReadAt == null)
            .ToListAsync(cancellationToken);

        foreach (var notification in unread)
        {
            notification.ReadAt = now;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
