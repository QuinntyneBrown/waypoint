using MediatR;

namespace WordUp.Application.Intake;

public record ApproveApplicationCommand(
    Guid ApplicationId,
    Guid CohortId,
    Guid MentorId,
    string? TrackInterest,
    string? DecisionNote) : IRequest<ApproveApplicationResult>;
