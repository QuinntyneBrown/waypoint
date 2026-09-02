using FluentValidation;

namespace Waypoint.Application.Authentication.Invitations;

public sealed class BulkInviteCommandValidator : AbstractValidator<BulkInviteCommand>
{
    public BulkInviteCommandValidator()
    {
        RuleFor(item => item.Emails)
            .NotNull()
            .Must(items => items is { Count: > 0 and <= 200 })
            .WithMessage("Provide between 1 and 200 email addresses.");
        RuleForEach(item => item.Emails)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);
        RuleFor(item => item.Role).NotEmpty().Must(Roles.IsValid).WithMessage("Unknown role.");
        RuleFor(item => item.CohortId).NotEqual(Guid.Empty).When(item => item.CohortId is not null);
        RuleFor(item => item.MentorId).NotEqual(Guid.Empty).When(item => item.MentorId is not null);
    }
}
