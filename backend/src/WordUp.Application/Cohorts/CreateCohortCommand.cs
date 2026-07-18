using MediatR;

namespace WordUp.Application.Cohorts;

public record CreateCohortCommand(
    string Name,
    Guid TrackId,
    Guid LeadMentorId,
    DateTime? StartDate,
    DateTime? EndDate,
    string? Location) : IRequest<CohortResult>;

public record CohortResult(Guid Id);
