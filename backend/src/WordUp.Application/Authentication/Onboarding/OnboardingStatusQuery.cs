using MediatR;

namespace WordUp.Application.Authentication.Onboarding;

public record OnboardingStatusQuery : IRequest<OnboardingStatusResult>;
