namespace Waypoint.Api.Contracts.LearningExperience;

public record UpdateReflectionRequest(
    string? Text,
    string? Mood,
    IReadOnlyDictionary<string, string>? SecondaryResponses,
    bool ShareWithMentor,
    bool IsDraft);
