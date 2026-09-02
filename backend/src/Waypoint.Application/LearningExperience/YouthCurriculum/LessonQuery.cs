using MediatR;

namespace Waypoint.Application.LearningExperience.YouthCurriculum;

public sealed record LessonQuery(Guid LessonId) : IRequest<LessonView>;

public sealed record LessonView(
    Guid LessonId,
    Guid ModuleId,
    string ModuleName,
    string Title,
    string? ScriptureReference,
    string? LearningObjectives,
    string? TeachingBody,
    string? ReflectionPrompts,
    int? EstimatedMinutes,
    int StepNumber,
    int TotalSteps,
    string Status,
    IReadOnlyList<LessonMediaView> Media,
    LessonReadingStateView ReadingState);

public sealed record LessonMediaView(Guid MediaId, string MediaType, string Url);

public sealed record LessonReadingStateView(string TextSize, bool Bookmarked, int Position);
