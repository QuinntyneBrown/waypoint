using MediatR;

namespace WordUp.Application.Intake;

public record DeclineApplicationCommand(Guid ApplicationId, string? DecisionNote) : IRequest;
