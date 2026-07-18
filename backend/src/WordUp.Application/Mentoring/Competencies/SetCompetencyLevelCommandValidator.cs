using FluentValidation;

namespace WordUp.Application.Mentoring.Competencies;

public sealed class SetCompetencyLevelCommandValidator : AbstractValidator<SetCompetencyLevelCommand>
{
    public SetCompetencyLevelCommandValidator()
    {
        RuleFor(command => command.Notes)
            .MaximumLength(2000);
    }
}
