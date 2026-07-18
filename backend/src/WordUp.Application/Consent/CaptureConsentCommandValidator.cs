using FluentValidation;

namespace WordUp.Application.Consent;

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
