using FluentValidation;

namespace WordUp.Application.ConsentAndSafeguarding;

public sealed class CaptureRequestedConsentCommandValidator
    : AbstractValidator<CaptureRequestedConsentCommand>
{
    public CaptureRequestedConsentCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.GuardianName).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Relationship).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Signature).NotEmpty().MaximumLength(128);
        RuleFor(x => x.ParticipationConsent).Equal(true)
            .WithMessage("Participation consent is required.");
    }
}
