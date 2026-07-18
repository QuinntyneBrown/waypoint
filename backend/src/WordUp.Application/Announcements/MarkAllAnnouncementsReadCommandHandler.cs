using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;
using WordUp.Domain;

namespace WordUp.Application.Announcements;

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
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        // Same visibility rules as the feed (ENGAGE-006).
        var query = _db.Announcements.Where(a => a.SentAt != null);
        query = query.Where(a => a.AudienceScope == "org" || (a.AudienceScope == "cohort" && a.CohortId == user.CohortId));

        if (user.Role != Roles.ProgramAdministrator)
        {
            var allowedTargets = MyAnnouncementsQueryHandler.TargetsForRole(user.Role);
            query = query.Where(a => allowedTargets.Contains(a.RoleTargeting));
        }

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
