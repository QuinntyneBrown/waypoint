using FluentValidation;
using Waypoint.Application.Authentication.Auth;

namespace Waypoint.Application.Authentication.PasswordReset;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword).Must(PasswordPolicy.IsCompliant).WithMessage(PasswordPolicy.Message);
        RuleFor(x => x.ConfirmPassword).Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
    }
}
