using MediatR;

namespace Waypoint.Application.ProgramAdministration.Cohorts;

public record CohortOptionsQuery : IRequest<CohortOptions>;

public record CohortOptions(
    IReadOnlyList<CohortOption> Tracks,
    IReadOnlyList<CohortOption> Mentors);

public record CohortOption(Guid Id, string Name);
