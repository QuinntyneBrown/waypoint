using MediatR;

namespace WordUp.Application.Cohorts;

public record ListCohortsQuery : IRequest<IReadOnlyList<CohortSummary>>;

public record CohortSummary(Guid Id, string Name, Guid? TrackId, Guid? MentorId, int MemberCount);
