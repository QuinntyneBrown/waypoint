using FluentValidation;
using WordUp.Domain;

namespace WordUp.Application.Invitations;

public class InviteCommandValidator : AbstractValidator<InviteCommand>
{
    public InviteCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Role).NotEmpty().Must(Roles.IsValid).WithMessage("Unknown role.");
    }
}
