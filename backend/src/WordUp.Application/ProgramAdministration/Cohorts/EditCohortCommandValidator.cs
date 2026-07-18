using FluentValidation;

namespace WordUp.Application.ProgramAdministration.Cohorts;

public sealed class EditCohortCommandValidator : AbstractValidator<EditCohortCommand>
{
    public EditCohortCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(128);
        RuleFor(command => command.Location).MaximumLength(256);
    }
}
