using MediatR;

namespace WordUp.Application.LearningExperience.CodingLessons;

public sealed record CodingLessonQuery(Guid LessonId) : IRequest<CodingLessonView>;

public sealed record CodingLessonView(
    Guid LessonId,
    Guid AssignmentId,
    string Title,
    string? Introduction,
    string Code,
    IReadOnlyList<CodingLessonStepView> Steps,
    IReadOnlyList<Guid> CheckedStepIds,
    int CompletionPercent,
    bool Completed,
    CodingLessonBadgeView? Badge,
    Guid? ExistingSubmissionId,
    string? ExistingSubmissionStatus);

public sealed record CodingLessonStepView(
    Guid Id,
    int Order,
    string Title,
    string Instructions,
    string CodeSample,
    string ChecklistLabel);

public sealed record CodingLessonBadgeView(
    Guid Id,
    string Name,
    string Icon,
    int CompletedCriteria,
    int TotalCriteria,
    bool Earned);
