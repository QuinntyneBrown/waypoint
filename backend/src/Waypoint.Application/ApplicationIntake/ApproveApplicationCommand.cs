using MediatR;

namespace Waypoint.Application.ApplicationIntake;

public record ApproveApplicationCommand(
    Guid ApplicationId,
    Guid CohortId,
    Guid MentorId,
    string? TrackInterest,
    string? DecisionNote) : IRequest<ApproveApplicationResult>, IAtomicOutboxRequest;
