using FluentValidation;

namespace Waypoint.Application.Mentoring.Flags;

public class RaiseFlagCommandValidator : AbstractValidator<RaiseFlagCommand>
{
    public RaiseFlagCommandValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}
