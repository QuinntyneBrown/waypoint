using MediatR;

namespace Waypoint.Application.Authentication.Onboarding;

public record CompleteOnboardingCommand(string DisplayName, string? Pronouns) : IRequest;
