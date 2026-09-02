using MediatR;

namespace Waypoint.Application.CurriculumAuthoring.Badges;

public record DeleteBadgeCommand(Guid Id) : IRequest;
