using MediatR;

namespace Waypoint.Application.Authentication.Onboarding;

public record OnboardingStatusQuery : IRequest<OnboardingStatusResult>;
