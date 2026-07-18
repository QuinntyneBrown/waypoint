using MediatR;

namespace WordUp.Application.ApplicationIntake;

public record DeclineApplicationCommand(Guid ApplicationId, string? DecisionNote) : IRequest;
