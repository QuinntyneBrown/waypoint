using FluentValidation;

namespace Waypoint.Application.ProgramAdministration.Settings;

public class UpdateOrgSettingsCommandValidator : AbstractValidator<UpdateOrgSettingsCommand>
{
    public UpdateOrgSettingsCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.DisplayName).MaximumLength(128);
        RuleFor(x => x.LogoUrl).MaximumLength(2048);
        RuleFor(x => x.City).MaximumLength(128);
        RuleFor(x => x.ConsentStatement).MaximumLength(4000);
        RuleFor(x => x.ContactEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
    }
}
