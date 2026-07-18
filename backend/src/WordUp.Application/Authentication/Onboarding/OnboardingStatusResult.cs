namespace WordUp.Application.Authentication.Onboarding;

public record OnboardingStatusResult(
    string DisplayName,
    string? Pronouns,
    Guid? CohortId,
    Guid? MentorId,
    bool OnboardingCompleted);
