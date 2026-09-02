using MediatR;
using Microsoft.EntityFrameworkCore;
using Waypoint.Application.Shared.Abstractions;

namespace Waypoint.Application.Engagement.Announcements;

public sealed class ManageAnnouncementsQueryHandler
    : IRequestHandler<ManageAnnouncementsQuery, IReadOnlyList<ManagedAnnouncement>>
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;

    public ManageAnnouncementsQueryHandler(IAppDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<IReadOnlyList<ManagedAnnouncement>> Handle(
        ManageAnnouncementsQuery request,
        CancellationToken cancellationToken)
    {
        await AnnouncementAccess.ActivateDueAsync(_db, _clock, cancellationToken);
        var announcements = await _db.Announcements.AsNoTracking()
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync(cancellationToken);
        var cohortIds = announcements
            .Where(item => item.CohortId != null)
            .Select(item => item.CohortId!.Value)
            .Distinct()
            .ToList();
        var cohortNames = await _db.Cohorts.AsNoTracking()
            .Where(item => cohortIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, item => item.Name, cancellationToken);
        var result = new List<ManagedAnnouncement>(announcements.Count);

        foreach (var announcement in announcements)
        {
            var stats = await AnnouncementAccess.StatsAsync(_db, announcement, cancellationToken);
            result.Add(new ManagedAnnouncement(
                announcement.Id,
                announcement.Title,
                announcement.Message,
                announcement.AudienceScope,
                announcement.CohortId,
                announcement.CohortId is Guid cohortId ? cohortNames.GetValueOrDefault(cohortId) : null,
                announcement.RoleTargeting,
                announcement.ScheduledAt,
                announcement.SentAt,
                announcement.CreatedAt,
                stats.Recipients,
                stats.Reads,
                stats.ReadRate));
        }

        return result;
    }
}
