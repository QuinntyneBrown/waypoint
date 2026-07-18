namespace WordUp.Api.Contracts.LearningExperience;

public record SaveReflectionRequest(Guid LessonId, string? Text, string? Mood, bool ShareWithMentor, bool IsDraft);
