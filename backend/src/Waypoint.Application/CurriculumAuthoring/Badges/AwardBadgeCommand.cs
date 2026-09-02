using MediatR;

namespace Waypoint.Application.CurriculumAuthoring.Badges;

public record AwardBadgeCommand(Guid BadgeId, Guid YouthUserId) : IRequest;
