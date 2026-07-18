using MediatR;

namespace WordUp.Application.Curriculum;

public record CreateLessonCommand(
    Guid ModuleId,
    string Title,
    string? ScriptureReference,
    int? EstimatedMinutes,
    string? LearningObjectives,
    string? TeachingBody,
    string? ReflectionPrompts,
    string? LeaderGuideNotes) : IRequest<LessonResult>;

public record LessonResult(Guid Id);
