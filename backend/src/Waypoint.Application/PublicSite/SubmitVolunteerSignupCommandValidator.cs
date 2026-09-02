using FluentValidation;

namespace Waypoint.Application.PublicSite;

public class SubmitVolunteerSignupCommandValidator : AbstractValidator<SubmitVolunteerSignupCommand>
{
    public SubmitVolunteerSignupCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Phone).MaximumLength(64);
        RuleFor(x => x.AreasOfInterest).MaximumLength(2000);

        // A volunteer request must record the screening-consent acknowledgement, and is blocked without it
        // (L2-PUB-003 AC2).
        RuleFor(x => x.ScreeningConsentAck).Equal(true)
            .WithMessage("You must acknowledge that volunteers working with youth complete a screening and reference check.");
    }
}
