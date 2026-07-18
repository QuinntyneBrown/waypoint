namespace WordUp.Api.Contracts.Authentication;

public record CompleteOnboardingRequest(string DisplayName, string? Pronouns, string? PhotoUrl);
