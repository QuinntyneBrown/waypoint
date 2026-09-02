using MediatR;

namespace Waypoint.Application.ProgramAdministration.Cohorts;

public record CreateCohortCommand(
    string Name,
    Guid TrackId,
    Guid LeadMentorId,
    DateTime? StartDate,
    DateTime? EndDate,
    string? Location) : IRequest<CohortResult>;

public record CohortResult(Guid Id);
