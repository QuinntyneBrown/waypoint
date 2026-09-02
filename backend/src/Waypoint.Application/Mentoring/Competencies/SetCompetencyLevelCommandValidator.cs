using FluentValidation;

namespace Waypoint.Application.Mentoring.Competencies;

public sealed class SetCompetencyLevelCommandValidator : AbstractValidator<SetCompetencyLevelCommand>
{
    public SetCompetencyLevelCommandValidator()
    {
        RuleFor(command => command.Notes)
            .MaximumLength(2000);
    }
}
