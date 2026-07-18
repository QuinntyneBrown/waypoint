using MediatR;

namespace WordUp.Application.Onboarding;

public record CompleteOnboardingCommand(string DisplayName, string? Pronouns, string? PhotoUrl) : IRequest;
