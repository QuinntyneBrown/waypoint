using MediatR;

namespace WordUp.Application.Badges;

public record DeleteBadgeCommand(Guid Id) : IRequest;
