using MediatR;

namespace Waypoint.Application.ProgramAdministration.Cohorts;

public record GetCohortQuery(Guid Id) : IRequest<CohortDetail>;

public record CohortDetail(
    Guid Id,
    string Name,
    Guid? TrackId,
    string? TrackName,
    Guid? LeadMentorId,
    string? LeadMentorName,
    DateTime StartDate,
    DateTime EndDate,
    string? Location,
    int MemberCount,
    IReadOnlyList<CohortYouthSummary> Members,
    IReadOnlyList<CohortYouthSummary> AvailableYouth);

public record CohortYouthSummary(Guid Id, string Name, string Email, string Status);
