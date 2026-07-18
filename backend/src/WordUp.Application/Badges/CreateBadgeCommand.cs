using MediatR;
using WordUp.Domain;

namespace WordUp.Application.Badges;

public record CreateBadgeCommand(
    string Name,
    string Icon,
    string? Description,
    BadgeCriteriaKind CriteriaKind,
    string? AutoRule,
    Guid? CompetencyId) : IRequest<BadgeResult>;
