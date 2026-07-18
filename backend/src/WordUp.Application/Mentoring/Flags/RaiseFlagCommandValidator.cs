using FluentValidation;

namespace WordUp.Application.Mentoring.Flags;

public class RaiseFlagCommandValidator : AbstractValidator<RaiseFlagCommand>
{
    public RaiseFlagCommandValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}
