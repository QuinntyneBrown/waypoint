namespace WordUp.Api.Contracts.CurriculumAuthoring;

public record CreateLessonRequest(
    Guid ModuleId,
    string Title,
    string? ScriptureReference,
    int? EstimatedMinutes,
    string? LearningObjectives,
    string? TeachingBody,
    string? ReflectionPrompts,
    string? LeaderGuideNotes);
