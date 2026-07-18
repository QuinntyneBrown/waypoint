using MediatR;

namespace WordUp.Application.ApplicationIntake;

public sealed record ApplicationReviewOptionsQuery : IRequest<ApplicationReviewOptions>;

public sealed record ApplicationReviewOptions(
    IReadOnlyList<ApplicationOption> Cohorts,
    IReadOnlyList<ApplicationMentorOption> Mentors,
    IReadOnlyList<ApplicationOption> Tracks);

public sealed record ApplicationOption(Guid Id, string Name);
public sealed record ApplicationMentorOption(Guid Id, string Name, IReadOnlyList<Guid> CohortIds);
