using MediatR;

namespace WordUp.Application.CurriculumAuthoring.Curriculum;

public record GetLessonQuery(Guid Id) : IRequest<LessonDetail>;

public record LessonDetail(
    Guid Id,
    Guid ModuleId,
    string Title,
    string? ScriptureReference,
    int? EstimatedMinutes,
    string? LearningObjectives,
    string? TeachingBody,
    string? ReflectionPrompts,
    string? LeaderGuideNotes,
    string Status);
