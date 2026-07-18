using MediatR;

namespace WordUp.Application.ProgramAdministration.Cohorts;

public record ListCohortsQuery : IRequest<IReadOnlyList<CohortSummary>>;

public record CohortSummary(
    Guid Id,
    string Name,
    Guid? TrackId,
    string? TrackName,
    Guid? MentorId,
    string? MentorName,
    DateTime StartDate,
    DateTime EndDate,
    string? Location,
    int MemberCount);
