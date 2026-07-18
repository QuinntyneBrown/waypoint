using FluentValidation;

namespace WordUp.Application.DisplayPreferences;

public class SaveDisplayPreferenceCommandValidator : AbstractValidator<SaveDisplayPreferenceCommand>
{
    public SaveDisplayPreferenceCommandValidator()
    {
        RuleFor(x => x.TextScalePercent)
            .InclusiveBetween(80, 200)
            .WithMessage("Text scale must be between 80% and 200%.");
    }
}
