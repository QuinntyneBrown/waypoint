using FluentValidation;

namespace WordUp.Application.Settings;

public class UpdateOrgSettingsCommandValidator : AbstractValidator<UpdateOrgSettingsCommand>
{
    public UpdateOrgSettingsCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.ContactEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
    }
}
