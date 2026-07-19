using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.Engagement.Announcements;

public class AnnouncementStatsQueryHandler : IRequestHandler<AnnouncementStatsQuery, AnnouncementStats>
{
    private readonly IAppDbContext _db;

    public AnnouncementStatsQueryHandler(IAppDbContext db) => _db = db;

    public async Task<AnnouncementStats> Handle(AnnouncementStatsQuery request, CancellationToken cancellationToken)
    {
        var announcement = await _db.Announcements
            .FirstOrDefaultAsync(a => a.Id == request.AnnouncementId, cancellationToken)
            ?? throw new NotFoundException("Announcement not found.");

        return await AnnouncementAccess.StatsAsync(_db, announcement, cancellationToken);
    }
}
