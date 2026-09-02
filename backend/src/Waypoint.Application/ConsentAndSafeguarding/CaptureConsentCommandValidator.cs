using FluentValidation;

namespace Waypoint.Application.ConsentAndSafeguarding;

public class CaptureConsentCommandValidator : AbstractValidator<CaptureConsentCommand>
{
    public CaptureConsentCommandValidator()
    {
        RuleFor(x => x.GuardianName).NotEmpty();
        RuleFor(x => x.Relationship).NotEmpty();
        RuleFor(x => x.Signature).NotEmpty();
        RuleFor(x => x.ParticipationConsent).Equal(true)
            .WithMessage("Participation consent is required.");
    }
}
