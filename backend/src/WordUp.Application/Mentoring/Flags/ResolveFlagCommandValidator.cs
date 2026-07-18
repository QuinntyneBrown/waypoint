using FluentValidation;

namespace WordUp.Application.Mentoring.Flags;

public class ResolveFlagCommandValidator : AbstractValidator<ResolveFlagCommand>
{
    public ResolveFlagCommandValidator()
    {
        RuleFor(x => x.ResolutionReason)
            .NotEmpty()
            .Must(FlagResolutionReasons.IsValid)
            .WithMessage("Unknown resolution reason.");
    }
}
