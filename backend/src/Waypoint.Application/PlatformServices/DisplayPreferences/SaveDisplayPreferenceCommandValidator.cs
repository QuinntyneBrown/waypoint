using FluentValidation;

namespace Waypoint.Application.PlatformServices.DisplayPreferences;

public class SaveDisplayPreferenceCommandValidator : AbstractValidator<SaveDisplayPreferenceCommand>
{
    public SaveDisplayPreferenceCommandValidator()
    {
        RuleFor(x => x.TextScalePercent)
            .InclusiveBetween(80, 200)
            .WithMessage("Text scale must be between 80% and 200%.");

        RuleFor(x => x.Theme)
            .Must(theme => theme is "system" or "light" or "dark")
            .WithMessage("Theme must be system, light, or dark.");
    }
}
