using MediatR;

namespace WordUp.Application.Authentication.Onboarding;

public record CompleteOnboardingCommand(string DisplayName, string? Pronouns) : IRequest;
