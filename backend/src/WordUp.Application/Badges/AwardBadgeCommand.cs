using MediatR;

namespace WordUp.Application.Badges;

public record AwardBadgeCommand(Guid BadgeId, Guid YouthUserId) : IRequest;
