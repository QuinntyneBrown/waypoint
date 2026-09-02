using MediatR;

namespace Waypoint.Application.CurriculumAuthoring.Badges;

public record CreateBadgeCommand(
    string Name,
    string Icon,
    string? Description,
    BadgeCriteriaKind CriteriaKind,
    string? AutoRule,
    Guid? TargetRefId,
    Guid? CompetencyId) : IRequest<BadgeResult>;
