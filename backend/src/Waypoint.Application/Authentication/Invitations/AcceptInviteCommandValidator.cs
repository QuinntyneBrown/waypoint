using FluentValidation;
using Waypoint.Application.Authentication.Auth;

namespace Waypoint.Application.Authentication.Invitations;

public class AcceptInviteCommandValidator : AbstractValidator<AcceptInviteCommand>
{
    public AcceptInviteCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Password).Must(PasswordPolicy.IsCompliant).WithMessage(PasswordPolicy.Message);
        RuleFor(x => x.AgreeToGuidelines).Equal(true)
            .WithMessage("You must agree to the community guidelines to continue.");
    }
}
