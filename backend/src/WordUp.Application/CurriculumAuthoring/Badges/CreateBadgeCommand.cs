using MediatR;

namespace WordUp.Application.CurriculumAuthoring.Badges;

public record CreateBadgeCommand(
    string Name,
    string Icon,
    string? Description,
    BadgeCriteriaKind CriteriaKind,
    string? AutoRule,
    Guid? CompetencyId) : IRequest<BadgeResult>;
