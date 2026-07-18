using FluentValidation;

namespace WordUp.Application.GuardianPortal;

public class UpdateGuardianContactCommandValidator : AbstractValidator<UpdateGuardianContactCommand>
{
    public UpdateGuardianContactCommandValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Relationship).MaximumLength(64);
        RuleFor(x => x.Phone).MaximumLength(32);
        RuleFor(x => x.EmergencyContactName).MaximumLength(128);
        RuleFor(x => x.EmergencyContactPhone).MaximumLength(32);
    }
}
