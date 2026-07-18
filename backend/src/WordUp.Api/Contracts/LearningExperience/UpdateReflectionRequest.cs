namespace WordUp.Api.Contracts.LearningExperience;

public record UpdateReflectionRequest(string? Text, string? Mood, bool ShareWithMentor, bool IsDraft);
