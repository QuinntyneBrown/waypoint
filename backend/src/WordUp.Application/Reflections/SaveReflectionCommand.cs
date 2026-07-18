using MediatR;

namespace WordUp.Application.Reflections;

/// <summary>Creates or updates the current youth's reflection for a lesson (L2-LEARN-003).
/// One reflection per youth+lesson; a draft is upserted in place.</summary>
public record SaveReflectionCommand(
    Guid LessonId,
    string? Text,
    string? Mood,
    bool ShareWithMentor,
    bool IsDraft) : IRequest<ReflectionResult>;

public record ReflectionResult(Guid Id);
