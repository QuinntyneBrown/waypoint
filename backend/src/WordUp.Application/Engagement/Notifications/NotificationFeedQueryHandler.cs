using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.Engagement.Notifications;

public class NotificationFeedQueryHandler : IRequestHandler<NotificationFeedQuery, IReadOnlyList<NotificationView>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public NotificationFeedQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<NotificationView>> Handle(NotificationFeedQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;
        return await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationView(
                n.Id,
                n.EventKey,
                n.Title,
                n.Body,
                n.LinkPath,
                n.ReadAt != null,
                n.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
