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

    public GuardianAnnouncementsQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<GuardianAnnouncementView>> Handle(
        GuardianAnnouncementsQuery request, CancellationToken cancellationToken)
    {
        var guardianId = _currentUser.UserId!.Value;

        // A guardian has no cohort of their own; cohort-scoped items resolve via the linked child(ren)
        // (L2-GUARDIAN-002 AC1; guardian has no CohortId per L2-AUTH-010).
        var childCohortIds = await _db.Users
            .Where(u => u.GuardianUserId == guardianId && u.CohortId != null)
            .Select(u => u.CohortId!.Value)
            .ToListAsync(cancellationToken);

        // Same role-targeting mapping as the main feed: Guardians / YouthGuardians / Everyone.
        var allowedTargets = MyAnnouncementsQueryHandler.TargetsForRole(Roles.Guardian);

        var announcements = await _db.Announcements
            .Where(a => a.SentAt != null)
            .Where(a => a.AudienceScope == "org"
                || (a.AudienceScope == "cohort" && a.CohortId != null && childCohortIds.Contains(a.CohortId.Value)))
            .Where(a => allowedTargets.Contains(a.RoleTargeting))
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
