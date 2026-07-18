namespace WordUp.Application.Onboarding;

public record OnboardingStatusResult(
    string DisplayName,
    string? Pronouns,
    Guid? CohortId,
    Guid? MentorId,
    bool OnboardingCompleted);
