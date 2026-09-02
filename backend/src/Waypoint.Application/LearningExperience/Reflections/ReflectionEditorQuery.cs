using MediatR;

namespace Waypoint.Application.LearningExperience.Reflections;

public sealed record ReflectionEditorQuery(Guid LessonId) : IRequest<ReflectionEditorView>;

public sealed record ReflectionEditorView(
    Guid LessonId,
    string LessonTitle,
    string? Prompt,
    ReflectionEntryView? Reflection);

public sealed record ReflectionEntryView(
    Guid Id,
    string? Text,
    string? Mood,
    IReadOnlyDictionary<string, string> SecondaryResponses,
    bool ShareWithMentor,
    bool IsDraft,
    DateTime UpdatedAt);
