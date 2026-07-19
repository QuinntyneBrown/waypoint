using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Engagement.Announcements;

namespace WordUp.Application.GuardianPortal;

public class GuardianAnnouncementsQueryHandler
    : IRequestHandler<GuardianAnnouncementsQuery, IReadOnlyList<GuardianAnnouncementView>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public GuardianAnnouncementsQueryHandler(IAppDbContext db, ICurrentUser currentUser, IClock clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<IReadOnlyList<GuardianAnnouncementView>> Handle(
        GuardianAnnouncementsQuery request, CancellationToken cancellationToken)
    {
        var guardianId = _currentUser.UserId!.Value;
        await AnnouncementAccess.ActivateDueAsync(_db, _clock, cancellationToken);
        var query = await AnnouncementAccess.VisibleToAsync(_db, guardianId, cancellationToken);
        var announcements = await query
            .OrderByDescending(a => a.SentAt)
            .ToListAsync(cancellationToken);

        var readIds = (await _db.AnnouncementReads
                .Where(ar => ar.UserId == guardianId)
                .Select(ar => ar.AnnouncementId)
                .ToListAsync(cancellationToken))
            .ToHashSet();

        return announcements
            .Select(a => new GuardianAnnouncementView(a.Id, a.Title, a.Message, a.SentAt, readIds.Contains(a.Id)))
            .ToList();
    }
}
