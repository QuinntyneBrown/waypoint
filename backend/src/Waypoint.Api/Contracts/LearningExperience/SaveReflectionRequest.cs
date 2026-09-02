namespace Waypoint.Api.Contracts.LearningExperience;

public record SaveReflectionRequest(
    Guid LessonId,
    string? Text,
    string? Mood,
    IReadOnlyDictionary<string, string>? SecondaryResponses,
    bool ShareWithMentor,
    bool IsDraft);
