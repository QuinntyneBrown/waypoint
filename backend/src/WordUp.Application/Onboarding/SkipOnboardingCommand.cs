using MediatR;

namespace WordUp.Application.Onboarding;

/// <summary>Skips onboarding for now (L2-AUTH-011 AC4): the youth proceeds to home and can resume later,
/// so onboarding is NOT marked complete.</summary>
public record SkipOnboardingCommand : IRequest;
