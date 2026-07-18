using FluentValidation;

namespace WordUp.Application.Authentication.Onboarding;

public class CompleteOnboardingCommandValidator : AbstractValidator<CompleteOnboardingCommand>
{
    public CompleteOnboardingCommandValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Pronouns).MaximumLength(64);
        RuleFor(x => x.PhotoUrl).MaximumLength(512);
    }
}
