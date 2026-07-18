using MediatR;

namespace WordUp.Application.CurriculumAuthoring.Badges;

public record DeleteBadgeCommand(Guid Id) : IRequest;
