using FluentValidation;
using WordUp.Application.Auth;

namespace WordUp.Application.PasswordReset;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword).Must(PasswordPolicy.IsCompliant).WithMessage(PasswordPolicy.Message);
        RuleFor(x => x.ConfirmPassword).Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
    }
}
