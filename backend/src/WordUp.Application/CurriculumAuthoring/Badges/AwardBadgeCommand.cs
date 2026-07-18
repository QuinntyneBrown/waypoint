using MediatR;

namespace WordUp.Application.CurriculumAuthoring.Badges;

public record AwardBadgeCommand(Guid BadgeId, Guid YouthUserId) : IRequest;
