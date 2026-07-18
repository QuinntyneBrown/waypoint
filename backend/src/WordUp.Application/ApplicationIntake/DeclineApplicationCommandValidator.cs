using FluentValidation;

namespace WordUp.Application.ApplicationIntake;

public sealed class DeclineApplicationCommandValidator : AbstractValidator<DeclineApplicationCommand>
{
    public DeclineApplicationCommandValidator()
    {
        RuleFor(command => command.ApplicationId).NotEmpty();
        RuleFor(command => command.DecisionNote).MaximumLength(2000);
    }
}
