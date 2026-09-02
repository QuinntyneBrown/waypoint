using FluentValidation;

namespace Waypoint.Application.LearningExperience.Submissions;

public sealed class ResubmitCommandValidator : AbstractValidator<ResubmitCommand>
{
    public ResubmitCommandValidator()
    {
        RuleFor(x => x.ExpectedVersion)
            .NotEmpty()
            .Must(value => Guid.TryParse(value, out _))
            .WithMessage("The submission version is required.");
    }
}
