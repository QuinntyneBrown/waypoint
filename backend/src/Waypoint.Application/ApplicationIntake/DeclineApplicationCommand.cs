using MediatR;

namespace Waypoint.Application.ApplicationIntake;

public record DeclineApplicationCommand(Guid ApplicationId, string? DecisionNote)
    : IRequest, IAtomicOutboxRequest;
