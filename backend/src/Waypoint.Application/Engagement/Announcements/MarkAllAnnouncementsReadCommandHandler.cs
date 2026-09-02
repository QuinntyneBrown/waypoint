using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;
using Waypoint.Application.Shared.Common;

namespace Waypoint.Application.Engagement.Announcements;

public class MarkAllAnnouncementsReadCommandHandler : IRequestHandler<MarkAllAnnouncementsReadCommand>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public MarkAllAnnouncementsReadCommandHandler(IAppDbContext db, ICurrentUser currentUser, IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task Handle(MarkAllAnnouncementsReadCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;
        await AnnouncementAccess.ActivateDueAsync(_db, _clock, cancellationToken);
        var query = await AnnouncementAccess.VisibleToAsync(_db, userId, cancellationToken);

        var visibleIds = await query.Select(a => a.Id).ToListAsync(cancellationToken);

        var alreadyRead = (await _db.AnnouncementReads
                .Where(ar => ar.UserId == userId && visibleIds.Contains(ar.AnnouncementId))
                .Select(ar => ar.AnnouncementId)
                .ToListAsync(cancellationToken))
            .ToHashSet();

        var now = _clock.UtcNow;
        foreach (var id in visibleIds)
        {
            if (alreadyRead.Contains(id))
            {
                continue;
            }

            _db.AnnouncementReads.Add(new AnnouncementRead
            {
                AnnouncementId = id,
                UserId = userId,
                ReadAt = now,
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
