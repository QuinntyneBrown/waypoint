using MediatR;

namespace Waypoint.Application.LearningExperience.CodingLessons;

public sealed record SaveCodingProgressCommand(
    Guid LessonId,
    string Code,
    IReadOnlyList<Guid> CheckedStepIds) : IRequest<CodingProgressResult>;

public sealed record CodingProgressResult(
    int CompletedCriteria,
    int TotalCriteria,
    int CompletionPercent,
    bool Completed,
    bool BadgeEarned);
