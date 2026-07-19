using MediatR;
using Microsoft.EntityFrameworkCore;
using WordUp.Application.Shared.Abstractions;

namespace WordUp.Application.CurriculumAuthoring.Badges;

public sealed class ListBadgesQueryHandler
    : IRequestHandler<ListBadgesQuery, IReadOnlyList<BadgeDefinitionView>>
{
    private readonly IAppDbContext _db;

    public ListBadgesQueryHandler(IAppDbContext db) => _db = db;

    public async Task<IReadOnlyList<BadgeDefinitionView>> Handle(
        ListBadgesQuery request,
        CancellationToken cancellationToken)
    {
        var badges = await _db.Badges
            .AsNoTracking()
            .Where(item => item.ArchivedAt == null)
            .OrderBy(item => item.Name)
            .ThenBy(item => item.Id)
            .ToListAsync(cancellationToken);

        var competencyIds = badges
            .Where(item => item.CompetencyId.HasValue)
            .Select(item => item.CompetencyId!.Value)
            .Distinct()
            .ToList();
        var competencyNames = await _db.Competencies
            .AsNoTracking()
            .Where(item => competencyIds.Contains(item.Id))
            .ToDictionaryAsync(item => item.Id, item => item.Name, cancellationToken);
        var badgeIds = badges.Select(item => item.Id).ToList();
        var awardCounts = await _db.BadgeAwards
            .AsNoTracking()
            .Where(item => badgeIds.Contains(item.BadgeId))
            .GroupBy(item => item.BadgeId)
            .Select(group => new { BadgeId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.BadgeId, item => item.Count, cancellationToken);

        return badges.Select(item => new BadgeDefinitionView(
                item.Id,
                item.Name,
                item.Icon,
                item.Description,
                item.CriteriaKind.ToString(),
                item.AutoRule,
                item.TargetRefId,
                item.CompetencyId,
                item.CompetencyId is { } competencyId
                    && competencyNames.TryGetValue(competencyId, out var competencyName)
                        ? competencyName
                        : null,
                awardCounts.GetValueOrDefault(item.Id)))
            .ToArray();
    }
}
