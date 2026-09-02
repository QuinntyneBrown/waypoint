using FluentValidation;
using Waypoint.Application.Authentication.Auth;

namespace Waypoint.Application.PlatformServices.Profile;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        // At least 8 characters including at least one letter and one digit (L2-AUTH-008).
        RuleFor(x => x.NewPassword)
            .Must(PasswordPolicy.IsCompliant)
            .WithMessage(PasswordPolicy.Message);

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword)
            .WithMessage("The confirmation password does not match the new password.");
    }
}
