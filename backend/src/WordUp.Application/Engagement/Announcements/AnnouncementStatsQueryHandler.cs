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

        // Recipient population = active users matching the announcement's audience + role targeting.
        var recipients = _db.Users.Where(u => u.Status == UserStatus.Active);

        if (announcement.AudienceScope == "cohort")
        {
            recipients = recipients.Where(u => u.CohortId == announcement.CohortId);
        }

        recipients = announcement.RoleTargeting switch
        {
            "Youth" => recipients.Where(u => u.Role == Roles.Youth),
            "Guardians" => recipients.Where(u => u.Role == Roles.Guardian),
            "Mentors" => recipients.Where(u => u.Role == Roles.Mentor),
            "YouthGuardians" => recipients.Where(u => u.Role == Roles.Youth || u.Role == Roles.Guardian),
            _ => recipients, // Everyone
        };

        var recipientCount = await recipients.CountAsync(cancellationToken);
        var readCount = await _db.AnnouncementReads
            .CountAsync(ar => ar.AnnouncementId == announcement.Id, cancellationToken);

        var readRate = recipientCount == 0 ? 0d : (double)readCount / recipientCount;
        return new AnnouncementStats(recipientCount, readCount, readRate);
    }
}
