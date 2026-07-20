using MediatR;

namespace WordUp.Application.ApplicationIntake;

public record ApproveApplicationCommand(
    Guid ApplicationId,
    Guid CohortId,
    Guid MentorId,
    string? TrackInterest,
    string? DecisionNote) : IRequest<ApproveApplicationResult>, IAtomicOutboxRequest;
