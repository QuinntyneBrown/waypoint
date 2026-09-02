using MediatR;

namespace Waypoint.Application.CurriculumAuthoring.Badges;

public sealed record ListBadgesQuery : IRequest<IReadOnlyList<BadgeDefinitionView>>;

public sealed record BadgeDefinitionView(
    Guid Id,
    string Name,
    string Icon,
    string? Description,
    string CriteriaKind,
    string? AutoRule,
    Guid? TargetRefId,
    Guid? CompetencyId,
    string? CompetencyName,
    int AwardedCount);
