using MediatR;

namespace WordUp.Application.Onboarding;

public record OnboardingStatusQuery : IRequest<OnboardingStatusResult>;
