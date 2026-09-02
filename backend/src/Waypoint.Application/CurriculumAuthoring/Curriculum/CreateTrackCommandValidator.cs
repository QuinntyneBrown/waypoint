using FluentValidation;

namespace Waypoint.Application.CurriculumAuthoring.Curriculum;

public class CreateTrackCommandValidator : AbstractValidator<CreateTrackCommand>
{
    public CreateTrackCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Key).NotEmpty().MaximumLength(64);
    }
}
