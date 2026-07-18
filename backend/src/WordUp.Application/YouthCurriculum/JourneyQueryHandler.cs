using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Abstractions;
using WordUp.Application.Common;

namespace WordUp.Application.YouthCurriculum;

public class JourneyQueryHandler : IRequestHandler<JourneyQuery, JourneyView>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public JourneyQueryHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<JourneyView> Handle(JourneyQuery request, CancellationToken cancellationToken)
    {
        var youthId = _currentUser.UserId ?? throw new ForbiddenException();

        IReadOnlyList<ModuleStatus> modules = Array.Empty<ModuleStatus>();
        var trackId = await CurriculumProgressCalculator.ResolveYouthTrackIdAsync(_db, youthId, cancellationToken);
        if (trackId is { } tid)
        {
            modules = await CurriculumProgressCalculator.BuildModuleStatusesAsync(_db, tid, youthId, cancellationToken);
        }

        var earned = await _db.BadgeAwards
            .Where(a => a.YouthUserId == youthId)
            .Join(_db.Badges, a => a.BadgeId, b => b.Id, (a, b) => new { a, b })
            .OrderByDescending(x => x.a.AwardedAt)
            .Select(x => new EarnedBadge(x.b.Id, x.b.Name, x.b.Icon, x.a.AwardedAt))
            .ToListAsync(cancellationToken);

        var earnedIds = earned.Select(e => e.BadgeId).ToHashSet();

        var locked = await _db.Badges
            .Where(b => b.ArchivedAt == null && !earnedIds.Contains(b.Id))
            .OrderBy(b => b.Name)
            .Select(b => new LockedBadge(
                b.Id,
                b.Name,
                b.Icon,
                b.Description ?? b.AutoRule ?? "Awarded by a mentor"))
            .ToListAsync(cancellationToken);

        return new JourneyView(modules, earned, locked);
    }
}
