using FluentValidation;

namespace Waypoint.Application.LearningExperience.CodingLessons;

public sealed class SaveCodingProgressCommandValidator : AbstractValidator<SaveCodingProgressCommand>
{
    public SaveCodingProgressCommandValidator()
    {
        RuleFor(item => item.Code).NotNull().MaximumLength(50000);
        RuleFor(item => item.CheckedStepIds).NotNull();
    }
}
