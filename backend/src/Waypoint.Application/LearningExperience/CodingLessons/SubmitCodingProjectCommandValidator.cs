using FluentValidation;

namespace Waypoint.Application.LearningExperience.CodingLessons;

public sealed class SubmitCodingProjectCommandValidator : AbstractValidator<SubmitCodingProjectCommand>
{
    public SubmitCodingProjectCommandValidator()
    {
        RuleFor(item => item.Code).NotEmpty().MaximumLength(50000);
        RuleFor(item => item.MentorNote).MaximumLength(1000);
    }
}
