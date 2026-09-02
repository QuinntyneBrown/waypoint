using FluentValidation;

namespace Waypoint.Application.Authentication.Invitations;

public class InviteCommandValidator : AbstractValidator<InviteCommand>
{
    public InviteCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Role).NotEmpty().Must(Roles.IsValid).WithMessage("Unknown role.");
        RuleFor(x => x.CohortId).NotEqual(Guid.Empty).When(x => x.CohortId is not null);
        RuleFor(x => x.MentorId).NotEqual(Guid.Empty).When(x => x.MentorId is not null);
    }
}
