using MediatR;

namespace WordUp.Application.Engagement.Sessions;

public sealed record SessionOptionsQuery : IRequest<SessionSchedulingOptions>;

public sealed record SessionSchedulingOptions(
    IReadOnlyList<SessionCohortOption> Cohorts,
    IReadOnlyList<SessionMentorOption> Mentors);

public sealed record SessionCohortOption(
    Guid CohortId,
    string Name,
    DateTime TermStart,
    DateTime TermEnd);

public sealed record SessionMentorOption(Guid MentorId, string Name);
