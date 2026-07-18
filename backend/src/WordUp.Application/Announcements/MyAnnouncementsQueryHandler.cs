using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;
using WordUp.Domain;

namespace WordUp.Application.Announcements;

public class MyAnnouncementsQueryHandler : IRequestHandler<MyAnnouncementsQuery, IReadOnlyList<AnnouncementView>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public MyAnnouncementsQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<AnnouncementView>> Handle(MyAnnouncementsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        // Only SENT announcements targeting this user's audience (org => everyone; cohort => same cohort)
        // and role (ENGAGE-006). Admins see everything sent.
        var query = _db.Announcements.Where(a => a.SentAt != null);
        query = query.Where(a => a.AudienceScope == "org" || (a.AudienceScope == "cohort" && a.CohortId == user.CohortId));

        if (user.Role != Roles.ProgramAdministrator)
        {
            var allowedTargets = TargetsForRole(user.Role);
            query = query.Where(a => allowedTargets.Contains(a.RoleTargeting));
        }

        var announcements = await query
            .OrderByDescending(a => a.SentAt)
            .ToListAsync(cancellationToken);

        var readIds = (await _db.AnnouncementReads
                .Where(ar => ar.UserId == userId)
                .Select(ar => ar.AnnouncementId)
                .ToListAsync(cancellationToken))
            .ToHashSet();

        return announcements
            .Select(a => new AnnouncementView(a.Id, a.Title, a.Message, a.SentAt, readIds.Contains(a.Id)))
            .ToList();
    }

    // Role targeting values a given role is allowed to see (ENGAGE-006).
    internal static string[] TargetsForRole(string role) => role switch
    {
        Roles.Youth => new[] { "Youth", "YouthGuardians", "Everyone" },
        Roles.Guardian => new[] { "Guardians", "YouthGuardians", "Everyone" },
        Roles.Mentor => new[] { "Mentors", "Everyone" },
        _ => new[] { "Everyone" },
    };
}
