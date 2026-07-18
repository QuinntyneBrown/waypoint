namespace WordUp.Api.Contracts.LearningExperience;

public record SaveReadingStateRequest(string? TextSize, bool Bookmarked, int Position);
