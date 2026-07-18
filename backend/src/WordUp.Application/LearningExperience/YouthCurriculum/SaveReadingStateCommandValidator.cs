using FluentValidation;

namespace WordUp.Application.LearningExperience.YouthCurriculum;

public sealed class SaveReadingStateCommandValidator : AbstractValidator<SaveReadingStateCommand>
{
    private static readonly string[] TextSizes = ["small", "medium", "large"];

    public SaveReadingStateCommandValidator()
    {
        RuleFor(command => command.TextSize).Must(size => size is null || TextSizes.Contains(size));
        RuleFor(command => command.Position).GreaterThanOrEqualTo(0);
    }
}
